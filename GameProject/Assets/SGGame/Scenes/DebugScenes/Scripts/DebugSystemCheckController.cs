using System;
using Cysharp.Threading.Tasks;
using SGGames.Game.Sys;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace SGGames.Game.Develop
{
    public sealed class DebugSystemCheckController : MonoBehaviour
    {
        [SerializeField] TMP_FontAsset _font;
        public bool IsOpen { get; private set; }

        public void Open()
        {
#if !IS_PRODUCT
            if (_panel == null) CreatePanel();
            _panel.SetActive(true);
            IsOpen = true;
            CheckStartup();
#endif
        }

#if !IS_PRODUCT
        static string s_TransitionResult = "未実行";
        static bool s_Reloading;
        static bool s_Reopen;
        IDisposable _blockA;
        IDisposable _blockB;
        GameObject _panel;
        TextMeshProUGUI _status;
        string _startupResult = "未確認";
        string _popupResult = "未実行";
        int _acceptedInputs;
        bool _popupBusy;
        bool _destroyPopupRequested;
        DebugSystemCheckPopup _popup;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetChecks()
        {
            s_TransitionResult = "未実行";
            s_Reloading = false;
            s_Reopen = false;
        }

        void Start()
        {
            if (s_Reopen) Open();
        }

        void Update()
        {
            if (!IsOpen) return;
            var input = IPlayerInputManager.Instance;
            if (!_popupBusy && input != null)
            {
                var ui = input.UIAction;
                if (ui.AxisUp || ui.AxisDown || ui.AxisLeft || ui.AxisRight || ui.Decide || ui.Cancel)
                {
                    _acceptedInputs++;
                }
            }
            _status.text = $"起動: {_startupResult}\n"
                + $"遷移: {s_TransitionResult}\n"
                + $"入力停止: {(input == null ? "Managerなし" : input.IsInputBlocked ? "停止中" : "受付中")}\n"
                + $"要求 A: {(_blockA != null ? "保持" : "なし")} / B: {(_blockB != null ? "保持" : "なし")}\n"
                + $"受付回数: {_acceptedInputs}（方向キー・決定・取消）\n"
                + $"Popup: {_popupResult}";
        }

        void OnDestroy()
        {
            ReleaseBlocks();
            if (_popup != null) Destroy(_popup.gameObject);
        }

        void CheckStartup()
        {
            var scopes = FindObjectsByType<PersistentSceneLifetimeScope>(FindObjectsInactive.Include);
            bool ready = scopes.Length == 1 && scopes[0].IsInitialized
                && IsSingleManager<CameraManager>(ICameraManager.Instance)
                && IsSingleManager<PlayerInputManager>(IPlayerInputManager.Instance)
                && IsSingleManager<WindowManager>(IWindowManager.Instance)
                && IsSingleManager<SceneTransitionManager>(ISceneTransitionManager.Instance)
                && IsSingleManager<GameManager>(IGameManager.Instance);
#if GAME_DEBUG
            ready &= IsSingleManager<DebugSystemManager>(IDebugSystemManager.Instance);
#endif
            _startupResult = ready ? "OK: 常駐初期化済み・必須Manager各1個" : "NG: BootSceneから起動し、Consoleも確認";
        }

        static bool IsSingleManager<T>(object service) where T : MonoBehaviour
        {
            var managers = FindObjectsByType<T>(FindObjectsInactive.Include);
            return managers.Length == 1 && managers[0].isActiveAndEnabled && ReferenceEquals(service, managers[0]);
        }

        void ToggleBlock(bool isA)
        {
            if (_popupBusy || s_Reloading) return;
            var input = IPlayerInputManager.Instance;
            if (input == null) return;
            if (isA)
            {
                if (_blockA == null) _blockA = input.AcquireInputBlock();
                else { _blockA.Dispose(); _blockA = null; }
            }
            else
            {
                if (_blockB == null) _blockB = input.AcquireInputBlock();
                else { _blockB.Dispose(); _blockB = null; }
            }
        }

        void ReleaseBlocks()
        {
            _blockA?.Dispose();
            _blockA = null;
            _blockB?.Dispose();
            _blockB = null;
        }

        void RequestReload()
        {
            if (_popupBusy || _blockA != null || _blockB != null || s_Reloading) return;
            var transition = ISceneTransitionManager.Instance;
            var input = IPlayerInputManager.Instance;
            if (transition == null || input == null || input.IsInputBlocked || transition.IsProcessingSceneChange) return;
            ReloadScene(transition, input).Forget();
        }

        // シーンの破棄をまたぐ検証なので、旧シーンのComponentを保持しない。
        static async UniTask ReloadScene(ISceneTransitionManager transition, IPlayerInputManager input)
        {
            s_Reloading = true;
            s_Reopen = true;
            s_TransitionResult = "再読み込み中…";
            var camera = ICameraManager.Instance;
            var window = IWindowManager.Instance;
            var game = IGameManager.Instance;
#if GAME_DEBUG
            var debug = IDebugSystemManager.Instance;
#endif
            IDisposable block = null;
            try
            {
                block = input.AcquireInputBlock();
                await transition.RequestSceneChange("DebugTopScene");
                bool retained = ReferenceEquals(input, IPlayerInputManager.Instance)
                    && ReferenceEquals(transition, ISceneTransitionManager.Instance)
                    && ReferenceEquals(camera, ICameraManager.Instance)
                    && ReferenceEquals(window, IWindowManager.Instance)
                    && ReferenceEquals(game, IGameManager.Instance);
#if GAME_DEBUG
                retained &= ReferenceEquals(debug, IDebugSystemManager.Instance);
#endif
                bool stillBlocked = input.IsInputBlocked;
                block.Dispose();
                block = null;
                s_TransitionResult = retained && stillBlocked && !input.IsInputBlocked && !transition.IsProcessingSceneChange
                    ? "OK: 常駐維持・遷移後も独立要求を維持・解除成功"
                    : "NG: 常駐同一性または入力停止の状態不一致";
            }
            catch (Exception exception)
            {
                s_TransitionResult = "NG: " + exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                block?.Dispose();
                s_Reloading = false;
            }
        }

        async UniTask OpenPopup()
        {
            var windows = IWindowManager.Instance;
            var input = IPlayerInputManager.Instance;
            if (_popupBusy || s_Reloading || windows == null || input == null || input.IsInputBlocked) return;
            if (ISceneTransitionManager.Instance?.IsProcessingSceneChange == true) return;
            _popupBusy = true;
            _destroyPopupRequested = false;
            _popupResult = "ロード中…";
            var cancelToken = gameObject.GetCancellationTokenOnDestroy();
            try
            {
                _popup = await windows.CreateWindow<DebugSystemCheckPopup>(DebugSystemCheckPopup.AssetAddress, popup =>
                {
                    cancelToken.ThrowIfCancellationRequested();
                    return UniTask.CompletedTask;
                });
                cancelToken.ThrowIfCancellationRequested();
                if (_popup == null) throw new InvalidOperationException("Popupアセットを取得できませんでした。");
                _popupResult = "表示中: 右の「閉じる」を選択・決定";
                var result = await _popup.Run();
                await UniTask.DelayFrame(1, cancellationToken: cancelToken);
                _popupResult = _popup != null ? "NG: 終了後もPopupが残っています"
                    : result == (1, "close") && !_destroyPopupRequested ? "OK: 選択ID (1, close) を返して終了"
                    : result == (-1, "") && _destroyPopupRequested ? "OK: 破棄により未選択で終了" : $"NG: 予期しない結果 {result}";
            }
            catch (OperationCanceledException) when (cancelToken.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                _popupResult = "NG: " + exception.Message;
                Debug.LogException(exception);
            }
            finally
            {
                if (_popup != null) Destroy(_popup.gameObject);
                _popup = null;
                _popupBusy = false;
            }
        }

        void ClosePanel()
        {
            if (_popupBusy || s_Reloading) return;
            ReleaseBlocks();
            IsOpen = false;
            s_Reopen = false;
            _panel.SetActive(false);
        }

        void CreatePanel()
        {
            if (EventSystem.current == null)
            {
                var events = new GameObject("Debug EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                events.GetComponent<EventSystem>().sendNavigationEvents = false;
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            var canvasObject = new GameObject("System Checks", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            _panel = canvasObject;
            var background = CreateRect("Panel", canvasObject.transform, new Vector2(24, -24), new Vector2(640, 990));
            background.gameObject.AddComponent<Image>().color = new Color(0.04f, 0.06f, 0.09f, 0.97f);
            CreateText(background, "基盤システムチェック", 18, 26, 45);
            _status = CreateText(background, "", 74, 20, 260);
            CreateText(background, "1. A取得 → B取得 → A解除でも停止を維持\n2. B解除で入力受付が再開することを確認\n操作ボタンは停止解除用にマウスで操作できます。", 340, 20, 110);
            CreateButton(background, "入力停止 A 取得 / 解除", 458, () => ToggleBlock(true));
            CreateButton(background, "入力停止 B 取得 / 解除", 518, () => ToggleBlock(false));
            CreateButton(background, "デバッグシーン再読み込み", 578, RequestReload);
            CreateButton(background, "Popupを開く", 638, () => OpenPopup().Forget());
            CreateButton(background, "表示中のPopupを破棄", 698, () =>
            {
                if (_popup == null) return;
                _destroyPopupRequested = true;
                Destroy(_popup.gameObject);
            });
            CreateButton(background, "起動状態を再確認", 758, CheckStartup);
            CreateButton(background, "メニューへ戻る（停止要求を解除）", 818, ClosePanel);
            CreateText(background, "Popup: 方向キーで選択、Space / ゲームパッドで決定。\n再読み込み・Popupは入力停止を解除してから実行。", 886, 18, 84);
        }

        static RectTransform CreateRect(string name, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            return rect;
        }

        TextMeshProUGUI CreateText(Transform parent, string value, float top, int fontSize, float height)
        {
            var text = CreateRect("Text", parent, new Vector2(18, -top), new Vector2(604, height)).gameObject.AddComponent<TextMeshProUGUI>();
            text.font = _font;
            text.fontSize = fontSize;
            text.text = value;
            text.raycastTarget = false;
            return text;
        }

        void CreateButton(Transform parent, string label, float top, UnityEngine.Events.UnityAction action)
        {
            var rect = CreateRect(label, parent, new Vector2(18, -top), new Vector2(604, 48));
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.15f, 0.23f, 0.32f);
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(action);
            var text = CreateText(rect, label, 5, 22, 40);
            text.rectTransform.sizeDelta = new Vector2(568, 40);
        }
#endif
    }
}
