//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     Platform.cs
 *    @brief    プラットフォームに依存する処理
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;

namespace SGSys
{
    //************************************************
    //************************************************
    // 実装用基底クラス
    //************************************************
    //************************************************
    public abstract class PlatformImpl
    {
        public string	name			{ get; protected set; }				//!< プラットフォーム名
        public string	prefRoot		{ get; protected set; }
        public string   advertisingId   { get; protected set; }

        protected PlatformImpl( string name_ )
        {
            this.name = name_;
            this.prefRoot = "";
            this.advertisingId = null;
        }

    
        //==========================================================================
        /**
         *    @brief       プラットフォーム固有初期化
         */
        //==========================================================================
        public abstract void Initialize();

    
        /// <summary>
        /// アプリケーションバージョン取得
        /// </summary>
        /// <returns></returns>
        public virtual string GetApplicationVersion()
        {
            return Application.version;
        }

        /// <summary>
        /// OSバージョン取得
        /// </summary>
        /// <returns></returns>
        public virtual string GetOsVersion()
        {
            return "0.0.0";
        }

        /// <summary>
        /// デバイス固有IDの取得
        /// </summary>
        /// <remarks>
        /// ここで取得できるIDは基本的にそのデバイス固有のIDですが、アプリケーションを再インストールした場合
        /// 値が変わる場合がありますので注意して下さい。
        /// </remarks>
        /// <returns>固有ID</returns>
        public virtual string GetDeviceUniqueId()
        {
            return SystemInfo.deviceUniqueIdentifier;
        }

        /// <summary>
        /// デバイス名取得
        /// </summary>
        /// <returns></returns>
        public virtual string GetDeviceName()
        {
            return SystemInfo.deviceModel;
        }
        
        /// <summary>
        /// 署名取得
        /// </summary>
        /// <returns></returns>
        public abstract string GetSignature();

        /// <summary>
        /// 署名その2取得
        /// </summary>
        /// <returns></returns>
        public abstract string GetSignature2();



        /// <summary>
        /// 広告ID取得リクエスト
        /// </summary>
        public virtual void RequestAdvertisingId()
        {
            DebugLog.Info("RequestAdvertisingId");
            bool ret = Application.RequestAdvertisingIdentifierAsync( (string adid, bool trackable, string error)=>{
#if GAME_DEBUG
                DebugLog.Info("Platform.RequestAdvertisingId : adid="+adid+" trackable="+trackable+" error="+error);
#endif
                this.advertisingId = adid;
            });

        }
        /// <summary>
        /// 広告IDの取得
        /// </summary>
        /// <remarks>
        /// 事前にRequestAdvertisingIdしておく必要あり
        /// </remarks>
        /// <returns>nullの場合取得できず</returns>
        public virtual string GetAdvertisingId()
        {
            return this.advertisingId;
        }

#if GAME_DEBUG
        public virtual void Debug_SetSignature2( string sig2 )
        {
        }
#endif

        /// <summary>
        /// 設定言語取得
        /// </summary>
        /// <returns></returns>
        public virtual SystemConst.Language GetLanguage()
        {
            switch( Application.systemLanguage )
            {
                case SystemLanguage.Japanese:
                    return SystemConst.Language.Japanese;
                case SystemLanguage.English:
                    return SystemConst.Language.English;
                case SystemLanguage.French:
                    return SystemConst.Language.French;
                case SystemLanguage.German:
                    return SystemConst.Language.German;
                case SystemLanguage.Italian:
                    return SystemConst.Language.Italian;
                case SystemLanguage.Spanish:
                    return SystemConst.Language.Spanish;
                case SystemLanguage.Dutch:
                    return SystemConst.Language.Netherlandic;
                case SystemLanguage.Korean:
                    return SystemConst.Language.Korean;
                case SystemLanguage.ChineseSimplified:
                    return SystemConst.Language.Chinese_Simplified;
                case SystemLanguage.ChineseTraditional:
                    return SystemConst.Language.Chinese_Traditional;
            }

            return SystemConst.Language.Japanese;
        }

        /// <summary>
        /// 設定地域取得
        /// </summary>
        /// <returns></returns>
        public virtual SystemConst.Country GetCountry()
        {
            return SystemConst.Country.Japan;
        }

        //==========================================================================
        /**
         *    @brief       アラートダイアログ表示
         *
         *    @param[in]   title    ダイアログタイトル
         *    @param[in]   body     ダイアログ本文
         *    @param[in]   ok       OKボタンメッセージ
         */
        //==========================================================================
        public abstract void ShowAlertDialog( string title, string body, string ok );
        
