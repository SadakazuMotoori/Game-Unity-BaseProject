using Cysharp.Threading.Tasks;
using UnityEngine;

using SGSys;

namespace SGGames.Game
{
    public interface IGameManager : IService<IGameManager>
    {
    }

    public class GameManager : MonoBehaviour, IGameManager
    {
        private static GameManager Instance => IGameManager.Instance as GameManager;

        void Awake()
        {
            UnityEngine.Object currentManager = IGameManager.Instance as UnityEngine.Object;
            if (currentManager != null && currentManager != this)
            {
                Destroy(gameObject);
                return;
            }

            ServiceLocator<IGameManager>.Register(this);

            Screen.orientation = ScreenOrientation.LandscapeLeft;

            // システム系初期化
            SGSys.Platform.Create();

            // ゲーム系初期化
            SGGames.Game.GameSettings.Create();
/*
            SGSys.SystemManager.Create();
            SGSys.Gfx.MaterialManager.Create();
            SGSys.Gfx.SkinnedMeshCombine.InitNoDrawMaterial();
            SGSys.MessageManager.Create();

            XeApp.Game.GameAssetPath.Initialize();
            XeApp.Game.Net.NetManager.Create();
            XeApp.Game.GameAssetLoader.Create();
            XeApp.Game.MagiAvatarManager.Create();
            XeApp.Game.ServerSettings.Initialize();
            XeApp.Game.GameSound.Create();
            XeApp.Game.FxManager.Create();
            XeApp.Game.GameSettings.Create();
            XeApp.Game.GameRenderUtil.Initialize();
            XeApp.Game.FileInstaller.Create();
            XeApp.Game.StencilMapManager.Create();
            XeApp.RawImageLoader.Create();
            XeApp.Game.CutSceneManager.Create();
            XeApp.Game.AvatarSnapShot.Create( this.gameObject );
            XeApp.Game.FirebaseUtil.Create();
            XeApp.Game.GyroController.Create();
            HashedPath.Create();

            XeApp.Game.UIAvatarSnapManager.Create();
            XeApp.Game.LayoutResourcesLoader.Create();
            XeApp.Game.LayoutResourcesManager.Create();
            XeApp.Game.LayoutInstanceManager.Create();
            XeApp.Game.InGameNotificationManager.Create();
            //XeApp.Game.SceneLayoutManager.Create();
            AddComponent<XeApp.Game.SceneLayoutManager>();
            XeSys.uGUI.uGUIInputControl.Create();
            //XeSys.uGUI.uGUILayoutManager.Create();
            uGUIManagerUpdateBehaviour.CreateObject(this.transform);
            LoadTextureResourcesManager.Create();
            LoadTextureResourcesManager.Instance.StandbyReceiver(3);

            XeApp.Game.Menu.BroadcastInfoManager.Create();
            XeApp.Game.Menu.DressCodeManager.Create();

            XeApp.Game.Net.NetErrorHandler.Create();

            AddComponent<XeApp.Game.CriMovieManager>();

#if GAME_DEBUG
            XeSys.DebugCheatMenu.buttonFontSize = 30;
            XeSys.DebugCheatMenu.buttonHeight = 70.0f;
            XeSys.DebugCheatMenu.Instance.Create( XeSys.Gfx.RenderManager.systemCanvas, Resources.GetBuiltinResource<Font>("Arial.ttf") );
            XeSys.DebugCheatMenu.CloseMenu();

            EasyPopUp.Initialize();
            EasyProgressBar.Initialize();
            EasyIndicator.Initialize();

            DebugUtil.Initialize();
#endif
*/
        }

        void OnDestroy()
        {
            UnityEngine.Object currentManager = IGameManager.Instance as UnityEngine.Object;
            if (currentManager == this)
            {
                ServiceLocator<IGameManager>.Unregister();
            }
        }

        #region 外部公開
        /// <summary>
        /// GameManagerオブジェクト以下に対象のオブジェクトを加える
        /// </summary>
        /// <param name="child">子オブジェクトのTransform</param>
        public static void AddChild( Transform child )
        {
            child.SetParent( Instance.transform, false );
        }

        /// <summary>
        /// GameManagerオブジェクト以下に対象のGameObjectを加える
        /// </summary>
        /// <param name="child">子オブジェクトとして追加するGameObject</param>
        public static void AddChild( GameObject child )
        {
            child.transform.SetParent( Instance.transform, false );
        }

        /// <summary>
        /// GameManagerにコンポーネントとして追加する
        /// </summary>
        /// <typeparam name="T">対象の型</typeparam>
        /// <returns>追加したコンポーネントのインスタンス</returns>
        public static T AddComponent<T>() where T : MonoBehaviour
        {
            T component = Instance.gameObject.AddComponent<T>();
            return component;
        }


        /// <summary>
        /// アプリケーション更新ページを開く
        /// </summary>
        public static void OpenApplicationUpdatePage()
        {
/*
#if GAME_DEBUG
            SGSys.DebugLog.Info("アプリ更新ページ : " + ServerSettings.urlApplication );
#endif
            Application.OpenURL( ServerSettings.urlApplication );
*/
        }
        /// <summary>
        /// お問い合わせページを開く
        /// </summary>
        public static void OpenInquiryPage()
        {
/*
#if GAME_DEBUG
            SGSys.DebugLog.Info("お問い合わせページ : " + ServerSettings.urlInquiry );
#endif
            Application.OpenURL( ServerSettings.urlInquiry );
*/
        }
      
        /// <summary>
        /// Game関連初期化処理
        /// </summary>
        public static void Initialize()
        {
            if ( GameManager.isInitialized )
            {
#if GAME_DEBUG
                SGSys.DebugLog.Warning("GameManager.Initialize : 実施済み");
#endif
                return;
            }
            if ( Instance == null )
            {
                return;
            }
            Co_Initialize().Forget();
        }
        public static async UniTask Co_Initialize()
        {
            await UniTask.DelayFrame(1);
/*
            yield return new WaitUntil( ()=>GameSound.IsInitialized() );

            FirebaseUtil.Initialize();
            yield return new WaitUntil( ()=>FirebaseUtil.isInitialized );

            
            Common.Database.LoadOnStartup();
            yield return new WaitWhile( ()=>Common.Database.isLoading );

            AppMessageManager.Instance.LoadOnStartup();
            yield return new WaitWhile( ()=>AppMessageManager.Instance.IsLoading() );
            
#if GAME_DEBUG
            DebugShow("データベース");
            Common.Database.Load();
            DebugHide();
            
            DebugShow("サウンドロード");
            yield return Instance.StartCoroutine( GameSound.Debug_LoadBank0() );
            DebugHide();

            DebugShow("シェーダーロード");
            ShaderCommonAsset.Load();
            yield return new WaitUntil( ()=>ShaderCommonAsset.isLoaded );
            DebugHide();
#endif
*/
            GameManager.isInitialized = true;
        }
        /// <summary>
        /// Initialize完了フラグ
        /// </summary>
        public static bool isInitialized { get; private set; }

#if GAME_DEBUG
        private static void DebugShow( string text )
        {
/* 
            SGSys.Gfx.RenderManager.fader.Fade( 0.0f, new Color(0,0,0,0.5f) );
            EasyPopUp.Instance.Open( text, 400f, 200f );
*/
        }
        private static void DebugHide()
        {
/*
            EasyPopUp.Instance.Close();
            SGSys.Gfx.RenderManager.fader.Fade( 0.0f, Color.clear );
*/
        }
#endif

        #endregion
    }
}
