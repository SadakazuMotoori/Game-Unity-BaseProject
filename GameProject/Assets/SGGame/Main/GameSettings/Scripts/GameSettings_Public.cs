using UnityEngine;

namespace SGGames.Game
{
    /// <summary>
    /// ゲーム設定に関する外部アクセス用処理まとめ
    /// 
    /// このファイルに無い方法以外でのアクセスを禁止します
    /// </summary>
    public sealed partial class GameSettings : MonoBehaviour
    {
        /// <summary>
        /// ゲーム起動時のUnix時間
        /// </summary>
        public static long startUnixTime { get; private set; }

        /// <summary>
        /// 現在の値を保存
        /// </summary>
        public static void Save()
        {
            PlayerPrefs.SetInt("Language", (int)GameSettings.language );
/*
            Instance.mGraphics.Save();
            Instance.mPlay.Save();
            Instance.mNotification.Save();
            Instance.mSound.Save();
*/           
        }
        /// <summary>
        /// 設定内容の退避
        /// </summary>
        public static void Push()
        {
/*            
            Instance.mGraphics.Push();
            Instance.mPlay.Push();
            Instance.mSound.Push();
*/            
        }

        /// <summary>
        /// 設定内容の復帰
        /// </summary>
        public static void Pop()
        {
/*
            Instance.mGraphics.Pop();
            Instance.mPlay.Pop();
            Instance.mSound.Pop();
*/
        }

        /// <summary>
        /// 初期状態にリセット
        /// </summary>
        public static void ResetSettings()
        {
/*
            Instance.mGraphics.ResetSettings();
            Instance.mSound.ResetSettings();
            Instance.mPlay.ResetSettings();
*/   
        }

        /// <summary>
        /// サウンド設定のリセット
        /// </summary>
        public static void ResetSoundSettings()
        {
/* 
            Instance.mSound.ResetSettings();
*/
        }


        /// <summary>
        /// 解像度設定内容を適用する
        /// </summary>
        /// <remarks>
        /// 解像度関連の処理は、見た目を考慮して画面暗転中などに処理行う方が良いと判断し
        /// 即時反映されないようにしています。
        /// この処理はその暗転中などに呼び出すための処理となります。
        /// </remarks>
        public static void ApplyResolution()
        {
/*  
            Change3dResolution( GameSettings.resolution3d, true );
            ChangeScreenResolution( GameSettings.screenResolution, GameSettings.lowRefreshRate, true );
*/
        }

        /// <summary>
        /// 強制終了フラグ
        /// trueの時、前回起動が強制終了している可能性アリ
        /// </summary>
        public static bool  forceQuit { get; set; }

