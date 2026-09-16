//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     PlatformAndroid.cs
 *    @brief    Android用ユーティリティ実装
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
#if UNITY_ANDROID
using UnityEngine;

namespace SGSys
{
    public class PlatformAndroid : PlatformImpl
    {
        private	AndroidJavaClass	mPlayer;
        private AndroidJavaClass    mBattery;
        private AndroidJavaClass    mClassEnvironment;

        private	AndroidJavaObject	mActivity;
        private AndroidJavaObject   mIndicator;

        private AndroidJavaClass    mClassIntent;
        private AndroidJavaClass    mClassBatteryManager;

        private	string				mOsVersion;
        private	string				mSignature;
        private string				mSignature2;

        private int mApiLevel;

        public PlatformAndroid() : base("android")
        {
        }
        
        public override void	Initialize()
        {
            mOsVersion			= "0.0";
            mSignature			= "unknown-signature";
            mSignature2			= "0";

            mPlayer     = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            mActivity   = mPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            mClassIntent        = new AndroidJavaClass("android.content.Intent");
            mClassBatteryManager= new AndroidJavaClass("android.os.BatteryManager");
            mClassEnvironment   = new AndroidJavaClass("android.os.Environment");


            //OSバージョン取得
            using( AndroidJavaClass buildClass = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                if ( null != buildClass )
                {
                    mOsVersion = buildClass.GetStatic<string>("RELEASE");
                    mApiLevel = buildClass.GetStatic<int>("SDK_INT");
#if GAME_DEBUG
                    DebugLog.Info("PlatformAndroid OS Version=["+mOsVersion+"] API Level=["+mApiLevel+"]" );
#endif
                }
            }
            if ( null != mActivity )
            {
                //インジケータプラグイン
                mIndicator = new AndroidJavaObject("jp.co.xeen.plugin.Indicator");
                if ( null != mIndicator ) {
                    mIndicator.CallStatic("Initialize");
                }
            }
            mBattery = new AndroidJavaClass("jp.co.xeen.plugin.Battery");

            using( AndroidJavaClass sig = new AndroidJavaClass("jp.co.xeen.plugin.Signature") )
            {
                if ( null != sig )
                {
                    sig.CallStatic("Initialize", "bin/Data/Managed/Metadata/global-metadata.dat");
                    mSignature = sig.CallStatic<string>("GetSignature",0);
                    mSignature2= sig.CallStatic<string>("GetSignature",1);
                }
            }
        }


    
        public override string	GetOsVersion()
        {
            return mOsVersion;
        }
        public override string GetSignature()
        {
            return mSignature;
        }
#if GAME_DEBUG
        public override void Debug_SetSignature2(string sig2)
        {
            mSignature2 = sig2;
        }
#endif
        public override string GetSignature2()
        {
            return mSignature2;
        }

        /// <summary>
        /// ダイアログの決定ボタンクリックリスナー
        /// </summary>
        /// <remarks>
        /// リスナーはJavaのスレッドから呼び出される為、onClick内で直接Unityの機能を利用してはいけない
        /// </remarks>
        class AlertDialogPositiveButtonListener : AndroidJavaProxy {
            private PlatformAndroid     mImpl;
            private int                 mValue;

            public AlertDialogPositiveButtonListener( PlatformAndroid impl ) : base("android.content.DialogInterface$OnClickListener")
            {
                mImpl = impl;
                mValue = -1;
            }
            public void onClick( AndroidJavaObject obj, int value )
            {
                mValue = value;
            }
        }

        /// <summary>
        /// ダイアログ表示
        /// </summary>
        /// <param name="title">タイトル文字列</param>
        /// <param name="body">本文</param>
        /// <param name="ok">決定ボタン文字列</param>
        public override void	ShowAlertDialog( string title, string body, string ok )
        {
            if ( null == mActivity )
            {
#if GAME_DEBUG
                DebugLog.Error("PlatformAndroid ShowAlertDialog : mActivity == null");
#endif //GAME_DEBUG
                return;
            }
            mActivity.Call("runOnUiThread", new AndroidJavaRunnable( ()=> {
                using( AndroidJavaObject builder = new AndroidJavaObject("android.app.AlertDialog$Builder", mActivity) )
                {
                    builder.Call<AndroidJavaObject>("setTitle",title);
                    builder.Call<AndroidJavaObject>("setMessage",body);
                    builder.Call<AndroidJavaObject>("setPositiveButton", ok, new AlertDialogPositiveButtonListener(this) );
                    AndroidJavaObject dialog = builder.Call<AndroidJavaObject>("create");
                    dialog.Call("show");
                }
            }));
        }


