//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     PlayerInputManager.cs
 *    @brief    プレイヤー入力管理
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using R3;

using UnityEngine;
using UnityEngine.InputSystem;
using SGSys;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       プレイヤー入力サービスの公開窓口.
     */
    //==========================================================================
    public interface IPlayerInputManager : IService<IPlayerInputManager>
    {
        //==========================================================================
        /**
         *    @brief       UI操作用の入力状態を取得する.
         *    @return      UI操作用入力状態.
         */
        //==========================================================================
        PlayerInputManager.UIActions UIAction { get; }

        //==========================================================================
        /**
         *    @brief       直近の入力デバイスがキーボード/マウス系かどうかを取得する.
         *    @return      キーボード/マウス系ならtrue.
         */
        //==========================================================================
        bool IsNowKeyboardMouseMode { get; }

        //==========================================================================
        /**
         *    @brief       入力デバイスが切り替わった時に通知されるObservableを取得する.
         *    @return      入力デバイス変更通知.
         */
        //==========================================================================
        Observable<PlayerInputManager.DevideTypes> OnChangeDevice { get; }

        // trueの間はUI入力と入力デバイス検知を止め、フェード中などの誤操作を防ぐ.
        bool IsInputBlocked { get; }
        void SetInputBlocked(bool isBlocked);

        //==========================================================================
        /**
         *    @brief       現在有効なInputActionMapを切り替える.
         *    @param[in]   mapName 切り替え先のActionMap名.
         */
        //==========================================================================
        void SwitchCurrentActionMap(string mapName);
    }

    //==========================================================================
    /**
     *    @brief       UnityのPlayerInputをServiceLocator経由で扱う入力管理クラス.
     */
    //==========================================================================
    [RequireComponent(typeof(PlayerInput))]
    [DefaultExecutionOrder(-1)]
    public class PlayerInputManager : MonoBehaviour, IPlayerInputManager
    {
        // UnityのPlayerInputを包み、UIやGameplayから必要な入力状態を読みやすい形で提供する.
        // 既存コードがPlayerInputManager.Instanceを参照しているため、ServiceLocator経由の互換入口として残す.
        private static PlayerInputManager Instance => IPlayerInputManager.Instance as PlayerInputManager;

        [SerializeField] PlayerInput _playerInput;

        //======================================
        // Gameplay
        //======================================
        public class GameplayActions
        {

            public void Initialize(InputActionMap actMap)
            {

            }
        }
        GameplayActions _gameplayAction = new();

        //======================================
        // UI
        //======================================
        public class UIActions
        {
            // UI ActionMapの入力を、呼び出し側がboolで確認できる形に変換する.
            InputAction _axis { get; set; }
            InputAction _decide { get; set; }
            InputAction _cancel { get; set; }
            InputAction _option1 { get; set; }
            InputAction _option2 { get; set; }
            bool _isInputBlocked;

            // 入力ブロック中は、すべてのUI入力を「押されていない」ものとして扱う.
            public bool AxisLeft => _isInputBlocked == false && _axis != null && _axis.WasPerformedThisFrame() ? _axis.ReadValue<Vector2>().x < 0 : false;
            public bool AxisRight => _isInputBlocked == false && _axis != null && _axis.WasPerformedThisFrame() ? _axis.ReadValue<Vector2>().x > 0 : false;

            public bool AxisUp => _isInputBlocked == false && _axis != null && _axis.WasPerformedThisFrame() ? _axis.ReadValue<Vector2>().y > 0 : false;
            public bool AxisDown => _isInputBlocked == false && _axis != null && _axis.WasPerformedThisFrame() ? _axis.ReadValue<Vector2>().y < 0 : false;
            public bool IsPressAxis => _isInputBlocked == false && _axis != null && _axis.ReadValue<Vector2>().sqrMagnitude > 0;

            public bool Decide => _isInputBlocked == false && _decide != null && _decide.triggered;
            public bool Cancel => _isInputBlocked == false && _cancel != null && _cancel.triggered;
            public bool Option1 => _isInputBlocked == false && _option1 != null && _option1.triggered;
            public bool Option2 => _isInputBlocked == false && _option2 != null && _option2.triggered;

            public void SetInputBlocked(bool isBlocked)
            {
                // Fade中など、画面操作を受け付けたくない期間だけtrueにする.
                _isInputBlocked = isBlocked;
            }

            public void Initialize(InputActionMap actMap)
            {
                // ActionMapが未設定でも、UI側がnull参照で止まらないようにする.
                if (actMap == null)
                {
                    return;
                }

                _axis = actMap.FindAction("Axis", false);
                _decide = actMap.FindAction("Decide", false);
                _cancel = actMap.FindAction("Cancel", false);
                _option1 = actMap.FindAction("Option1", false);
                _option2 = actMap.FindAction("Option2", false);
            }

        }
        UIActions _uiAction = new();
        public UIActions UIAction => _uiAction;

        bool _isInputBlocked;
        public bool IsInputBlocked => _isInputBlocked;
        public void SetInputBlocked(bool isBlocked)
        {
            // Manager本体とUIActionsの両方へ同じブロック状態を反映する.
            _isInputBlocked = isBlocked;
            _uiAction.SetInputBlocked(isBlocked);
        }


        //======================================
        // 入力デバイスの種類.
        //======================================
        public enum DevideTypes
        {
            None,
            Keyboard,
            XBOX,
            PlayStation,
            Switch,

            _Count_,
        }
        DevideTypes _lastInputDevice = DevideTypes.None;

        // 入力デバイス識別用.
        private InputAction _deletectionKeyboard = new InputAction(type: InputActionType.PassThrough, binding: "<Keyboard>/AnyKey", interactions: "Press");
        private InputAction _deletectionXBOX = new InputAction(type: InputActionType.PassThrough, binding: "<XInputController>/*", interactions: "Press");
        private InputAction _deletectionDS = new InputAction(type: InputActionType.PassThrough, binding: "<DualShockGamepad>/*", interactions: "Press");
        private InputAction _deletectionSwitch = new InputAction(type: InputActionType.PassThrough, binding: "<SwitchProControllerHID>/*", interactions: "Press");

        // 直近の入力デバイスがキーボード/マウス系かどうか.
        public bool IsNowKeyboardMouseMode => _lastInputDevice == DevideTypes.Keyboard;


        //======================================
        // イベント
        //======================================

        // 入力デバイスが変更された時の通知.
        BehaviorSubject<DevideTypes> _onChangeDevice = new(DevideTypes.None);
        public Observable<DevideTypes> OnChangeDevice => _onChangeDevice;



        //======================================
        // 初期化.
        private void Awake()
        {
            // PlayerInputManagerは全体で1つだけ使う. 重複した場合は先に登録済みのものを優先する.
            // ServiceLocatorはinterfaceで返すため、Unity独自のnull判定を使えるようObjectとして比較する.
            UnityEngine.Object currentManager = IPlayerInputManager.Instance as UnityEngine.Object;
            if (currentManager != null && currentManager != this)
            {
                Destroy(gameObject);
                return;
            }

            // 他のクラスは IPlayerInputManager.Instance から、このManagerへアクセスする.
            // 既存の PlayerInputManager.Instance も、この登録内容を参照する.
            ServiceLocator<IPlayerInputManager>.Register(this);

            // Prefab側の設定漏れがあっても、同じGameObject上のPlayerInputを自動取得する.
            if (_playerInput == null)
            {
                TryGetComponent(out _playerInput);
            }

            if (_playerInput == null || _playerInput.actions == null)
            {
#if GAME_DEBUG && !IS_PRODUCT
                DebugLog.Error(SystemConst.DebugGroup.System,"PlayerInputManagerにPlayerInput、またはInputActionが設定されていません。");
#endif
                enabled = false;
                return;
            }

            // Gameplay/UIのActionMapを初期化する.
            _gameplayAction.Initialize(_playerInput.actions.FindActionMap("Gameplay", false));
            _uiAction.Initialize(_playerInput.actions.FindActionMap("UI", false));

            _deletectionKeyboard.Enable();
            _deletectionXBOX.Enable();
            _deletectionDS.Enable();
            _deletectionSwitch.Enable();
        }

        private void OnDestroy()
        {
            // 自分が登録したサービスだけを解除し、別のPlayerInputManagerの登録を消さないようにする.
            // interfaceのまま比較するとUnityのObject比較にならないため、Objectへ戻してから比較する.
            UnityEngine.Object currentManager = IPlayerInputManager.Instance as UnityEngine.Object;
            if (currentManager == this)
            {
                ServiceLocator<IPlayerInputManager>.Unregister();
            }

            // コードで作成したInputActionは、破棄時に明示的に解放する.
            _deletectionKeyboard.Dispose();
            _deletectionXBOX.Dispose();
            _deletectionDS.Dispose();
            _deletectionSwitch.Dispose();
            _onChangeDevice.Dispose();
        }

        void Update()
        {
            if (_isInputBlocked)
            {
                // ブロック中はデバイス切り替え検知も含めて入力処理を停止する.
                return;
            }

            // 入力デバイスを判定する.
            if (_deletectionKeyboard.triggered || (Mouse.current != null && Mouse.current.delta.magnitude > 0))
            {
                if (_lastInputDevice != DevideTypes.Keyboard)
                {
                    // マウスアンロック中はカーソルを表示する.
                    if (Cursor.lockState == CursorLockMode.None)
                    {
                        Cursor.visible = true;
                    }
#if GAME_DEBUG && !IS_PRODUCT
                    DebugLog.Info(SystemConst.DebugGroup.System,"[Inputデバイス変更]キーボード");
#endif
                    _onChangeDevice.OnNext(DevideTypes.Keyboard);
                    _lastInputDevice = DevideTypes.Keyboard;
                }
            }

            // XBOXコントローラー.
            else if (_deletectionXBOX.triggered)
            {
                if (_lastInputDevice != DevideTypes.XBOX)
                {
                    // マウスアンロック中はカーソルを非表示にする.
                    if (Cursor.lockState == CursorLockMode.None)
                    {
                        Cursor.visible = false;
                    }
#if GAME_DEBUG && !IS_PRODUCT
                    DebugLog.Info(SystemConst.DebugGroup.System,"[Inputデバイス変更]XBOXコントローラ");
#endif
                    _onChangeDevice.OnNext(DevideTypes.XBOX);
                    _lastInputDevice = DevideTypes.XBOX;
                }
            }
            // PlayStationコントローラー.
            else if (_deletectionDS.triggered)
            {
                if (_lastInputDevice != DevideTypes.PlayStation)
                {
                    // マウスアンロック中はカーソルを非表示にする.
                    if (Cursor.lockState == CursorLockMode.None)
                    {
                        Cursor.visible = false;
                    }
#if GAME_DEBUG && !IS_PRODUCT
                    DebugLog.Info(SystemConst.DebugGroup.System,"[Inputデバイス変更]デュアルショック");
#endif
                    _onChangeDevice.OnNext(DevideTypes.PlayStation);
                    _lastInputDevice = DevideTypes.PlayStation;
                }
            }
            // Switchコントローラー.
            else if (_deletectionSwitch.triggered)
            {
                if (_lastInputDevice != DevideTypes.Switch)
                {
                    // マウスアンロック中はカーソルを非表示にする.
                    if (Cursor.lockState == CursorLockMode.None)
                    {
                        Cursor.visible = false;
                    }
#if GAME_DEBUG && !IS_PRODUCT
                    DebugLog.Info(SystemConst.DebugGroup.System,"[Inputデバイス変更]Switchコントローラ");
#endif
                    _onChangeDevice.OnNext(DevideTypes.Switch);
                    _lastInputDevice = DevideTypes.Switch;
                }
            }
        }

        //==========================================================================
        /**
         *    @brief       現在有効なInputActionMapを変更する.
         *    @param[in]   mapName 切り替え先のActionMap名. 空ならdefaultActionMapへ戻す.
         */
        //==========================================================================
        public void SwitchCurrentActionMap(string mapName)
        {
            if (_playerInput == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(mapName) == false)
            {
                _playerInput.SwitchCurrentActionMap(mapName);
#if GAME_DEBUG && !IS_PRODUCT
                DebugLog.Info( SystemConst.DebugGroup.System, $"[PlayerInputManager] ****** SwitchCurrentActionMap : {mapName}");
#endif
            }
            else
            {
                _playerInput.SwitchCurrentActionMap(_playerInput.defaultActionMap);
#if GAME_DEBUG && !IS_PRODUCT
                DebugLog.Info( SystemConst.DebugGroup.System, $"[PlayerInputManager] ****** SwitchCurrentActionMap : {_playerInput.defaultActionMap}");
#endif
            }
        }
    }
}
