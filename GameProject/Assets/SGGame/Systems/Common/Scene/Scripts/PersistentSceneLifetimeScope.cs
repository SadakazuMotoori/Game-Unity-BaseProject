//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     PersistentSceneLifetimeScope.cs
 *    @brief    常駐シーン用LifetimeScope
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using MackySoft.Navigathena;
using MackySoft.Navigathena.SceneManagement;
using MackySoft.Navigathena.SceneManagement.VContainer;
using System;
using System.Threading;
using UnityEngine;
using VContainer;
using VContainer.Unity;

using SGSys;
namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       PersistentScene用ライフサイクル.
     */
    //==========================================================================
    public sealed class PersistentSceneLifecycle : SceneLifecycleBase
    {
        // PersistentScene自体のライフサイクル. 常駐シーンなので現状は待機処理のみを行う.
        protected override async UniTask OnInitialize(ISceneDataReader reader, IProgress<IProgressDataStore> progress, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1);
        }

        protected override async UniTask OnEnter(ISceneDataReader reader, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1);
        }

        protected override async UniTask OnExit(ISceneDataWriter writer, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1);
        }

        protected override async UniTask OnFinalize(ISceneDataWriter writer, IProgress<IProgressDataStore> progress, CancellationToken cancellationToken)
        {
            await UniTask.DelayFrame(1);
        }
    }

    //==========================================================================
    /**
     *    @brief       ゲーム全体で共有するManager Prefabを生成するLifetimeScope.
     */
    //==========================================================================
    public sealed class PersistentSceneLifetimeScope : LifetimeScope
    {
        //==========================================================================
#if GAME_DEBUG && !IS_PRODUCT
         // PersistentSceneが起動時に生成するDebugSystemManager Prefab.
         // デバッグ機能管理は開発中全シーン共通で必要になるため、PersistentSceneで常駐させる.
         [Header("デバッグシステム管理システム")]
         [SerializeField] GameObject _debugSystemManagerPrefab;
         GameObject _debugSystemManagerInstance;
#endif
        //==========================================================================

        // PersistentSceneが起動時に生成するMainCamera Prefab.
        // SerializeFieldで参照を持つことで、必須システムをビルドに含める.
        [Header("カメラ管理システム")]
        [SerializeField] GameObject _mainCameraPrefab;
        GameObject _mainCameraInstance;

        // PersistentSceneが起動時に生成するInputManager Prefab.
        // 入力は全シーン共通で必要になるため、PersistentSceneで常駐させる.
        [Header("入力管理システム")]
        [SerializeField] GameObject _inputManagerPrefab;
        GameObject _inputManagerInstance;

        // PersistentSceneが起動時に生成するSoundManager Prefab.
        // サウンド再生は全シーン共通で必要になるため、PersistentSceneで常駐させる.
        [Header("サウンド管理システム")]
        [SerializeField] GameObject _soundManagerPrefab;
        GameObject _soundManagerInstance;

        // PersistentSceneが起動時に生成するWindowManager Prefab.
        // ウィンドウ制御は全シーン共通で必要になるため、PersistentSceneで常駐させる.
        [Header("ウィンドウ管理システム")]
        [SerializeField] GameObject _windowManagerPrefab;
        GameObject _windowManagerInstance;

        // PersistentSceneが起動時に生成するSceneTransitionManager Prefab.
        // シーン遷移制御は全シーン共通で必要になるため、PersistentSceneで常駐させる.
        [Header("シーン推移管理システム")]
        [SerializeField] GameObject _sceneTransitionManagerPrefab;
        GameObject _sceneTransitionManagerInstance;
        //==========================================================================

        // PersistentSceneが起動時に生成するGameManager Prefab.
        // アプリ固有の機能やシステムは全てこれが管理。全シーン共通で必要になるため、PersistentSceneで常駐させる.
        [Header("アプリ固有機能管理システム")]
        [SerializeField] GameObject _gameManagerPrefab;
        GameObject _gameManagerInstance;

        //==========================================================================

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterSceneLifecycle<PersistentSceneLifecycle>();
        }
    
        private void Start()
        {
            // 常駐Managerは、シーン遷移を始める前に必要な順序で初期化する.
#if GAME_DEBUG && !IS_PRODUCT
            InitializeDebugSystemManager();
#endif
            InitializeMainCamera();
            InitializeInputManager();
            InitializeSoundManager();
            InitializeWindowManager();
            InitializeSceneTransitionManager();

            InitializeGameManager();

            // 非製品版はデバッグシーン、製品版はタイトルシーンへ遷移する.
            string              _nextSceneName  = "";
#if !IS_PRODUCT
            _nextSceneName = "DebugTopScene";
#else
            _nextSceneName = "TitleScene";
#endif
            if (!SceneManager.GetSceneByName(_nextSceneName).isLoaded)
            {
   //             ISceneTransitionManager.Instance.RequestSceneChange(_nextSceneName,0).Forget();
            }
        }

        void InitializeInputManager()
        {
            // すでにInputManagerを生成済みなら、二重生成を避ける.
            if (_inputManagerInstance != null)
            {
                return;
            }

            // 何らかの理由でPlayerInputManagerが先に存在している場合も、重複生成しない.
    //        if (IPlayerInputManager.Instance != null)
            {
                return;
            }

            if (_inputManagerPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "InputManager : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _inputManagerInstance = Instantiate(_inputManagerPrefab, transform);
        }

        void InitializeSoundManager()
        {
            // すでにSoundManagerを生成済みなら、二重生成を避ける.
            if (_soundManagerInstance != null)
            {
                return;
            }

            // 何らかの理由でSoundManagerが先に存在している場合も、重複生成しない.
    //        if (ISoundManager.Instance != null)
            {
                return;
            }

            if (_soundManagerPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "SoundManager : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _soundManagerInstance = Instantiate(_soundManagerPrefab, transform);
        }

        void InitializeMainCamera()
        {
            // すでにMainCameraを生成済みなら、二重生成を避ける.
            if (_mainCameraInstance != null)
            {
                return;
            }

            // 何らかの理由でCameraManagerが先に存在している場合も、重複生成しない.
    //        if (ICameraManager.Instance != null)
            {
                return;
            }

            if (_mainCameraPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "MainCamera : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _mainCameraInstance = Instantiate(_mainCameraPrefab, transform);
        }

        void InitializeWindowManager()
        {
            // すでにWindowManagerを生成済みなら、二重生成を避ける.
            if (_windowManagerInstance != null)
            {
                return;
            }

            // 何らかの理由でWindowManagerが先に存在している場合も、重複生成しない.
    //        if (IWindowManager.Instance != null)
            {
                return;
            }

            if (_windowManagerPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "WindowManager : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _windowManagerInstance = Instantiate(_windowManagerPrefab, transform);
        }

        void InitializeSceneTransitionManager()
        {
            // SceneTransitionManagerも他Managerと同様、既存登録があれば重複生成しない.
            if (_sceneTransitionManagerInstance != null)
            {
                return;
            }

    //        if (ISceneTransitionManager.Instance != null)
            {
                return;
            }

            if (_sceneTransitionManagerPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "SceneTransitionManager : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _sceneTransitionManagerInstance = Instantiate(_sceneTransitionManagerPrefab, transform);
        }

        void InitializeGameManager()
        {
            // GameManagerも他Managerと同様、既存登録があれば重複生成しない.
            if (_gameManagerInstance != null)
            {
                return;
            }

    //        if (IGameManager.Instance != null)
            {
                return;
            }

            if (_gameManagerPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.Game, "GameManager : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _gameManagerInstance = Instantiate(_gameManagerPrefab, transform);
        }

#if GAME_DEBUG && !IS_PRODUCT
        void InitializeDebugSystemManager()
        {
            // DebugSystemManagerも他Managerと同様、既存登録があれば重複生成しない.
            if (_debugSystemManagerInstance != null)
            {
                return;
            }

    //        if (IDebugSystemManager.Instance != null)
            {
                return;
            }

            if (_debugSystemManagerPrefab == null)
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "DebugSystemManager : PrefabがPersistentSceneLifetimeScopeに設定されていません。");
                return;
            }

            _debugSystemManagerInstance = Instantiate(_debugSystemManagerPrefab, transform);
        }
#endif
    }
}