        //==========================================================================
        /**
         *    @brief       簡易インジケーター表示
         */
        //==========================================================================
        public abstract void ShowIndicator();

        //==========================================================================
        /**
         *    @brief       簡易インジケーター非表示
         */
        //==========================================================================
        public abstract void HideIndicator();
        
        


        /// <summary>
        /// 固有情報の初期化
        /// </summary>
        /// <param name="root">ルートのパス（オプション）</param>
        /// <remarks>
        /// rootはAndroid等キーチェーンが存在しない環境において、固有情報を保存する場所を指定したパス等に使用
        /// </remarks>
        public abstract void InitializePreference( string root );

        /// <summary>
        /// 固有情報の読み込み
        /// </summary>
        /// <param name="key">情報のキー</param>
        /// <returns>keyに紐づいている値</returns>
        /// <remarks>
        /// PlayerPrefsはアプリ自体を削除するとその存在が消えてしまう機能です。
        /// この機能はアプリを削除してもその値が残るようにするための機能となります。
        /// iOSではキーチェーン、Androidでは外部に設けた隠しファイルでそれを管理します。
        /// </remarks>
        public abstract string LoadPreference( string key );

        /// <summary>
        /// 固有情報の保存
        /// </summary>
        /// <param name="key">情報のキー</param>
        /// <param name="value">keyに紐づいている値</param>
        public abstract void SavePreference( string key, string value );

        /// <summary>
        /// 固有情報のキーの存在確認
        /// </summary>
        /// <param name="key">確認対象のキー</param>
        /// <returns>trueの時、keyが保存されている</returns>
        public abstract bool HasPreference( string key );

        /// <summary>
        /// 固有情報の削除
        /// </summary>
        /// <param name="key">削除対象のキー</param>
        public abstract void DeletePreference( string key );


        /// <summary>
        /// ネイティブ側へのログ表示制御
        /// </summary>
        /// <param name="enabled">trueの時、表示ON</param>
        public abstract void EnableLog( bool enabled );

        /// <summary>
        /// バッテリー関連監視許可
        /// </summary>
        /// <param name="enabled">trueにしてGetBattery系の処理を有効にします</param>
        public abstract void EnableBatteryMonitoring( bool enabled );

        /// <summary>
        /// バッテリー残量を取得
        /// </summary>
        /// <returns>0.0～1.0の値</returns>
        public virtual float GetBatteryLevel()
        {
            return SystemInfo.batteryLevel;
        }

        /// <summary>
        /// バッテリーの状態を取得
        /// </summary>
        /// <returns>BatteryStatus</returns>
        public virtual BatteryStatus GetBatteryStatus()
        {
            switch ( SystemInfo.batteryStatus )
            {
                case UnityEngine.BatteryStatus.Charging:
                    return BatteryStatus.Charging;

                case UnityEngine.BatteryStatus.Full:
                    return BatteryStatus.Full;

                case UnityEngine.BatteryStatus.NotCharging:
                    return BatteryStatus.NotCharging;

                case UnityEngine.BatteryStatus.Discharging:
                    return BatteryStatus.NotCharging;
            }

            return BatteryStatus.Unknown;
        }

        /// <summary>
        /// メッセージ共有
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        public abstract void ShareMessage(string subject, string title, string body);

        /// <summary>
        /// システムのクリップボードへテキストをコピー
        /// </summary>
        /// <param name="text"></param>
        public abstract void SetClipboardText(string text);


        public virtual string TestString()
        {
            return "";
        }

        /// <summary>
        /// WebViewのCookieを全て削除
        /// </summary>
        public abstract void WebViewRemoveAllCookie();

        /// <summary>
        /// ストレージの空き容量を返す
        /// </summary>
        /// <returns>容量（バイト）</returns>
        public abstract long CalcStorageAvailableSize();
    }

    /// <summary>
    /// バッテリー状態
    /// </summary>
    public enum BatteryStatus
    {
        Unknown,			//!< 不明
        Full,				//!< 充電完了
        Charging,			//!< 充電中
        NotCharging,		//!< 充電されていない
    }

    //************************************************
    //************************************************
    // 公開クラス
    //************************************************
    //************************************************
    public class Platform {
        private	static	PlatformImpl		mImpl;
    
        public	static	string				name		{ get { return mImpl.name; } }
    

#if GAME_DEBUG
        public static string	debugAppVersion	{ get; set; }	//!< デバッグ強制バージョン設定用
#endif
        //==========================================================================
        /**
         *    @brief       インスタンス生成と初期化
         */
        //==========================================================================
        public static void Create()
        {
#if UNITY_EDITOR
            mImpl = new PlatformEditor();
#elif UNITY_ANDROID
            mImpl = new PlatformAndroid();
#elif UNITY_IOS
            mImpl = new PlatformIos();
#elif UNITY_WEBPLAYER
            mImpl = new PlatformWebPlayer();
#else
            mImpl = new PlatformEditor();
#endif
            mImpl.Initialize();

#if GAME_DEBUG
            Platform.debugAppVersion = null;
#endif
        }

