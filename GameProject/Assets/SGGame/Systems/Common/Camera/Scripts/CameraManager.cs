//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     CameraManager.cs
 *    @brief    カメラ管理
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Rendering.Universal;

using SGSys;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       カメラ関連システムの公開窓口.
     */
    //==========================================================================
    public interface ICameraManager : IService<ICameraManager>
    {
        //======================================
        // プレイヤーカメラ関係
        //======================================
        Transform CurrentCameraTarget { get; }
        void SetCameraTarget(Transform target);
        
        CameraController CamCtrl { get; }
        void SetCamera(CameraController CamCtrl);

        float SetCinemachineBrainDefaultBlendTime(float duration);

        Camera UICamera             { get; }
        Camera CurrentMainCamera    { get; }
        void SetMainCameraRenderingEnabled(bool enabled);

        // ワールド座標をUI座標へ変換する.
        Vector2? ConvertWorldToUIPos(Vector3 worldPos) { return Vector2.zero; }

        void ManageMainCamera(ManagedMainCamera camera);
        void UnmanageMainCamera(ManagedMainCamera camera);

        void UpdateManagedMainCameraList();
    }

    //==========================================================================
    /**
     *    @brief       現在有効なCameraControllerとMainCameraを一元管理する.
     */
    //==========================================================================
    // 他のカメラ系コンポーネントより先にServiceLocatorへ登録するため、実行順を早める.
    [DefaultExecutionOrder(-10000)]
    public class CameraManager : MonoBehaviour, ICameraManager
    {
        // PersistentSceneで常駐し、現在有効なCameraControllerやMainCameraを一元管理する。
        //======================================
        // プレイヤーカメラ関係
        //======================================
        Transform _currentCameraTarget;
        public Transform CurrentCameraTarget => _currentCameraTarget;
        public void SetCameraTarget(Transform target)
        {
            float old = SetCinemachineBrainDefaultBlendTime(0);
            _currentCameraTarget = target;
            if (_CameraCtrl != null)
            {
                _CameraCtrl.SetCameraTarget(_currentCameraTarget);
            }

            SetCinemachineBrainDefaultBlendTime(old);
        }

        CameraController _CameraCtrl;
        public CameraController CamCtrl => _CameraCtrl;

        public void SetCamera(CameraController CamCtrl)
        {
            _CameraCtrl = CamCtrl;

            // CameraControllerが後から有効化された場合でも、すでに設定済みのターゲットを反映する。
            if (_CameraCtrl != null)
            {
                _CameraCtrl.SetCameraTarget(_currentCameraTarget);
            }
        }

        ReactiveProperty<CameraController.CameraData> _nowCamTypeRP = new();
        public System.IObservable<CameraController.CameraData> NowCamTypeRP => _nowCamTypeRP;

        CameraController.CameraData CurrentCamData
        {
            get
            {
                // ターゲットやControllerが未設定の起動直後は、現在カメラをまだ確定できない。
                if (_currentCameraTarget == null) return null;
                if (_CameraCtrl == null) return null;
                return _CameraCtrl.CurrentNormalCamData;
            }
        }

        // UIカメラ
        Camera _uiCamera;
        public Camera UICamera => _uiCamera;

        [SerializeField] List<Canvas> _uiCanvasList;

        // ワールド->UI座標変換用
        [SerializeField] RectTransform _uiRectTransform;

        //======================================
        // 複数メインカメラ管理
        // ・メインカメラは１つだけ有効にして、他を無効にするため。
        //======================================
        // メインカメラ管理リスト
        HashSet<ManagedMainCamera> _managedMainCameras = new();
        // 現在のメインカメラ
        ManagedMainCamera _currentMainCamera;
        bool _isMainCameraRenderingEnabled = false;
        public Camera CurrentMainCamera
        {
            get
            {
                if (_currentMainCamera == null) return null;
                return _currentMainCamera.Cam;
            }
        }
        public ManagedMainCamera CurrentMainCamera2 => _currentMainCamera;

        public void SetMainCameraRenderingEnabled(bool enabled)
        {
            _isMainCameraRenderingEnabled = enabled;
            UpdateManagedMainCameraList();
        }

        //==========================================================================
        /**
         *    @brief       ワールド座標をUI基準Rect上の座標へ変換する.
         *    @param[in]   worldPos ワールド座標.
         *    @return      UI座標. 変換できない場合はnull.
         */
        //==========================================================================
        public Vector2? ConvertWorldToUIPos(Vector3 worldPos)
        {
            // UI座標変換には「現在のメインカメラ」「UIカメラ」「UI基準Rect」がすべて必要。
            if (_currentMainCamera == null) return null;
            if (_currentMainCamera.Cam == null) return null;
            if (_uiRectTransform == null) return null;
            if (_uiCamera == null) return null;

            Vector3 vTarget = worldPos - _currentMainCamera.Cam.transform.position;

            if (Vector3.Dot(vTarget, _currentMainCamera.Cam.transform.forward) < 0) return null;

            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_currentMainCamera.Cam, worldPos);

            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_uiRectTransform, screenPos, _uiCamera, out pos);
            return pos;
        }

        //==========================================================================
        /**
         *    @brief       メインカメラ候補を登録する.
         *    @param[in]   mainCamera 登録するメインカメラ候補.
         */
        //==========================================================================
        public void ManageMainCamera(ManagedMainCamera mainCamera)
        {
            // OnEnable/Startの実行順によってnullが来ても、管理リストを壊さない。
            if (mainCamera == null) return;

            _managedMainCameras.Remove(mainCamera);

            _managedMainCameras.Add(mainCamera);

            // 現在のメインカメラ候補を再評価する.
            UpdateManagedMainCameraList();
        }

        //==========================================================================
        /**
         *    @brief       メインカメラ候補の登録を解除する.
         *    @param[in]   mainCamera 登録解除するメインカメラ候補.
         */
        //==========================================================================
        public void UnmanageMainCamera(ManagedMainCamera mainCamera)
        {
            // 破棄中や無効化中にnull扱いになっても、安全に無視する。
            if (mainCamera == null) return;

            _managedMainCameras.Remove(mainCamera);

            // 現在のメインカメラ候補を再評価する.
            UpdateManagedMainCameraList();
        }

        //==========================================================================
        /**
         *    @brief       メインカメラ候補を整理し、最優先のカメラだけを有効化する.
         */
        //==========================================================================
        public void UpdateManagedMainCameraList()
        {
            _currentMainCamera = null;

            // 破棄済み参照を削除する.
            _managedMainCameras.RemoveWhere(x => x == null);

            // 最も優先順位の高いカメラを検索する.
            foreach (var managedCam in _managedMainCameras)
            {
                // ManagedMainCamera自体が無効なら、候補から外す.
                if (managedCam.enabled == false)
                {
                    continue;
                }

                // Cameraコンポーネントを持たないものは、メインカメラ候補にできない。
                if (managedCam.Cam == null)
                {
                    continue;
                }

                // 同じ優先順位の場合は、後から評価したカメラを優先する.
                if (_currentMainCamera == null)
                {
                    _currentMainCamera = managedCam;
                }
                else if (managedCam.Priority >= _currentMainCamera.Priority)
                {
                    _currentMainCamera = managedCam;
                }
            }

            // 最優先カメラのみ有効化し、その他はすべて無効化する.
            foreach (var managedCam in _managedMainCameras)
            {
                if (_currentMainCamera == managedCam)
                {
                    if (managedCam.Cam == null)
                    {
                        continue;
                    }

                    // 最優先のカメラだけを実際に描画する。
                    managedCam.Cam.enabled = _isMainCameraRenderingEnabled;

                    var cameraData = _currentMainCamera.Cam.GetUniversalAdditionalCameraData();

                    _uiCamera = null;
                    if (cameraData != null)
                    {
                        // URPのCameraStackからUIレイヤーのカメラを探し、Canvasの描画カメラに使う。
                        int uiLayer = LayerMask.NameToLayer("UI");
                        foreach (var cam in cameraData.cameraStack)
                        {
                            if (cam.gameObject.layer == uiLayer)
                            {
                                _uiCamera = cam;
                            }
                        }
                    }

                    if (_uiCanvasList != null)
                    {
                        foreach (var uiCanvas in _uiCanvasList)
                        {
                            if (uiCanvas != null)
                            {
                                uiCanvas.worldCamera = _uiCamera;
                            }
                        }
                    }

                }
                else
                {
                    if (managedCam.Cam == null)
                    {
                        continue;
                    }

                    // 優先されなかったメインカメラは無効化し、同時に複数描画されるのを防ぐ。
                    managedCam.Cam.enabled = false;
                }
            }
        }

        //==========================================================================
        /**
         *    @brief       CinemachineBrainのデフォルトブレンド時間を一時変更する.
         *    @param[in]   duration 設定するブレンド時間.
         *    @return      変更前のブレンド時間.
         */
        //==========================================================================
        public float SetCinemachineBrainDefaultBlendTime(float duration)
        {
            // ActiveBrainが1つもない場合、GetActiveBrain(0)自体が失敗するため先に件数を確認する。
            if (Unity.Cinemachine.CinemachineBrain.ActiveBrainCount == 0) return 0;

            Unity.Cinemachine.CinemachineBrain brain = Unity.Cinemachine.CinemachineBrain.GetActiveBrain(0);

            // MainCameraがまだ生成されていないタイミングでは、Blend時間を変更できない。
            if (brain == null) return 0;

            float old = brain.DefaultBlend.Time;
            brain.DefaultBlend.Time = duration;
            return old;
        }

        void Awake()
        {
            // CameraManagerは全体で1つだけ使う。重複した場合は先に登録済みのものを優先する。
            // ServiceLocatorはinterfaceで返すため、Unity独自のnull判定を使えるようObjectとして比較する。
            UnityEngine.Object currentManager = ICameraManager.Instance as UnityEngine.Object;
            if (currentManager != null && currentManager != this)
            {
                Destroy(gameObject);
                return;
            }

            // 他のクラスは ICameraManager.Instance から、このManagerへアクセスする。
            ServiceLocator<ICameraManager>.Register(this);

            // 同じPrefabにCameraControllerが付いている場合は、自動で接続する。
            if (TryGetComponent(out CameraController cameraController))
            {
                SetCamera(cameraController);
            }

            // 同じPrefabにManagedMainCameraが付いている場合は、自分自身をメインカメラ候補にする。
            if (TryGetComponent(out ManagedMainCamera mainCamera))
            {
                ManageMainCamera(mainCamera);
            }
        }

        void OnDestroy()
        {
            // 自分が登録したサービスだけを解除し、別のCameraManagerの登録を消さないようにする。
            // interfaceのまま比較するとUnityのObject比較にならないため、Objectへ戻してから比較する。
            UnityEngine.Object currentManager = ICameraManager.Instance as UnityEngine.Object;
            if (currentManager == this)
            {
                ServiceLocator<ICameraManager>.Unregister();
            }
        }

        void Start()
        {
            // CameraControllerの変更を監視し、設定済みターゲットを再反映する.
            this.ObserveEveryValueChanged(x => _CameraCtrl)
                .Subscribe(ctrl =>
                {
                    if (_CameraCtrl != null)
                    {
                        _CameraCtrl.SetCameraTarget(_currentCameraTarget);
                    }
                });

            // 現在カメラ種別の変更を通知用ReactivePropertyへ反映する.
            this.ObserveEveryValueChanged(x => CurrentCamData)
                .Subscribe(camData =>
                {
                    _nowCamTypeRP.Value = camData;
                });
        }
    }
}
