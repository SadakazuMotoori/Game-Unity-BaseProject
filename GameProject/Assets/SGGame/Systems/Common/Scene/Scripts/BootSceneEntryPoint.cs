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
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using UnityEngine.SceneManagement;
using VContainer.Unity;
using MackySoft.Navigathena.SceneManagement;

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
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, this.GetCancellationTokenOnDestroy());
            cancellationToken = linkedCancellation.Token;
            cancellationToken.ThrowIfCancellationRequested();

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
            if (!persistentScene.TryGetComponentInScene(out PersistentSceneLifetimeScope persistentLifetimeScope, true) || !persistentLifetimeScope.isActiveAndEnabled)
            {
                throw new System.InvalidOperationException("PersistentSceneに有効なPersistentSceneLifetimeScopeがありません。");
            }
            if (persistentLifetimeScope.Container == null)
            {
                await UniTask.SwitchToMainThread(cancellationToken);
                persistentLifetimeScope.Build();
            }

            cancellationToken.ThrowIfCancellationRequested();
            persistentLifetimeScope.Initialize();
            await ISoundManager.Instance.WaitUntilReady(cancellationToken);
            return persistentLifetimeScope;
        }

        internal async UniTask RequestInitialSceneChange()
        {
            string              _nextSceneName  = "";
#if !IS_PRODUCT
            _nextSceneName = "DebugTopScene";
#else
            _nextSceneName = "Title";
#endif
            if (!SceneManager.GetSceneByName(_nextSceneName).isLoaded)
            {
                await ISceneTransitionManager.Instance.RequestSceneChange(_nextSceneName,0);
            }
        }
    }

    public sealed class BootSceneLifecycle : SceneLifecycleBase
    {
        readonly BootSceneEntryPoint _bootSceneEntryPoint;

        public BootSceneLifecycle(BootSceneEntryPoint bootSceneEntryPoint)
        {
            _bootSceneEntryPoint = bootSceneEntryPoint;
        }

        protected override UniTask OnEnter(ISceneDataReader reader, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            // Bootのアンロードで遷移を中断しないよう、以降は常駐Managerへ引き渡す。
            _bootSceneEntryPoint.RequestInitialSceneChange().Forget();
            return UniTask.CompletedTask;
        }
    }
}