        /// <summary>
        /// インジケータ表示
        /// </summary>
        public override void ShowIndicator()
        {
            if ( null == mActivity )
            {
#if GAME_DEBUG
                DebugLog.Error("PlatformAndroid ShowIndicator : mActivity == null");
#endif
                return;
            }
            if ( null == mIndicator )
            {
#if GAME_DEBUG
                DebugLog.Error("PlatformAndroid ShowIndicator : mIndicator == null");
#endif
                return;
            }
            mIndicator.CallStatic("ShowSimple");
        }
        /// <summary>
        /// インジケータ消去
        /// </summary>
        public override void HideIndicator()
        {
            if ( null == mActivity )
            {
#if GAME_DEBUG
                DebugLog.Error("PlatformAndroid HideIndicator : mActivity == null");
#endif
                return;
            }
            if ( null == mIndicator )
            {
#if GAME_DEBUG
                DebugLog.Error("PlatformAndroid HideIndicator : mIndicator == null");
#endif
                return;
            }
            mIndicator.CallStatic("HideSimple");
        }

        /// <summary>
        /// バッテリーのモニタリング許可
        /// </summary>
        /// <param name="enabled"></param>
        public override void EnableBatteryMonitoring(bool enabled)
        {
        }


#if false   //UnityEngine.SystemInfo.batteryLevelが正しく無い時はこちらを利用する
        /// <summary>
        /// バッテリーの充電レベル取得
        /// </summary>
        /// <returns>0.0～1.0</returns>
        public override float GetBatteryLevel()
        {
            string action = mClassIntent.GetStatic<string>("ACTION_BATTERY_CHANGED");
            string level = mClassBatteryManager.GetStatic<string>("EXTRA_LEVEL");
            string scale = mClassBatteryManager.GetStatic<string>("EXTRA_SCALE");

            AndroidJavaObject filter = new AndroidJavaObject("android.content.IntentFilter",action);
            if ( null == filter )
            {
                return 1.0f;
            }
            AndroidJavaObject intent = mActivity.Call<AndroidJavaObject>("registerReceiver", null, filter );
            if ( null == intent )
            {
                return 1.0f;
            }

            int levelValue = intent.Call<int>("getIntExtra", level, -1 );
            int scaleValue = intent.Call<int>("getIntExtra", scale, -1 );

            float ret = (float)levelValue / scaleValue;
            return Mathf.Clamp01(ret);
        }
#endif

#if false   //UnityEngine.SystemInfo.batteryStatusが正しく無い時はこちらを利用する
        /// <summary>
        /// バッテリーの状態取得
        /// </summary>
        /// <returns></returns>
        public override BatteryStatus GetBatteryStatus()
        {
            string action = mClassIntent.GetStatic<string>("ACTION_BATTERY_CHANGED");

            AndroidJavaObject filter = new AndroidJavaObject("android.content.IntentFilter", action);
            if ( null == filter )
            {
                return BatteryStatus.Unknown;
            }
            AndroidJavaObject intent = mActivity.Call<AndroidJavaObject>("registerReceiver", null, filter );
            if ( null == intent )
            {
                return BatteryStatus.Unknown;
            }
            string statusAction = mClassBatteryManager.GetStatic<string>("EXTRA_STATUS");
            int status = intent.Call<int>("getIntExtra", statusAction, -1 );

            int statusCharning   = mClassBatteryManager.GetStatic<int>("BATTERY_STATUS_CHARGING");
            int statusFull       = mClassBatteryManager.GetStatic<int>("BATTERY_STATUS_FULL");
            int statusNotCharging= mClassBatteryManager.GetStatic<int>("BATTERY_STATUS_NOT_CHARGING");

            if ( status == statusCharning )
            {
                return BatteryStatus.Charging;
            }
            if ( status == statusFull )
            {
                return BatteryStatus.Full;
            }
            if ( status == statusNotCharging )
            {
                return BatteryStatus.NotCharging;
            }

            return BatteryStatus.Unknown;
        }
#endif    

        private void GetLocale( out string lang, out string country )
        {
            if ( null != mActivity ) {
                string locale = mActivity.CallStatic<string>("GetLocale");
                string[] splits = locale.Split('_');
                lang = splits[0];
                country = splits.Length > 1 ? splits[1] : "";
    //			DebugLog.Warning("GetLocale = "+locale + " Lang="+lang+" Country="+country);
            } else {
                lang = "jp";
                country = "JP";
            }
        }


        public override void EnableLog(bool enabled)
        {
#if PLATFORM_TEST_MODE
#else
            mActivity.CallStatic("EnableLog", enabled);
#endif
        }


        private string GetPrefDir()
        {
            if ( null == mActivity ) {
                return null;
            }
            var path = Application.persistentDataPath;
            return path + "/.xeen/" + this.prefRoot;
        }

        private string GetPrefPath( string key )
        {
            string path = GetPrefDir();
            if ( null == path )
            {
                return null;
            }
            path += "/" + GetPreferenceHash( key );
            return path;
        }

