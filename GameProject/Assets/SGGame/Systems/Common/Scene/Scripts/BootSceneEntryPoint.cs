//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     BootSceneEntryPoint.cs
 *    @brief    起動シーン用エントリポイント
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena.SceneManagement.Utilities;
using MackySoft.Navigathena.SceneManagement.VContainer;
using System.Threading;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using VContainer.Unity;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       初回起動時のみ実行されるシーンエントリポイント.
     *
     *    常駐するPersistentSceneを読み込み、以降のシーンの親Scopeを用意します.
     */
    //==========================================================================
    public sealed class BootSceneEntryPoint : ScopedSceneEntryPoint
    {
        // 起動時に必ず親として扱う常駐シーン名.
        const string kPersistentSceneName = "PersistentScene";

        //==========================================================================
        /**
         *    @brief       親LifetimeScopeとして使うPersistentSceneを用意する.
         *    @param[in]   cancellationToken キャンセル通知.
         *    @return      PersistentSceneのLifetimeScope.
         */
        //==========================================================================
        protected override async UniTask<LifetimeScope> EnsureParentScope(CancellationToken cancellationToken)
        {
            // PersistentSceneが未ロードなら追加ロードし、以降のシーンの親Scopeとして使う.
            if (!SceneManager.GetSceneByName(kPersistentSceneName).isLoaded)
            {
                await SceneManager.LoadSceneAsync(kPersistentSceneName, LoadSceneMode.Additive)
                .ToUniTask(cancellationToken: cancellationToken);
            }

            Scene persistentScene = SceneManager.GetSceneByName(kPersistentSceneName);

    #if UNITY_EDITOR
            // Editor上ではPersistentSceneを現在シーンより前へ並べる.
            EditorSceneManager.MoveSceneBefore(persistentScene, gameObject.scene);
    #endif

            // PersistentSceneのLifetimeScopeコンテナを構築する.
            if (persistentScene.TryGetComponentInScene(out LifetimeScope persistentLifetimeScope, true) && persistentLifetimeScope.Container == null)
            {
                await UniTask.RunOnThreadPool(() => persistentLifetimeScope.Build(), cancellationToken: cancellationToken);
            }
            return persistentLifetimeScope;
        }
    }
}