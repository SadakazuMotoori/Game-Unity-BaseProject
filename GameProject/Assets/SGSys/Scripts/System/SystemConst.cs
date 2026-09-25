//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     SystemConst.cs
 *    @brief    システム用各種定数
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************

namespace SGSys 
{
    public static class SystemConst
    {
        //================================================
        //! デバッグ用グループ名
        //================================================
        public static class DebugGroup
        {
            public static string System		= "system";
            public static string Game		= "game";
            public static string Scene		= "scene";
        }
        
        //================================================
        //! UnityのShaderLabで用意されているレンダリングキュー
        //
        //http://docs.unity3d.com/ja/current/Manual/SL-SubshaderTags.html
        //================================================
        public static class RenderQueue
        {
            public const int	Background	= 1000;
            public const int	Geometry	= 2000;
            public const int	AlphaTest	= 2450;
            public const int	Transparent = 3000;
            public const int	Overlay		= 4000;
        }
        
        //================================================
        //! UnityのGameObjectのLayer値
        //================================================
        public static class Layer
        {
            public const int	Default			= 0;
            public const int	TransparentFx	= 1;
            public const int	IgnoreRaycast	= 2;
            public const int	Water			= 4;
            public const int	UI				= 5;
            public const int    UserDebug       = 31;
        }
        
        //================================================
        //! UnityのCameraのcullingMask値
        //================================================
        public static class LayerMask
        {
            public const int	Default			= (1<<Layer.Default);
            public const int	TransparentFx	= (1<<Layer.TransparentFx);
            public const int	IgnoreRaycast	= (1<<Layer.IgnoreRaycast);
            public const int	Water			= (1<<Layer.Water);
            public const int	UI				= (1<<Layer.UI);
            public const int    UserDebug       = (1<<Layer.UserDebug);
        }
        
        //================================================
        //! データファイルの読み込み方法
        //================================================
        public enum FileFromType : int
        {
              Server							//!< サーバーから
            , Storage							//!< 端末内ストレージから
            , StreamingAsset					//!< アプリ内StreamingAssetsフォルダから
        };
        
        //================================================
        //! 設定言語
        //================================================
        public enum Language : int
        {
              Unknown
            , Japanese					//!< 日本語
            , English					//!< 英語（アメリカおよび標準）
            , English_UK				//!< 英語（イギリス）
            , French					//!< フランス語
            , German					//!< ドイツ語
            , Italian					//!< イタリア語
            , Spanish					//!< スペイン語
            , Netherlandic				//!< オランダ語
            , Chinese_zh_Hant			//!< 中国語（繁体字）
            , Chinese_zh_Hans			//!< 中国語（簡体字）
            , Korean					//!< 韓国語
            
            , Chinese_Traditional = Chinese_zh_Hant
            , Chinese_Simplified  = Chinese_zh_Hans
        };
    
        //================================================
        //! 設定地域
        //================================================
        public enum Country : int
        {
              Unknown
            , Japan						//!< 日本
            , UnitedStates				//!< アメリカ
            , UnitedKingdom				//!< イギリス
            , France					//!< フランス
            , Germany					//!< ドイツ
            , Italy						//!< イタリア
            , Spain						//!< スペイン
            , Netherland				//!< オランダ
            , China						//!< 中国
            , Taiwan					//!< 台湾
            , Korea						//!< 韓国
        };
    }
}
