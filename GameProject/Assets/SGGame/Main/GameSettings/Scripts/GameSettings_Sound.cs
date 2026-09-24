using System.Collections.Generic;
using UnityEngine;

namespace SGGames.Game
{
    /// <summary>
    /// ゲーム設定（サウンドに関する情報）関連
    /// </summary>
    public sealed partial class GameSettings : MonoBehaviour
    {
        public class Sound
        {
            /// <summary>
            /// オーディオ関係設定最小値
            /// </summary>
            public const int PARAM_MIN = 0;
            /// <summary>
            /// オーディオ関係設定最大値
            /// </summary>
            public const int PARAM_MAX = 10;
            /// <summary>
            /// オーディオ関係未設定時初期値
            /// </summary>
            public const int PARAM_DEF = 10;

            /// <summary>
            /// BGM音量
            /// </summary>
            public int   bgmLevel
            {
                get {
                    return GetLevel(LevelType.Bgm);
                }
                set {
                    SetLevel(LevelType.Bgm,value);
                }
            }
            /// <summary>
            /// SE音量
            /// </summary>
            public int   seLevel
            {
                get {
                    return GetLevel(LevelType.Se);
                }
                set {
                    SetLevel( LevelType.Se, value );
                }
            }
        
            /// <summary>
            /// SE(ショット音）音量
            /// </summary>
            public int   shotLevel
            {
                get {
                    return GetLevel(LevelType.Shot);
                }
                set {
                    SetLevel(LevelType.Shot,value);
                }
            }

            /// <summary>
            /// ボイス音量
            /// </summary>
            public int   voiceLevel
            {
                get {
                    return GetLevel(LevelType.Voice);
                }
                set {
                    SetLevel( LevelType.Voice, value );
                }
            }

            /// <summary>
            /// 合いの手音量
            /// </summary>
            public int   clapLevel
            {
                get {
                    return GetLevel(LevelType.Clap);
                }
                set {
                    SetLevel( LevelType.Clap, value );
                }
            }

            /// <summary>
            /// バトル内共通SE
            /// </summary>
            public int  battleCommonLevel
            {
                get {
                    return GetLevel(LevelType.SeBattleCommon);
                }
                set {
                    SetLevel( LevelType.SeBattleCommon, value );
                }
            }
            /// <summary>
            /// バトル内ボスSE
            /// </summary>
            public int  battleBossLevel
            {
                get {
                    return GetLevel(LevelType.SeBattleBoss);
                }
                set {
                    SetLevel( LevelType.SeBattleBoss, value );
                }
            }
            /// <summary>
            /// バトル内ユニットSE
            /// </summary>
            public int  battleUnitLevel
            {
                get {
                    return GetLevel(LevelType.SeBattleUnit);
                }
                set {
                    SetLevel( LevelType.SeBattleUnit, value );
                }
            }

            private class Info
            {
                public string key { get; private set; }
                public int current;
                private int push;

                public Info( string _key ) {
                    this.key = _key;
                }

                public void Load() {
                    this.current = PlayerPrefs.GetInt( this.key, PARAM_DEF );
                }
                public void Save() {
                    PlayerPrefs.SetInt( this.key, this.current );
                }

                public void Reset() {
                    this.current = PARAM_DEF;
                }
                public void Push() {
                    this.push = this.current;
                }
                public void Pop() {
                    this.current = this.push;
                }
            }

            private List<Info> mLevelInfoList;

            private void SetLevel( LevelType lt, int value )
            {
                mLevelInfoList[(int)lt].current = value;
            }
            private int GetLevel( LevelType lt )
            {
                return mLevelInfoList[(int)lt].current;
            }

            enum LevelType
            {
                Bgm,
                Voice,
                Clap,
                Shot,
                Se,
                SeBattleCommon,
                SeBattleBoss,
                SeBattleUnit,
            }

            static string[] s_KeyNames = new string[]
            {
                "Sound.BgmLevel",
                "Sound.VoiceLevel",
                "Sound.ClapLevel",
                "Sound.ShotLevel",
                "Sound.SeLevel",
                "Sound.SeBattleCommonLevel",
                "Sound.SeBattleBossLevel",
                "Sound.SeBattleUnitLevel",
            };

            /// <summary>
            /// コンストラクタ
            /// </summary>
            public Sound()
            {
                int count = s_KeyNames.Length;
                mLevelInfoList = new List<Info>(count);

                for ( int i=0; i<count; ++i )
                {
                    var info = new Info( s_KeyNames[i] );
                    mLevelInfoList.Add( info );
                }
            }

            /// <summary>
            /// 設定状態の読み込み
            /// </summary>
            public void Load()
            {
                foreach( var info in mLevelInfoList )
                {
                    info.Load();
                }
            }
            /// <summary>
            /// 設定状態の保存
            /// </summary>
            public void Save()
            {
                foreach( var info in mLevelInfoList )
                {
                    info.Save();
                }
            }

            /// <summary>
            /// パラメータのリセット
            /// </summary>
            public void ResetSettings()
            {
                foreach( var info in mLevelInfoList )
                {
                    info.Reset();
                }
            }

            /// <summary>
            /// 現在の設定の退避
            /// </summary>
            public void Push()
            {
                foreach( var info in mLevelInfoList )
                {
                    info.Push();
                }
            }
            /// <summary>
            /// 退避した値の復帰
            /// </summary>
            public void Pop()
            {
                foreach( var info in mLevelInfoList )
                {
                    info.Pop();
                }
            }
        }
    }
}