        private string GetPreferenceHash( string value )
        {
            using( var hash = System.Security.Cryptography.SHA256.Create() )
            {
                var bytes = new System.Text.UTF8Encoding( false, true ).GetBytes( value );
                return System.BitConverter.ToString( hash.ComputeHash( bytes ) ).Replace( "-", "" );
            }
        }

        public override void InitializePreference(string root)
        {
            this.prefRoot = GetPreferenceHash( root );
        }

        public override string LoadPreference(string key)
        {
            string path = GetPrefPath(key);
            if ( null == path ) {
#if GAME_DEBUG
                DebugLog.Warning("PlatformAndroid.LoadPreference : null");
#endif
                return null;
            }
#if GAME_DEBUG
            DebugLog.Info("PlatformAndroid.LoadPreference : " + path );
#endif

            System.IO.FileInfo fi = new System.IO.FileInfo(path);
            if ( !fi.Exists ) {
                return null;
            }
            if ( 0 >= fi.Length ) {
                return "";
            }

            using System.IO.StreamReader sr = new System.IO.StreamReader(path);
            if ( null == sr ) {
                return null;
            }
            string text = sr.ReadToEnd();
            sr.Close();
#if GAME_DEBUG
            DebugLog.Info("PlatformAndroid.LoadPreference : key="+key+" value="+text);
#endif
            return text;
        }

        public override void SavePreference(string key, string value)
        {
            string path = GetPrefPath( key );
            if ( null == path )
            {
#if GAME_DEBUG
                DebugLog.Warning("PlatformAndroid.SavePreference : path is null");
#endif
                return;
            }
#if GAME_DEBUG
            DebugLog.Info("PlatformAndroid.SavePreference : " + path );
#endif

            string pathDir = System.IO.Path.GetDirectoryName( path );
            if ( !System.IO.Directory.Exists( pathDir ) )
            {
#if GAME_DEBUG
                DebugLog.Info("PlatformAndroid.SavePreference : CreateDirectory="+pathDir );
#endif
                System.IO.Directory.CreateDirectory( pathDir );
            }

            using System.IO.StreamWriter sw = new System.IO.StreamWriter( path, false );
            if ( null == sw )
            {
                return;
            }
            sw.Write( value );
            sw.Close();

#if GAME_DEBUG
            DebugLog.Info("PlatformAndroid.SavePreference : key="+key+" value="+value);
#endif
        }

        public override void DeletePreference(string key)
        {
            string path = GetPrefPath( key );
            if ( null == path )
            {
                return;
            }
            System.IO.File.Delete(path);
#if GAME_DEBUG
            DebugLog.Info("PlatformAndroid.DeletePreference : key="+key );
#endif
        }

        public override bool HasPreference(string key)
        {
            string path = GetPrefPath( key );
            if ( System.IO.File.Exists( path ) )
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// メッセージ共有
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        public override void ShareMessage(string subject, string title, string body)
        {
            var intentClass = new AndroidJavaClass("android.content.Intent");
            var intentObject = new AndroidJavaObject("android.content.Intent");
            // intentのアクション設定
            intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
            intentObject.Call<AndroidJavaObject>("setType", "text/plain");
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), title);
            intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), body);

            // アクティビティを取得
            var unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
            // intent発行
            currentActivity.Call("startActivity", intentObject);
        }

        /// <summary>
        /// システムのクリップボードへテキストをコピー
        /// </summary>
        /// <param name="text"></param>
        public override void SetClipboardText(string text)
        {
            // アクティビティを取得
            var jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            // クリップボード取得
            var clipboard = jo.Call<AndroidJavaObject>("getSystemService", "clipboard");
            // クリップボードに渡すオブジェクトを作成
            var clipdata = new AndroidJavaClass("android.content.ClipData");
            var clipObject = clipdata.CallStatic<AndroidJavaObject>("newPlainText", "label", text);

            clipboard.Call("setPrimaryClip", clipObject);
        }



        public override string TestString()
        {
            if ( null == mActivity )
            {
                return "";
            }
            return mActivity.Call<string>("TestString");
        }

        public override void WebViewRemoveAllCookie()
        {
            var cookieManager = new AndroidJavaClass("android.webkit.CookieManager").CallStatic<AndroidJavaObject>("getInstance");
//			cookieManager.Call("setAcceptCookie", new object[] { true });
            cookieManager.Call("removeAllCookie");
        }

        /// <summary>
        /// ストレージの空き容量を計算する
        /// </summary>
        /// <returns></returns>
        public override long CalcStorageAvailableSize ()
        {
            var statFs = new AndroidJavaObject("android.os.StatFs", Application.temporaryCachePath );
            var availableBlocks = statFs.Call<long>("getAvailableBlocksLong");
            var blockSize = statFs.Call<long>("getBlockSizeLong");
            var freeBytes = availableBlocks * blockSize;
            return freeBytes;
        }
    }
} //namespace SGLib


#endif //UNITY_ANDROID
