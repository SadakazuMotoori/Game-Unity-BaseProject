//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     CameraController.cs
 *    @brief    VirtualCamera制御
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UniRx;
using Unity.Cinemachine;
using UnityEngine;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       CameraManagerから操作されるVirtualCamera制御クラス.
     */
    //==========================================================================
    public partial class CameraController : MonoBehaviour
    {
        // このControllerが扱うVirtualCamera一覧。Inspector上の登録内容をCameraDataとして参照する。
        [SerializeField]
        private CameraData[] m_CameraTable;

        // Normal Cameraとして扱うCameraDataのIndex.
        [Header("※Normal CameraのみのIndex")]
        [SerializeField] int _nowNormalCameraIndex = 0;

        //==========================================================================
        /**
         *    @brief       指定したカメラIDのCameraDataを取得する.
         *    @param[in]   camid カメラID.
         *    @return      対象のCameraData. 取得できない場合はnull.
         */
        //==========================================================================
        public CameraData GetCamera(CameraData.eCameraID camid)
        {
            return GetCameraData((int)camid);
        }
        public CameraData NormalCamera  { get { return GetCamera(CameraData.eCameraID.Normal); } }
        public CameraData DemoCamera    { get { return GetCamera(CameraData.eCameraID.Demo); } }
        public CameraData FreeCamera    { get { return GetCamera(CameraData.eCameraID.Free); } }

        // 全てのカメラ対象の現在のカメラ
        public ReactiveProperty<CameraData> CurrentCamData { get; set; } = new();
        // NormaCamera専用の現在のカメラ
        public CameraData               CurrentNormalCamData    => GetCameraData(_nowNormalCameraIndex);
        public CinemachineVirtualCameraBase CurrentVCam         => CurrentNormalCamData?.VCam;

        CameraData GetCameraData(int index)
        {
            // 未設定や範囲外のIndexは、例外にせず「取得できない」としてnullを返す。
            if (m_CameraTable == null) return null;
            if (index < 0 || index >= m_CameraTable.Length) return null;
            return m_CameraTable[index];
        }

        void Awake()
        {
            // VirtualCameraを1つも登録していない場合は、初期化する対象がない。
            if (m_CameraTable == null) return;

            // 初期設定
            for (int i = 0; i < m_CameraTable.Length; i++)
            {
                // 配列の一部だけ未設定でも、設定済みのVirtualCameraは動かせるようにする。
                if (m_CameraTable[i] == null) continue;
                m_CameraTable[i].Initialize(this);
            }
        }

        void OnEnable()
        {
            // このControllerが有効になったら、CameraManagerから操作できるように登録する。
            if (ICameraManager.Instance != null)
            {
                ICameraManager.Instance.SetCamera(this);
            }
        }

        void OnDisable()
        {
            // 自分が現在のControllerとして登録されている場合だけ、Managerから外す。
            if (ICameraManager.Instance != null && ICameraManager.Instance.CamCtrl == this)
            {
                ICameraManager.Instance.SetCamera(null);
            }
        }

        //==========================================================================
        /**
         *    @brief       管理中のVirtualCameraへ追従対象を設定する.
         *    @param[in]   target 追従対象.
         */
        //==========================================================================
        public void SetCameraTarget(Transform target)
        {
            // ターゲット未設定でも後で設定できるよう、ここでは安全に何もしない。
            if (m_CameraTable == null) return;

            foreach (var vcamData in m_CameraTable)
            {
                // 未設定の枠は飛ばし、設定済みのVirtualCameraだけにターゲットを反映する。
                if (vcamData == null) continue;
                vcamData.SetTarget(target);
            }
        }

        //==========================================================================
        /**
         *    @brief       1つのVirtualCameraに対する設定と操作窓口.
         */
        //==========================================================================
        [System.Serializable]
        public class CameraData
        {
            [SerializeField]
            eCameraID m_CameraID;
            public eCameraID CameraID => m_CameraID;

            [SerializeField]
            protected CinemachineVirtualCameraBase m_VCam;
            public CinemachineVirtualCameraBase VCam => m_VCam;

            CinemachinePanTilt m_CinemachinePanTilt;

            public CameraController m_OwnerCtrl { get; private set; }

            //==========================================================================
            /**
             *    @brief       m_CameraTableに登録されているカメラ種別.
             */
            //==========================================================================
            public enum eCameraID
            {
                Normal,
                Demo,
                Free,

                Debug
            }

            public void Initialize(CameraController owner)
            {
                m_OwnerCtrl = owner;

                // VirtualCamera未設定のCameraDataは、以降のCinemachine操作を行えない。
                if (m_VCam == null) return;

                m_CinemachinePanTilt = m_VCam.GetCinemachineComponent(CinemachineCore.Stage.Aim) as CinemachinePanTilt;
            }

            public void SetInputAxis()
            {
                // Cinemachine 3では入力接続はInputAxisController側で行うため、ここでは何もしない。
            }

            public void SetCurrent(bool enable)
            {
                // VirtualCamera未設定なら、優先度を変更する対象がない。
                if (m_VCam == null) return;

                m_VCam.Priority.Value = enable ? 10 : -100;
            }

            public float GetAngleY() => m_CinemachinePanTilt == null ? 0 : m_CinemachinePanTilt.PanAxis.Value;
            public void SetAngleY(float angle)
            {
                // PanTiltがないカメラでは、水平角度を直接操作できない。
                if (m_CinemachinePanTilt == null) return;
                m_CinemachinePanTilt.PanAxis.Value = angle;
            }

            public void SetTarget(Transform target)
            {
                // VirtualCamera未設定なら、Follow/LookAtを設定する対象がない。
                if (m_VCam == null) return;

                m_VCam.Follow = target;
                m_VCam.LookAt = target;
            }

        }
    }
}