        //==========================================================================
        /**
         *    @brief       アプリケーションバージョンの取得
         */
        //==========================================================================
        public static string GetApplicationVersion()
        {
#if GAME_DEBUG
            if ( !string.IsNullOrEmpty(Platform.debugAppVersion) )
            {
                return Platform.debugAppVersion;
            }
#endif
            return mImpl.GetApplicationVersion();
        }

        //==========================================================================
        /**
         *    @brief       OSバージョンの取得
         */
        //==========================================================================
        public static string GetOsVersion()
        {
            return mImpl.GetOsVersion();
        }

        /// <summary>
        /// 署名取得
        /// </summary>
        /// <returns></returns>
        public static string GetSignature()
        {
            return mImpl.GetSignature();
        }

        /// <summary>
        /// 署名その2取得
        /// </summary>
        /// <returns></returns>
        public static string GetSignature2()
        {
            return mImpl.GetSignature2();
        }

#if GAME_DEBUG
        public static void Debug_SetSignature2( string sig2 )
        {
            mImpl.Debug_SetSignature2( sig2 );
        }
#endif //GAME_DEBUG

        //==========================================================================
        /**
         *    @brief       デバイス名の取得
         */
        //==========================================================================
        public static string GetDeviceName()
        {
            return mImpl.GetDeviceName();
        }

        /// <summary>
        /// デバイス名 + OSバージョン文字列の取得
        /// </summary>
        /// <returns></returns>
        public static string GetDeviceNameAndOsVersion()
        {
            return string.Format( "{0} - {1}", GetDeviceName(), GetOsVersion() );
        }

        /// <summary>
        /// デバイス情報の取得
        /// </summary>
        /// <returns>"デバイス名" - "OSバージョン" - "アプリバージョン"</returns>
        public static string GetDeviceInfo()
        {
            return string.Format( "{0} - {1} - {2}", GetDeviceName(), GetOsVersion(), GetApplicationVersion() );
        }

        //==========================================================================
        /**
         *    @brief       OKボタンのみのアラートダイアログ表示
         *
         *    @param[in]   title    タイトル文
         *    @param[in]   body     本文
         *    @param[in]   ok       OKボタン文字列
         */
        //==========================================================================
        public static void ShowAlertDialog( string title, string body, string ok )
        {
            mImpl.ShowAlertDialog( title, body, ok );
        }

        //==========================================================================
        /**
         *    @brief       簡易インジケーターの表示
         */
        //==========================================================================
        public static void ShowIndicator()
        {
            mImpl.ShowIndicator();
        }

        //==========================================================================
        /**
         *    @brief       簡易インジケーターの消去
         */
        //==========================================================================
        public static void HideIndicator()
        {
            mImpl.HideIndicator();
        }
        
        
        //==========================================================================
        /**
         *    @brief       設定言語を取得する
         */
        //==========================================================================
        public static SystemConst.Language GetLanguage()
        {
            return mImpl.GetLanguage();
        }

        //==========================================================================
        /**
         *    @brief       設定地域を取得する
         */
        //==========================================================================
        public static SystemConst.Country GetCountry()
        {
            return mImpl.GetCountry();
        }

        /// <summary>
        /// デバイス固有IDの取得
        /// </summary>
        /// <remarks>
        /// ここで取得できるIDは基本的にそのデバイス固有のIDですが、アプリケーションを再インストールした場合
        /// 値が変わる場合がありますので注意して下さい。
        /// </remarks>
        /// <returns>固有ID</returns>
        public static string GetDeviceUniqueId()
        {
            return mImpl.GetDeviceUniqueId();
        }

        /// <summary>
        /// ネイティブ側へのログ表示制御
        /// </summary>
        /// <param name="enabled">trueの時、表示ON</param>
        public static void EnableLog( bool enabled )
        {
            mImpl.EnableLog( enabled );
        }

        /// <summary>
        /// 固有情報の読み込み
        /// </summary>
        /// <param name="key">情報のキー</param>
        /// <returns>keyに紐づいている値</returns>
        /// <remarks>
        /// PlayerPrefsはアプリ自体を削除するとその存在が消えてしまう機能です。
        /// この機能はアプリを削除してもその値が残るようにするための機能となります。
        /// iOSではキーチェーン、Androidでは外部に設けた隠しファイルでそれを管理します。
        /// </remarks>
        public static string LoadPreference( string key )
        {
            if ( !HasPreference( key ) )
            {
                return null;
            }

            return mImpl.LoadPreference( key );
        }

