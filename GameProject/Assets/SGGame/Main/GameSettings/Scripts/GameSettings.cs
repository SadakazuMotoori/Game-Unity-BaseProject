using UnityEngine;

namespace SGGames.Game
{
    /// <summary>
    /// ゲーム設定（オプション）関連
    /// </summary>
    public sealed partial class GameSettings : MonoBehaviour
    {
        private const long MB = (1024*1024);
        private const long GB = (MB*1024);
        /// <summary>
        /// アプリをプレイするのに必要なサイズ（バイト）
        /// </summary>
        public const long REQUIRED_SIZE = (10*GB);

        /// <summary>
        /// アプリに必要な容量をGBサイズを少数第一位までの文字列で返す
        /// </summary>
        /// <returns>GBサイズ文字列</returns>
        public static string GetRequiredGBSizeString()
        {
            float temp = REQUIRED_SIZE / (float)GB;
            return string.Format("{0:F1}GB", temp );
        }

        /// <summary>
        /// ゲーム内言語定数
        /// 
        /// SGSys.SystemConst.Languageはあくまでシステムから返された言語定数で
        /// この定数はアプリ内で対応している言語となる。
        /// </summary>
        public enum Language
        {
            Japanese,
            English,
        }
        /// <summary>
        /// GameSettings.Languageと対となるSGSys.SystemConst.Language値
        /// </summary>
        /// <remarks>
        /// GameSettings.Languageを編集した時はこのテーブルも更新する
        /// </remarks>
        static SGSys.SystemConst.Language[] s_systemLanguageTable = new SGSys.SystemConst.Language[]
        {
            SGSys.SystemConst.Language.Japanese,
            SGSys.SystemConst.Language.English,
        };

        /// <summary>
        /// 設定言語
        /// </summary>
        private Language mLanguage;
        /// <summary>
        /// 起動時システム側から受け取った言語
        /// </summary>
        private Language mAwakeLanguage;

        private static GameSettings Instance    { get; set; }

        private Sound mSound;
/*
        private Play            mPlay;
        private Notification    mNotification;
        private Graphics        mGraphics;
*/
        private static Sound sound { get { return Instance.mSound; } }
/*
        private static Play play { get { return Instance.mPlay; } }
        private static Notification notification { get { return Instance.mNotification; } }
        private static Graphics graphics { get { return Instance.mGraphics; } }
*/

        void Awake()
        {
            Instance = this;

            mSound = new Sound();
/*            
            mPlay = new Play();
            mNotification = new Notification();
            mGraphics = new Graphics();
*/
            InitializeLanguage();
            LoadPrefs();
            InitializeForceQuit();

            GameSettings.startUnixTime = SGSys.Utility.GetCurrentUnixTime() - (long)Time.realtimeSinceStartup;
        }

        private void OnApplicationPause (bool pause)
        {
/*            
#if !UNITY_EDITOR && GAME_DEBUG
            Debug.Log("GameSettings.OnApplicationPause : "+pause);
#endif
            
            if ( pause ) {
                mPlay.SaveOnApplicationPause();
                PlayerPrefs.SetInt("Pause",1);
            } else {
                PlayerPrefs.SetInt("Pause",0);
            }
            PlayerPrefs.Save();
*/            
        }

        void InitializeLanguage()
        {  
            switch ( SGSys.Platform.GetLanguage() )
            {
                case SGSys.SystemConst.Language.Japanese:
                    mAwakeLanguage = Language.Japanese;
                    break;
                default:
                    mAwakeLanguage = Language.English;
                    break;
            }
            GameSettings.language = GameSettings.awakeLanguage;         
        }

        void InitializeForceQuit()
        {           
            if ( !PlayerPrefs.HasKey("Pause") )
            {
                PlayerPrefs.SetInt( "Pause", 1 );
            }
            int pause = PlayerPrefs.GetInt("Pause");
            if ( 0 < pause )
            {
                GameSettings.forceQuit = false;
            }
            else
            {
                GameSettings.forceQuit = true;
            }
            PlayerPrefs.SetInt("Pause",0);
            PlayerPrefs.Save();

#if !UNITY_EDITOR && GAME_DEBUG
            Debug.Log("GameSettings.InitializeForceQuit : " + GameSettings.forceQuit );
#endif
        }

        /// <summary>
        /// 保存されている設定内容のロード
        /// </summary>
        void LoadPrefs()
        {
            mLanguage = (Language)PlayerPrefs.GetInt("Language", (int)Language.Japanese );
            mSound.Load();
/*
            mGraphics.Load();
            mPlay.Load();
            mNotification.Load();
*/            
        }

        /// <summary>
        /// GameSetting作成
        /// </summary>
        public static void Create()
        {           
            Component gameManager = IGameManager.Instance as Component;
            if (gameManager == null)
            {
                return;
            }

            gameManager.gameObject.AddComponent<GameSettings>();           
        }
    }
}
