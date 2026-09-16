//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     PlatformEditor.cs
 *    @brief    Unityエディターモード時のユーティリティ
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
#if UNITY_EDITOR || (!UNITY_ANDROID && !UNITY_IOS)
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SGSys
{
	public class PlatformEditor : PlatformImpl
	{
		private string mSignature2;

		public PlatformEditor() : base("editor")
		{
		}
		
		public override void	Initialize()
		{
			mSignature2 = "0";
#if !UNITY_EDITOR
			this.name = Application.platform.ToString();
#endif
		}
		
		public override string	GetOsVersion()
		{
#if UNITY_EDITOR
			return "99.99.99";
#else
			return SystemInfo.operatingSystem;
#endif
		}

		public override string	GetDeviceName()
		{
#if UNITY_EDITOR
			return "UnityEditor";
#else
			return base.GetDeviceName();
#endif
		}

		public override string GetSignature()
		{
			return "unknown-signature-editor";
		}

		public override string GetSignature2()
		{
			return mSignature2;
		}
#if GAME_DEBUG
		public override void Debug_SetSignature2(string sig2)
		{
			mSignature2 = sig2;
		}
#endif

		public override void	ShowAlertDialog( string title, string body, string ok )
		{
			DebugLog.Warning( SystemConst.DebugGroup.System, "ShowAlertDialog : unimplement");
		}

		public override void ShowIndicator()
		{
			DebugLog.Warning( SystemConst.DebugGroup.System, "ShowIndicator : unimplement" );
		}

		public override void HideIndicator()
		{
			DebugLog.Warning( SystemConst.DebugGroup.System, "HideIndicator : unimplement" );
		}
		
		public override SystemConst.Language GetLanguage()
		{
#if UNITY_EDITOR
			return SystemConst.Language.Japanese;
#else
			return base.GetLanguage();
#endif
		}

		public override SystemConst.Country GetCountry()
		{
			return SystemConst.Country.Japan;
		}

		public override string GetDeviceUniqueId()
		{
			return SystemInfo.deviceUniqueIdentifier;
		}

		public override void EnableLog(bool enabled)
		{
		}


		public override void EnableBatteryMonitoring(bool enabled)
		{
		}

		public override float GetBatteryLevel()
		{
#if UNITY_EDITOR
			return 1.0f;	//Editor上は決め打ちの値
#else
			return base.GetBatteryLevel();
#endif
		}

		public override BatteryStatus GetBatteryStatus()
		{
#if UNITY_EDITOR
			return BatteryStatus.Full;	//Editor上は決め打ちの値
#else
			return base.GetBatteryStatus();
#endif
		}

		/// <summary>
		/// プリファレンス保存用パスの取得
		/// </summary>
		/// <param name="key">プリファレンスのキー</param>
		/// <returns>保存先パス</returns>
		private string GetPreferencePath( string key )
		{
#if UNITY_EDITOR
			string path = Application.temporaryCachePath + "/" + this.prefRoot + key;
#else
			string path = Application.persistentDataPath + "/" + this.prefRoot + key;
#endif
			return path;
		}

		public override string LoadPreference(string key)
		{
			string path = GetPreferencePath( key );
			System.IO.FileInfo fi = new System.IO.FileInfo(path);
			if ( !fi.Exists )
			{
				return null;
			}

			if ( 0 >= fi.Length )
			{
				return "";
			}

			using System.IO.StreamReader sr = new System.IO.StreamReader(path);
			string text = sr.ReadToEnd();
			sr.Close();
			return text;
		}

		public override void SavePreference(string key, string value)
		{
			string path = GetPreferencePath( key );
			System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName( path ) );
			using System.IO.StreamWriter sw = new System.IO.StreamWriter( path, false );
			sw.Write( value );
			sw.Close();
		}

		public override void DeletePreference(string key)
		{
			string path = GetPreferencePath( key );
			System.IO.File.Delete(path);
		}

		public override bool HasPreference(string key)
		{
			string path = GetPreferencePath( key );
			if ( System.IO.File.Exists( path ) )
			{
				return true;
			}
			return false;
		}

		public override void InitializePreference(string root)
		{
			this.prefRoot = root;
		}

		/// <summary>
		/// メッセージ共有
		/// </summary>
		/// <param name="subject"></param>
		/// <param name="title"></param>
		/// <param name="body"></param>
		public override void ShareMessage(string subject, string title, string body)
		{
			foreach ( char invalidChar in System.IO.Path.GetInvalidFileNameChars() )
			{
				subject = (subject ?? "").Replace( invalidChar, '_' );
			}
			var output = string.Format("{0}/{1}_{2}.txt", Application.persistentDataPath, subject, Utility.GetLocalDateTime(Utility.GetCurrentUnixTime()).ToString("yyyy-M-d_HHmmss"));
			if (!string.IsNullOrEmpty(output))
			{
				using System.IO.StreamWriter sw = new System.IO.StreamWriter(output, false);
				sw.Write(body);
				sw.Close();
#if UNITY_EDITOR
				EditorUtility.OpenWithDefaultApp(output);
#endif
			}
		}

		/// <summary>
		/// システムのクリップボードへテキストをコピー
		/// </summary>
		/// <param name="text"></param>
		public override void SetClipboardText(string text)
		{
			GUIUtility.systemCopyBuffer = text;
		}

		public override void WebViewRemoveAllCookie()
		{
		}


        /// <summary>
        /// ストレージの空き容量を計算する
        /// </summary>
        /// <returns></returns>
        public override long CalcStorageAvailableSize ()
		{
            var dir = System.IO.Directory.GetCurrentDirectory();
            var drive = System.IO.Path.GetPathRoot(dir);
            var di = new System.IO.DriveInfo(dir);
            return di.AvailableFreeSpace;
        }
    }
} //namespace SGLib

#endif //UNITY_EDITOR