        /// <summary>
        /// 固有情報の保存
        /// </summary>
        /// <param name="key">情報のキー</param>
        /// <param name="value">keyに紐づいている値</param>
        public static void SavePreference( string key, string value )
        {
            mImpl.SavePreference( key, value );
        }

        /// <summary>
        /// 固有情報のキーの存在確認
        /// </summary>
        /// <param name="key">確認対象のキー</param>
        /// <returns>trueの時、keyが保存されている</returns>
        public static bool HasPreference( string key )
        {
            return mImpl.HasPreference( key );
        }

        /// <summary>
        /// 固有情報のキーの削除
        /// </summary>
        /// <param name="key">確認対象のキー</param>
        public static void DeletePrefernce( string key )
        {
            mImpl.DeletePreference(key);
        }

        /// <summary>
        /// 固有情報の初期化
        /// </summary>
        /// <param name="root">特殊なフォルダなど管理する時の引数</param>
        public static void InitializePreference( string root="" )
        {
            mImpl.InitializePreference( root );
        }

        /// <summary>
        /// バッテリー関連の処理を許可する
        /// </summary>
        /// <param name="enabled"></param>
        public static void EnableBatteryMonitoring( bool enabled )
        {
            mImpl.EnableBatteryMonitoring( enabled );
        }

        /// <summary>
        /// バッテリー残量の取得
        /// </summary>
        /// <returns>0.0～1.0の値</returns>
        public static float GetBatteryLevel()
        {
            return mImpl.GetBatteryLevel();
        }

        /// <summary>
        /// バッテリー状態の取得
        /// </summary>
        /// <returns>BatteryStatus</returns>
        public static BatteryStatus GetBatteryStatus()
        {
            return mImpl.GetBatteryStatus();
        }


        /// <summary>
        /// メッセージ共有
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        public static void ShareMessage(string subject, string title, string body)
        {
            mImpl.ShareMessage(subject, title, body);
        }

        /// <summary>
        /// システムのクリップボードへテキストをコピー
        /// </summary>
        /// <param name="text"></param>
        public static void SetClipboardText(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        /// <summary>
        /// システムのクリップボード文字列を取得
        /// </summary>
        /// <returns></returns>
        public static string GetClipboardText()
        {
            return GUIUtility.systemCopyBuffer;
        }

        public static string TestString() {
#if GAME_DEBUG
            return mImpl.TestString();
#else
            return "";
#endif
        }

        /// <summary>
        /// WebViewのCookieを全て削除
        /// </summary>
        public static void WebViewRemoveAllCookie()
        {
            mImpl.WebViewRemoveAllCookie();
        }


        /// <summary>
        /// 広告IDの取得要求
        /// </summary>
        public static void RequestAdvertisingId()
        {
            mImpl.RequestAdvertisingId();
        }

        /// <summary>
        /// 広告IDの取得
        /// </summary>
        /// <remarks>
        /// 事前にRequestAdvertisingIdしておく必要あり
        /// </remarks>
        /// <returns>nullの場合取得できず</returns>
        public static string GetAdvertisingId()
        {
            return mImpl.GetAdvertisingId();
        }


        private const long KB = (1024);
        private const long MB = (1024 * KB);
        private const long GB = (1024 * MB);

        /// <summary>
        /// ストレージの空き容量をバイト単位で求める
        /// 処理に時間がかかる場合があるので、リアルタイムに呼び出さないようにして下さい
        /// </summary>
        /// <returns></returns>
        public static long CalcStorageAvailableSize()
        {
            return mImpl.CalcStorageAvailableSize();
        }

        /// <summary>
        /// ストレージの空き容量をメガバイト単位で求める
        /// 処理に時間がかかる場合があるので、リアルタイムに呼び出さないようにして下さい
        /// </summary>
        /// <returns>空き容量（ギガバイト）</returns>
        public static long CalcStorageAvailableSizeMB()
        {
            var size = CalcStorageAvailableSize() / MB;
            return size;
        }

        /// <summary>
        /// ストレージの空き容量をギガバイト単位で求める
        /// 処理に時間がかかる場合があるので、リアルタイムに呼び出さないようにして下さい
        /// </summary>
        /// <returns>空き容量（メガバイト）</returns>
        public static long CalcStorageAvailableSizeGB()
        {
            long size = CalcStorageAvailableSize() / GB;
            return size;
        }

    }
} //namespace SGLib
