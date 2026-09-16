//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     PlatformIos.cs
 *    @brief    iOS用ユーティリティ実装
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
#if UNITY_IOS
using UnityEngine;
using System.Runtime.InteropServices;


namespace SGSys
{
    public class PlatformIos : PlatformImpl
    {
        [DllImport ("__Internal")]	private static extern void SGSysUtility_Initialize();
        [DllImport ("__Internal")]	private static extern void SGSysUtility_ShowAlertDialog( string title, string body, string ok );
        [DllImport ("__Internal")]	private static extern void SGSysUtility_ShowIndicator();
        [DllImport ("__Internal")]	private static extern void SGSysUtility_HideIndicator();
        [DllImport ("__Internal")]	private static extern float SGSysUtility_EnableBatteryMonitoring( bool enabled );
        [DllImport ("__Internal")]	private static extern float SGSysUtility_GetBatteryLevel();
        [DllImport ("__Internal")]	private static extern int SGSysUtility_GetBatteryStatus();
        [DllImport ("__Internal")]	private static extern void SGSysUtility_ShareMessage( string subject, string title, string body );
        [DllImport ("__Internal")]	private static extern void SGSysUtility_SetClipboardText( string text );
        [DllImport ("__Internal")]	private static extern void SGSysUtility_WebViewRemoveAllCookie();
        [DllImport ("__Internal")]  private static extern long SGSysUtility_CalcStorageAvailableSize();

        public PlatformIos() : base("ios")
        {
        }
        
        public override void Initialize()
        {
            SGSysUtility_Initialize();

            GameObject obj = new GameObject("PlatformIos");
            GameObject systemRoot = GameObject.Find("GameManager");
            if (systemRoot != null)
            {
                obj.transform.SetParent(systemRoot.transform);
            }
        }
    
        public override string	GetOsVersion()
        {
            return UnityEngine.iOS.Device.systemVersion;
        }
        public override string	GetSignature()
        {
            return "ios-signature";
        }
        public override string	GetSignature2()
        {
            return "0";
        }
        

        public override void	ShowAlertDialog( string title, string body, string ok )
        {
            SGSysUtility_ShowAlertDialog( title, body, ok );
        }

        public override void ShowIndicator()
        {
            SGSysUtility_ShowIndicator();
        }
        
        public override void HideIndicator()
        {
            SGSysUtility_HideIndicator();
        }

        public override void EnableLog(bool enabled)
        {
        }

        public override void InitializePreference (string root) 
        {
        }

        public override string LoadPreference(string key)
        {
            return IOSKeyChain.GetData( key );
        }

        public override void SavePreference(string key, string value)
        {
            IOSKeyChain.SetData( key, value );
        }

        public override bool HasPreference(string key)
        {
            return IOSKeyChain.ContainsKey(key);
        }

        public override void DeletePreference(string key)
        {
            IOSKeyChain.DeleteData( key );
        }

        public override void EnableBatteryMonitoring(bool enabled)
        {
            SGSysUtility_EnableBatteryMonitoring( enabled );
        }

#if false   //UnityEngine.SystemInfo.batteryLevelが正しく無い時はこちらを利用する
        public override float GetBatteryLevel() {
            return SGSysUtility_GetBatteryLevel();
        }
#endif

#if false   //UnityEngine.SystemInfo.batteryStatusが正しく無い時はこちらを利用する
        public override BatteryStatus GetBatteryStatus() {
            return (BatteryStatus)SGSysUtility_GetBatteryStatus();
        }
#endif

        /// <summary>
        /// メッセージ共有
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="title"></param>
        /// <param name="body"></param>
        public override void ShareMessage(string subject, string title, string body)
        {
            SGSysUtility_ShareMessage(subject, title, body);
        }

        /// <summary>
        /// システムのクリップボードへテキストをコピー
        /// </summary>
        /// <param name="text"></param>
        public override void SetClipboardText(string text)
        {
            SGSysUtility_SetClipboardText(text);
        }


        public override void WebViewRemoveAllCookie()
        {
            SGSysUtility_WebViewRemoveAllCookie();
        }

        /// <summary>
        /// ストレージの空き容量を返す
        /// </summary>
        /// <returns></returns>
        public override long CalcStorageAvailableSize ()
        {
            return SGSysUtility_CalcStorageAvailableSize();
        }
    }


} //namespace SGSys

#endif //UNITY_IPHONE