        /// <summary>
        /// 言語
        /// </summary>
        public static Language language
        {
            get {
                return Instance.mLanguage;
            }
            set {
                Instance.mLanguage = value;
            }
        }
        /// <summary>
        /// 言語のインデックス値
        /// </summary>
        public static int languageIndex
        {
            get {
                return (int)Instance.mLanguage;
            }
        }
        /// <summary>
        /// 起動時にシステムから拾った言語
        /// </summary>
        public static Language awakeLanguage
        {
            get {
                return Instance.mAwakeLanguage;
            }

        }

/*
        /// <summary>
        /// 現在設定されているゲーム言語からシステム言語定数を得る
        /// </summary>
        public static SGGames.SystemConst.Language    systemLanguage
        {
            get {
                return s_systemLanguageTable[ GameSettings.languageIndex ];
            }
        }
        /// <summary>
        /// systemLanguageのインデックス値を取得
        /// </summary>
        public static int systemLanguageIndex
        {
            get {
                return (int)GameSettings.systemLanguage;
            }
        }
*/

#region Play
/*
        /// <summary>
        /// ジャイロの状態
        /// </summary>
        public static bool gyroState
        {
            get {
                return play.gyroState;
            }
            set {
                play.gyroState = value;
            }
        }

        /// <summary>
        /// 必殺ショット演出の表示頻度取得
        /// </summary>
        /// <returns></returns>
        public static Play.CutSceneDisplayLevel GetCutSceneDisplayLevel( Play.CutSceneDisplayId id )
        {
            return play.GetCutSceneDisplayLevel(id);
        }

        /// <summary>
        /// 必殺ショット演出の表示頻度設定
        /// </summary>
        /// <param name="level">設定する頻度</param>
        public static void SetCutSceneDisplayLevel( Play.CutSceneDisplayId id, Play.CutSceneDisplayLevel level )
        {
            play.SetCutSceneDisplayLevel( id, level );

        }

        /// <summary>
        /// カットシーン再生許可判定
        /// </summary>
        /// <returns>trueの時、再生可能</returns>
        public static bool CanPlayCutScene( Play.CutSceneDisplayId id )
        {
            return play.CanPlayCutScene(id);
        }
        /// <summary>
        /// カットシーン最終再生時間の更新
        /// </summary>
        public static void UpdateCutSceneDisplayTime( Play.CutSceneDisplayId id )
        {
            play.UpdateCutSceneTime( id );
        }

        /// <summary>
        /// ロビー内でのユーザー名表示形式を取得
        /// </summary>
        /// <returns></returns>
        public static Play.LobbyNameDisplayType GetLobbyNameDisplayType()
        {
            return play.lobbyNameDisplayType;
        }

        /// <summary>
        /// ロビー内でのユーザー名表示形式を設定
        /// </summary>
        /// <returns></returns>
        public static void SetLobbyNameDisplayType( Play.LobbyNameDisplayType displayType )
        {
            play.lobbyNameDisplayType = displayType;
        }

        /// <summary>
        /// ロビー内で使用するキャラクタ画像のダウンロード許可判定
        /// </summary>
        /// <returns></returns>
        public static bool CanDownloadAvatarSnap()
        {
            return play.CanDownloadSnapShot();
        }
        /// <summary>
        /// ロビー内で使用するキャラクタ画像のダウンロード最終時間更新
        /// 
        /// 1つの画像をロードするたびに呼ぶのではなく、その場面で必要なロード処理を終えたタイミングで呼び出す様にして下さい
        /// そうしないと1つ目の画像しかロード出来なくなります。
        /// </summary>
        public static void UpdatAnvatarSnapDownloadTime()
        {
            play.UpdateSnapDownloadTime();
        }

        #endregion


        #region Sound
        /// <summary>
        /// BGM音量
        /// </summary>
        public static int   bgmLevel
        {
            get {
                return sound.bgmLevel;
            }
            set {
                sound.bgmLevel = value;
                GameSound.SetBgmMixerVolume( value );
            }
        }
        /// <summary>
        /// SE音量
        /// </summary>
        public static int   seLevel
        {
            get {
                return sound.seLevel;
            }
            set {
                sound.seLevel = value;
                GameSound.SetSeMixerVolume( value );
            }
        }
        /// <summary>
        /// SE(バトル共通）の音量
        /// </summary>
        public static int seBattleCommonLevel
        {
            get {
                return sound.battleCommonLevel;
            }
            set {
                sound.battleCommonLevel = value;
                GameSound.SetSeBattleCommonVolume( value );
            }
        }
        /// <summary>
        /// SE(バトルボス）の音量
        /// </summary>
        public static int seBattleBossLevel
        {
            get {
                return sound.battleBossLevel;
            }
            set {
                sound.battleBossLevel = value;
                GameSound.SetSeBattleBossVolume( value );
            }
        }
        /// <summary>
        /// SE(バトルユニット）の音量
        /// </summary>
        public static int seBattleUnitLevel
        {
            get {
                return sound.battleUnitLevel;
            }
            set {
                sound.battleUnitLevel = value;
                GameSound.SetSeBattleUnitVolume( value );
            }
        }
        /// <summary>
        /// SE(ショット音)
        /// </summary>
        public static int   shotLevel
        {
            get {
                return sound.shotLevel;
            }
            set {
                sound.shotLevel = value;
                GameSound.SetShotMixerVolume( value );
            }
        }

        /// <summary>
        /// ボイス音量
        /// </summary>
        public static int   voiceLevel
        {
            get {
                return sound.voiceLevel;
            }
            set {
                sound.voiceLevel = value;
                GameSound.SetVoiceMixerVolume( value );
            }
        }

        /// <summary>
        /// 合いの手音量
        /// </summary>
        public static int   clapLevel
        {
            get {
                return sound.clapLevel;
            }
            set {
                sound.clapLevel = value;
                GameSound.SetClapMixerVolume( value );
            }
        }
        #endregion


        #region Notification
        #endregion


        #region Graphics
        /// <summary>
        /// 描画設定レベル
        /// VeryLow～VeryHighはテーブルに従った値でしか設定できない
        /// 個別に設定する場合はCustomを利用する
        /// </summary>
        public static Graphics.SettingLevel settingLevel {
            get {
                return graphics.settingLevel;
            }
            set {
                graphics.settingLevel = value;
                graphics.SetSettings( value );
            }
        }
        /// <summary>
        /// 端末解像度
        /// </summary>
        public static Graphics.Resolution    screenResolution
        {
            get {
                return graphics.screenResolution;
            }
            set {
                if ( graphics.IsCutsom() ) {
                    graphics.screenResolution = value;
                }
            }
        }
        /// <summary>
        /// 現在の端末解像度のスケール
        /// </summary>
        public static float screenResolutionScale
        {
            get {
                return graphics.screenResolutionScale;
            }
        }

        /// <summary>
        /// 3D解像度
        /// </summary>
        public static Graphics.Resolution    resolution3d
        {
            get {
                return graphics.resolution3d;
            }
            set {
                if ( graphics.IsCutsom() ) {
                    graphics.resolution3d = value;
                }
            }
        }

        /// <summary>
        /// 30fpsモード
        /// </summary>
        public static bool lowRefreshRate
        {
            get {
                return graphics.lowRefreshRate;
            }
            set {
                if ( graphics.IsCutsom() )
                {
                    graphics.lowRefreshRate = value;
                }
            }
        }
        /// <summary>
        /// エフェクト表示レベル
        /// </summary>
        public static Graphics.EffectLevel effectLevel
        {
            get {
                return graphics.effectLevel;
            }
            set {
                if ( graphics.IsCutsom() ) {
                    graphics.effectLevel = value;
                }
            }
        }
        /// <summary>
        /// ローモデルの仕様フラグ
        /// </summary>
        public static bool useLowModel
        {
            get {
                return graphics.useLowModel;
            }
            set {
                graphics.useLowModel = value;
            }
        }
        /// <summary>
        /// バトル内の画面フィルタ表示フラグ
        /// </summary>
        public static bool useBattleFilter
        {
            get {
                return graphics.useBattleFilter;
            }
            set {
                graphics.useBattleFilter = value;
            }
        }

        /// <summary>
        /// ポストエフェクトの使用
        /// </summary>
        public static bool usePostEffect
        {
            get {
                return graphics.usePostEffect;
            }
            set {
                if ( graphics.IsCutsom() ) {
                    graphics.usePostEffect = value;
                }
            }
        }
#if GAME_DEBUG
        /// <summary>
        /// デバッグ用強制ポストエフェクト使用フラグ
        /// </summary>
        public static bool forceUsePostEffect
        {
            set {
                graphics.usePostEffect = value;
            }
        }
#endif


        /// <summary>
        /// 現在の値に基づいた端末解像度の適用
        /// </summary>
        /// <param name="_newResolution">新しい端末解像度</param>
        /// <param name="_lowRefreshRate">端末の低リフレッシュレート設定</param>
        /// <param name="_force">強制変更フラグ</param>
        public static void ChangeScreenResolution( GameSettings.Graphics.Resolution _newResolution, bool _lowRefreshRate, bool _force=false )
        {
            bool change = false;
            if ( _newResolution!=GameSettings.screenResolution ) {
                change = true;
            }
            if ( _lowRefreshRate!=GameSettings.lowRefreshRate ) {
                change = true;
            }
            if ( _force ) {
                change = true;
            }
#if UNITY_EDITOR
            if ( _newResolution != Graphics.Resolution.High ) {
                SGSys.DebugLog.Warning("GameSettings.ChangeScreenResolution : Editorではスクリーンサイズをスクリプトから変えられない為、Highしか適用されません");
                change = false;
            }
#endif

            if ( !change ) {
                return;
            }

            float scale = GetResolutionScale( _newResolution );

            int width = (int)(SGSys.Gfx.RenderManager.startScreenSize.x * scale);
            int height= (int)(SGSys.Gfx.RenderManager.startScreenSize.y * scale);
            Instance.mGraphics.lowRefreshRate = _lowRefreshRate;
            int fps = GameSettings.lowRefreshRate ? 30 : 60;
            SGSys.Gfx.RenderManager.ChangeResolution( width, height, fps );
            Application.targetFrameRate = fps;
            
            Instance.mGraphics.screenResolution = _newResolution;
            Instance.mGraphics.screenResolutionScale = scale;

            SGSys.App.Game.Main.CameraController.ChangeSystemResolution();
        }
        /// <summary>
        /// 3D画面の解像度変更
        /// </summary>
        /// <param name="newResolution"></param>
        /// <param name="force">強制変更フラグ</param>
        public static void Change3dResolution( GameSettings.Graphics.Resolution newResolution, bool force=false )
        {
            bool change = false;

            if ( newResolution != GameSettings.resolution3d ) {
                change = true;
            }
            if ( force ) {
                change = true;
            }
            if ( !change ) {
                return;
            }

            Instance.mGraphics.resolution3d = newResolution;
            XeApp.Game.Main.CameraController.Change3dResolution();
        }

        /// <summary>
        /// 解像度値ごとの画面表示スケール
        /// </summary>
        static float[] sResolutionScaleTable = new float[]
        {
            0.25f,  //Low
            0.50f,  //Middle
            1.00f,  //High
        };

        /// <summary>
        /// 指定解像度の表示スケールを得る
        /// </summary>
        /// <param name="target">対象の解像度</param>
        /// <returns>設定すべきスケール</returns>
        public static float GetResolutionScale( GameSettings.Graphics.Resolution target )
        {
            var index = (int)target;
            return sResolutionScaleTable[index];
        }

        /// <summary>
        /// UI用の向き設定
        /// </summary>
        public static Graphics.UIOrientation uiOrientation
        {
            get {
                return graphics.uiOrientation;
            }
            set {
                graphics.uiOrientation = value;
            }
        }
*/
#endregion    
    }
}
