//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     WindowManager.cs
 *    @brief    Window管理
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;

using Cysharp.Threading.Tasks;
using DG.Tweening;
using R3;
using R3.Triggers;

using SGSys;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       フェード用Imageに設定する色.
     */
    //==========================================================================
    public enum FadeColors
    {
        Black,
        White,
    }

    //==========================================================================
    /**
     *    @brief       フェード幕の表示方向.
     */
    //==========================================================================
    public enum FadeTypes
    {
        FadeIn,
        FadeOut,
    }

    //==========================================================================
    /**
     *    @brief       フェード幕を配置するUIレイヤーの高さ.
     */
    //==========================================================================
    public enum FadePriorities
    {
        FullScreen,
        UnderLoadingUI,
        UnderHUD,
    }

    //==========================================================================
    /**
     *    @brief       Window関連システムの公開窓口.
     */
    //==========================================================================
    public interface IWindowManager : IService<IWindowManager>
    {
        void SetNormalWindow(NormalWindow window);
        UniTask RequestFade(FadeColors fadeColor, FadeTypes fadeType, int durationMilliseconds, FadePriorities priority);
        UniTask<TWindow> CreateWindow<TWindow>(object assetAddress, System.Func<TWindow, UniTask> onInitialize) where TWindow : WindowBase;
        UniTask CloseWindow(WindowBase window);
    }

    //==========================================================================
    /**
     *    @brief       PopupWindowの生成・破棄、入力Map切り替え、画面フェードを管理する.
     */
    //==========================================================================
    [DefaultExecutionOrder(-100000)]
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        #region Singleton
        private static WindowManager Instance => IWindowManager.Instance as WindowManager;

        private void Awake()
        {
            UnityEngine.Object currentManager = IWindowManager.Instance as UnityEngine.Object;
            if (currentManager != null && currentManager != this)
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator<IWindowManager>.Register(this);
            Initialize();
        }

        private void OnDestroy()
        {
            UnityEngine.Object currentManager = IWindowManager.Instance as UnityEngine.Object;
            if (currentManager == this)
            {
                ServiceLocator<IWindowManager>.Unregister();
            }
        }
        #endregion

        [Header("参照")]
        [SerializeField] Transform _popupWindowGroup;

        // フェード幕として使うImage. Prefab側では1x1でも、初期化時に画面サイズへ広げる.
        [SerializeField] UnityEngine.UI.Image _imgFadeCurtrain;
        Tween _tweenFadeCurtain;
        Canvas _canvasFadeCurtain;
        bool _isFadeCurtainInitialized;
        int _fadeCurtainScreenWidth;
        int _fadeCurtainScreenHeight;

        const int kFadeSortingOrderFullScreen = 100;
        const int kFadeSortingOrderUnderLoadingUI = 50;
        const int kFadeSortingOrderUnderHUD = -50;


        NormalWindow _currentNormalWindow = null;

        void Initialize()
        {
            InitializeFadeCurtain();
        }

        void Start()
        {
            // 開いているPopupの有無に応じて、利用するInputActionMapを切り替える.
            void ChangeInputMode()
            {
                if (_popupWindowGroup.childCount == 0)
                {
                    // NormalWindowが存在するなら、それの設定を使用する.
                    if (_currentNormalWindow != null)
                    {
                        // ActionMap名が設定されている時だけ切り替える.
                        if (string.IsNullOrEmpty(_currentNormalWindow.InputActionMap) == false)
                        {
                            IPlayerInputManager.Instance.SwitchCurrentActionMap(_currentNormalWindow.InputActionMap);
                        }
                    }
                    else
                    {
                        // NormalWindowが無い場合はデフォルトに戻す.
                        IPlayerInputManager.Instance.SwitchCurrentActionMap(null);
                    }
                }
                else
                {
                    // 一番上のPopupWindowのActionMapにする.
                    for (int iChild = _popupWindowGroup.childCount - 1; iChild >= 0; iChild--)
                    {
                        var puWnd = _popupWindowGroup.GetChild(iChild).GetComponent<PopupWindow>();
                        if (puWnd == null) continue;

                        // ActionMap名が設定されている時だけ切り替える.
                        if (string.IsNullOrEmpty(puWnd.InputActionMap) == false)
                        {
                            IPlayerInputManager.Instance.SwitchCurrentActionMap(puWnd.InputActionMap);
                            return;
                        }
                    }
                }
            }

            // 子Window数が変化した時、InputActionMapを変更する.
            _popupWindowGroup.OnTransformChildrenChangedAsObservable()
                .Subscribe(_ =>
                {
                    ChangeInputMode();
                }).AddTo(this);

            // NormalWindowが変更された時、InputActionMapを変更する.
            Observable.EveryValueChanged(this, x => _currentNormalWindow)
                .Subscribe(_ =>
                {
                    ChangeInputMode();
                });
        }

        //==========================================================================
        /**
         *    @brief       Popupが無い時の入力Map参照先を設定する.
         *    @param[in]   window 現在の通常Window.
         */
        //==========================================================================
        public void SetNormalWindow(NormalWindow window)
        {
            // Popupが無い時の入力Map参照先として、現在の通常Windowを保持する.
            _currentNormalWindow = window;
        }

        //==========================================================================
        /**
         *    @brief       フェードを要求する.
         *    @param[in]   fadeColor フェード色.
         *    @param[in]   fadeType フェード種別.
         *    @param[in]   durationMilliseconds フェード時間.
         *    @param[in]   priority フェード表示優先度.
         */
        //==========================================================================
        public async UniTask RequestFade(FadeColors fadeColor, FadeTypes fadeType, int durationMilliseconds, FadePriorities priority)
        {
            // リクエストごとに開始Alphaを決め直し、FadeIn/FadeOutの途中状態に依存しないようにする.
            InitializeFadeCurtain();
            if (_imgFadeCurtrain == null) return;

            UpdateFadeCurtainSize();
            SetFadePriority(priority);

            float targetAlpha = fadeType == FadeTypes.FadeIn ? 0 : 1;
            float startAlpha = fadeType == FadeTypes.FadeIn ? 1 : 0;
            SetFadeColor(fadeColor, startAlpha);

            _imgFadeCurtrain.gameObject.SetActive(true);
            _imgFadeCurtrain.raycastTarget = true;

            _tweenFadeCurtain?.Kill();
            _tweenFadeCurtain = null;

            float duration = Mathf.Max(0, durationMilliseconds) / 1000.0f;
            if (duration <= 0)
            {
                SetFadeColor(fadeColor, targetAlpha);
                ApplyFadeEndState(targetAlpha);
                return;
            }

            Tween tween = _imgFadeCurtrain.DOFade(targetAlpha, duration)
                .SetEase(Ease.Linear)
                .SetUpdate(true)
                .SetLink(_imgFadeCurtrain.gameObject);
            _tweenFadeCurtain = tween;

            await tween;

            if (_tweenFadeCurtain != tween) return;

            _tweenFadeCurtain = null;
            ApplyFadeEndState(targetAlpha);
        }

        void InitializeFadeCurtain()
        {
            // 初回だけCanvasやサイズを取得し、未使用時はRaycastも表示も無効にしておく.
            if (_isFadeCurtainInitialized) return;
            if (_imgFadeCurtrain == null) return;

            _canvasFadeCurtain = _imgFadeCurtrain.GetComponentInParent<Canvas>();
            UpdateFadeCurtainSize();
            SetFadePriority(FadePriorities.FullScreen);
            SetFadeColor(FadeColors.Black, 0);
            _imgFadeCurtrain.raycastTarget = false;
            _imgFadeCurtrain.gameObject.SetActive(false);
            _isFadeCurtainInitialized = true;
        }

        void UpdateFadeCurtainSize()
        {
            // 解像度が変わった時だけ、1x1のPrefab Imageを現在の画面サイズへ合わせ直す.
            if (_imgFadeCurtrain == null) return;
            if (_fadeCurtainScreenWidth == Screen.width && _fadeCurtainScreenHeight == Screen.height) return;

            var rectTransform = _imgFadeCurtrain.transform as RectTransform;
            if (rectTransform == null) return;

            _fadeCurtainScreenWidth = Screen.width;
            _fadeCurtainScreenHeight = Screen.height;
            rectTransform.sizeDelta = new Vector2(_fadeCurtainScreenWidth, _fadeCurtainScreenHeight);
        }

        void SetFadeColor(FadeColors fadeColor, float alpha)
        {
            Color color = fadeColor == FadeColors.White ? Color.white : Color.black;
            color.a = alpha;
            _imgFadeCurtrain.color = color;
        }

        void SetFadePriority(FadePriorities priority)
        {
            // CanvasのsortingOrderで、フェード幕をLoadingUI/HUDより上または下へ配置する.
            if (_canvasFadeCurtain == null) return;

            switch (priority)
            {
                case FadePriorities.UnderHUD:
                    _canvasFadeCurtain.sortingOrder = kFadeSortingOrderUnderHUD;
                    break;
                case FadePriorities.UnderLoadingUI:
                    _canvasFadeCurtain.sortingOrder = kFadeSortingOrderUnderLoadingUI;
                    break;
                default:
                    _canvasFadeCurtain.sortingOrder = kFadeSortingOrderFullScreen;
                    break;
            }
        }

        void ApplyFadeEndState(float alpha)
        {
            // 完全に透明になった幕は非表示にし、入力を拾わない状態へ戻す.
            bool isVisible = alpha > 0;
            _imgFadeCurtrain.raycastTarget = isVisible;
            _imgFadeCurtrain.gameObject.SetActive(isVisible);
        }

        //==========================================================================
        /**
         *    @brief       Windowアセットをロードして生成する.
         *    @param[in]   assetAddress 作成するWindowアセット.
         *    @param[in]   onInitialize WindowのOnInitialize前に実行される関数.
         *    @return      生成したWindow.
         */
        //==========================================================================
        public async UniTask<TWindow> CreateWindow<TWindow>(object assetAddress, System.Func<TWindow, UniTask> onInitialize) where TWindow : WindowBase
        {
            // アセットをロードする.
            var handle = UnityEngine.AddressableAssets.Addressables.LoadAssetAsync<GameObject>(assetAddress);
            // ロード完了を待つ.
            var asset = await handle;
            if(asset == null)
            {
                return null;
            }

            // Windowの親Transformを取得する.
            Transform parent = _popupWindowGroup;

            // Windowを生成する.
            var goWindow = Instantiate(asset, parent);
            var window = goWindow.GetComponent<TWindow>();

            // Window破棄時にアセットハンドルも解放されるように登録する.
            goWindow.OnDestroyAsObservable()
                .Subscribe(_ =>
                {
                    UnityEngine.AddressableAssets.Addressables.Release(handle);
                });

            // Windowを初期化する.
            await InitWindow(window, onInitialize);

            return window;
        }

        //==========================================================================
        /**
         *    @brief       生成したWindowを初期化して表示状態へ進める.
         *    @param[in]   window 初期化するWindow.
         *    @param[in]   onInitialize WindowのOnInitialize前に実行される関数.
         */
        //==========================================================================
        async UniTask InitWindow<TWindow>(TWindow window, System.Func<TWindow, UniTask> onInitialize) where TWindow : WindowBase
        {
            // 非表示状態で初期化する.
            window.gameObject.SetActive(false);

            window.NowState = WindowBase.WindowStates.Initializing;
            // 引数で指定された外部初期化関数を実行する.
            if(onInitialize != null)
            {
                await onInitialize(window);
            }
            // Windowの初期化関数を実行する.
            await window.OnInitialize();
            window.NowState = WindowBase.WindowStates.Initialized;

            // Activeにして表示処理へ進める.
            window.gameObject.SetActive(true);

            window.NowState = WindowBase.WindowStates.Showing;
            // Windowの表示関数を実行する.
            await window.OnShow();
            window.NowState = WindowBase.WindowStates.Shown;
        }

        //==========================================================================
        /**
         *    @brief       Windowを閉じて破棄する.
         *    @param[in]   window 閉じるWindow.
         */
        //==========================================================================
        public async UniTask CloseWindow(WindowBase window)
        {
            // 既に終了中なら何もしない.
            if (window.NowState == WindowBase.WindowStates.Closeing) return;

            // 表示状態まで待つ.
            await window.WaitForShown();

            // 破棄時の処理を実行する.
            window.NowState = WindowBase.WindowStates.Closeing;
            await window.OnClose();

            // Windowを破棄する.
            Destroy(window.gameObject);
        }
    }

}
