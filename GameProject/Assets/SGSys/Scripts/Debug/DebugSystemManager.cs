//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     DebugSystemManager.cs
 *    @brief    デバッグシステム管理
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;

using SGSys;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       デバッグシステム管理サービスの公開窓口.
     */
    //==========================================================================
    public interface IDebugSystemManager : IService<IDebugSystemManager>
    {
    }

    //==========================================================================
    /**
     *    @brief       デバッグシステム管理クラス.
     */
    //==========================================================================
    [DefaultExecutionOrder(-1)]
    public class DebugSystemManager : MonoBehaviour, IDebugSystemManager
    {
        [Header("FPS")]
        /// <summary>
        /// FPS表示オブジェクトのプレハブ
        /// </summary>
        /// <remarks>
        /// 処理不要の場合SGSys/Prefab/DebugSystemManagerプレハブのInspectorにてNone設定をします
        /// </remarks>
        [SerializeField] private DebugFPS _debugFpsPrefab;

        void Awake()
        {
            UnityEngine.Object currentManager = IDebugSystemManager.Instance as UnityEngine.Object;
            if (currentManager != null && currentManager != this)
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator<IDebugSystemManager>.Register(this);
            InitializeDebugSystem();
        }

        void OnDestroy()
        {
            UnityEngine.Object currentManager = IDebugSystemManager.Instance as UnityEngine.Object;
            if (currentManager == this)
            {
                ServiceLocator<IDebugSystemManager>.Unregister();
            }
        }

        void InitializeDebugSystem()
        {
            if (_debugFpsPrefab != null)
            {
                SGSys.DebugFPS.Create(_debugFpsPrefab, transform);
            }

            DebugLog.AddGroup(SystemConst.DebugGroup.System);
            DebugLog.AddGroup(SystemConst.DebugGroup.Game);
            DebugLog.AddGroup(SystemConst.DebugGroup.Scene);
        }
    }
}