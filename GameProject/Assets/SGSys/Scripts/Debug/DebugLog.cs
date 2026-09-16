//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     DebugLog.cs
 *    @brief    デバッグ用ログ表示クラス
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SGSys
{
    public class DebugLog
    {
#if GAME_DEBUG
        private static string	DEFAULT_GROUP_NAME = "default";
        private static DebugLog	mInstance = new DebugLog();


        public  static bool enabled;
        
        class GroupInfo {
            string	mName;				//!< グループ名.
            bool	mDisplay;			//!< ログ表示フラグ.
            
            public	string	Name	{ get { return mName; } }
            
            public GroupInfo( string name )
            {
                mName	= name;
                mDisplay= true;
            }
            
            public void SetDisplay( bool display )
            {
                mDisplay = display;
            }

            public bool IsDisplay()
            {
                return mDisplay;
            }
        }
        private Dictionary<string,GroupInfo>	mDicGroup;
#endif //GAME_DEBUG
        
        //==========================================================================
        /**
         *    @brief       コンストラクタ.
         */
        //==========================================================================
        public DebugLog()
        {
#if GAME_DEBUG
            mInstance = this;
            mDicGroup = new Dictionary<string,GroupInfo>();

            GroupInfo info = new GroupInfo( DEFAULT_GROUP_NAME );
            mDicGroup.Add( DEFAULT_GROUP_NAME, info );

            DebugLog.enabled = true;
#endif
        }

        
        //==========================================================================
        /**
         *    @brief       表示グループの追加.
         */
        //==========================================================================
        public static void AddGroup( string groupName )
        {
#if GAME_DEBUG
            if ( mInstance.mDicGroup.ContainsKey( groupName ) )
            {
                return;
            }
            GroupInfo info = new GroupInfo( groupName );
            mInstance.mDicGroup.Add( groupName, info );
#endif
        }

        //==========================================================================
        /**
         *    @brief       表示グループの削除.
         */
        //==========================================================================
        public static void RemoveGroup( string groupName )
        {
#if GAME_DEBUG
            if ( !mInstance.mDicGroup.ContainsKey( groupName ) )
            {
                DebugLog.Error("DebugLog.RemoveGroup : ["+groupName+"] is not exist" );
                return;
            }
            mInstance.mDicGroup.Remove( groupName );
#endif
        }
        
        //==========================================================================
        /**
         *    @brief       グループの表示設定.
         */
        //==========================================================================
        public static void SetDisplay( string groupName, bool display )
        {
#if GAME_DEBUG
            if ( !mInstance.mDicGroup.ContainsKey( groupName ) ) {
                DebugLog.Error("DebugLog.SetDisplay : ["+groupName+"] is not exist" );
                return;
            }
            
            GroupInfo info = mInstance.mDicGroup[groupName];
            info.SetDisplay( display );
#endif
        }

        private static bool IsDisp()
        {
#if GAME_DEBUG
            if ( !Debug.isDebugBuild )
            {
                return false;
            }

            if ( !DebugLog.enabled )
            {
                return false;
            }
            return true;
#else
            return false;
#endif
        }


        private static bool IsEnabled( string groupName )
        {
#if GAME_DEBUG
            if ( !IsDisp() )
            {
                return false;
            }

            GroupInfo info = null;
            if ( !mInstance.mDicGroup.TryGetValue( groupName, out info ) )
            {
                return false;
            }
            return info.IsDisplay();
#else
            return false;
#endif
        }


        //グループ名指定可能版.
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Info( string groupName, object message )
        {
#if GAME_DEBUG
            if ( !IsEnabled( groupName ) )
            {
                return;
            }
            Debug.Log( "["+groupName+"] " + message );
#endif
        }
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Warning( string groupName, object message )
        {
#if GAME_DEBUG
            if ( !IsEnabled( groupName ) )
            {
                return;
            }
            Debug.LogWarning( "["+groupName+"] " + message );
#endif
        }
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Error( string groupName, object message )
        {
#if GAME_DEBUG
            if ( !IsEnabled( groupName ) )
            {
                return;
            }
            Debug.LogError( "["+groupName+"] " + message );
#endif
        }
        
        //! Debug.Logのラッパー処理 	
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Info( object message )
        {
#if GAME_DEBUG
            Info( DEFAULT_GROUP_NAME, message );
#endif
        }
        //! Debug.LogWarningのラッパー処理 	
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Warning( object message )
        {
#if GAME_DEBUG
            Warning( DEFAULT_GROUP_NAME, message );
#endif
        }
        //! Debug.LogErrorのラッパー処理 	
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Error( object message ) {
#if GAME_DEBUG
            Error( DEFAULT_GROUP_NAME, message );
#endif
        }
    }
}	//namespace SGSys

