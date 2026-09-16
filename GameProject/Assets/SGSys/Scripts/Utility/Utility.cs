//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     Utility.cs
 *    @brief    各種ユーティリティ 
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace SGSys
{
    public class Utility
    {
        //==========================================================================
        /**
         *    @brief       X.Y.Z形式のバージョン文字列を数値化したものを得る.
         *    @param[in]   versionString  X.Y.Z形式のバージョン文字列.
         *    @return      整数化した値.
         */
        //==========================================================================
        public static int GetVersionValue( string versionString )
        {
            if (versionString == null) {
                throw new System.ArgumentNullException(nameof(versionString));
            }
            string[] splitedString = versionString.Split( '.' );
            int count = splitedString.Length;
            if (count != 3) {
                throw new System.ArgumentException("Expected X.Y.Z version format.", nameof(versionString));
            }
            int[] splitedValue = new int[count];
            
            for ( int i=0; i<count; i++ ) {
                splitedValue[i] = int.Parse( splitedString[i] );
                if (splitedValue[i] < 0 || splitedValue[i] > 999) {
                    throw new System.ArgumentOutOfRangeException(nameof(versionString), "Each version component must be between 0 and 999.");
                }
            }
            int version = 0;
            int scale = 1;
            for ( int i=count-1; i>=0; i-- ) {
                version += splitedValue[i] * scale;
                scale *= 1000;
            }
            return version;
        }
        
        //==========================================================================
        /**
         *    @brief       文字列の取得 
         *    @param[in]   bytes  読み込むバッファ
         *    @param[in]   length 取得するサイズ
         *    @param[out]  offset 読み込むバッファのオフセット
         *    @return      取得した文字列 
         */
        //==========================================================================
        public static string GetString( byte[] bytes, int length, ref int offset )
        {
            string str = System.Text.Encoding.Unicode.GetString( bytes, offset, length );
            offset += length;
            return str;
        }

        //==========================================================================
        /**
         *    @brief       文字列の取得 
         * 
         *    bytesの先頭2バイトに文字列長が入っている必要があります
         *
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @return      取得した文字列 
         */
        //==========================================================================
        public static string GetString( byte[] bytes, ref int offset )
        {
            short length = GetValueInt16( bytes, ref offset );
            string str = System.Text.Encoding.Unicode.GetString( bytes, offset, length );
            offset += length;
            return str;
        }
        
        //==========================================================================
        /**
         *    @brief       ASCII文字列の取得 
         * 
         *    ヌル文字がくるまで解析し文字列として返します
         *
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @param[in]   length bytes[offset]から取得する最大文字列長
         *    @return      取得した文字列 （内部でヌル文字の位置を調べている為
         *                                 取得文字列長はlengthと等価でない場合があります）
         */
        //==========================================================================
        public static string GetAsciiString( byte[] bytes, ref int offset, int length )
        {
            //ヌル文字検索
            int strLength = 0;
            for ( int i=0; i<length; i++ )
            {
                byte a = bytes[offset+i];
                if ( '\0'==a ) {
                    break;
                }
                ++strLength;
            }

            string str = System.Text.Encoding.ASCII.GetString( bytes, offset, strLength );
            offset += length;
            return str;
        }
        
        //==========================================================================
        /**
         *    @brief       byte型の値を取得しオフセットを進める.
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @return      取得した値 
         */
        //==========================================================================
        public static byte GetValueByte( byte[] bytes, ref int offset )
        {
            byte param = bytes[offset];
            offset += 1;
            return param;
        }

        //==========================================================================
        /**
         *    @brief       int16型の値を取得しオフセットを進める 
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @return      取得した値 
         */
        //==========================================================================
        public static short GetValueInt16( byte[] bytes, ref int offset )
        {
            short param = System.BitConverter.ToInt16( bytes, offset );
            offset += 2;
            return param;
        }

        //==========================================================================
        /**
         *    @brief       uint16型の値を取得しオフセットを進める 
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット
         *    @return      取得した値 
         */
        //==========================================================================
        public static ushort GetValueUInt16( byte[] bytes, ref int offset ) {
            ushort param = System.BitConverter.ToUInt16( bytes, offset );
            offset += 2;
            return param;
        }

        //==========================================================================
        /**
         *    @brief       int32型の値を取得しオフセットを進める 
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @return      取得した値 
         */
        //==========================================================================
        public static int GetValueInt32( byte[] bytes, ref int offset )
        {
            int param = System.BitConverter.ToInt32( bytes, offset );
            offset += 4;
            return param;
        }

        //==========================================================================
        /**
         *    @brief       uint32型の値を取得しオフセットを進める 
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @return      取得した値 
         */
        //==========================================================================
        public static uint GetValueUInt32( byte[] bytes, ref int offset )
        {
            uint param = System.BitConverter.ToUInt32( bytes, offset );
            offset += 4;
            return param;
        }

        //==========================================================================
        /**
         *    @brief       float型の値を取得しオフセットを進める 
         *    @param[in]   bytes  読み込むバッファ 
         *    @param[out]  offset 読み込むバッファのオフセット 
         *    @return      取得した値 
         */
        //==========================================================================
        public static float GetValueFloat( byte[] bytes, ref int offset )
        {
            float param = System.BitConverter.ToSingle( bytes, offset );
            offset += 4;
            return param;
        }

        //==========================================================================
        /**
         *    @brief       UNIX時間からローカル時間を得る 
         *    @param[in]   unixTime  得たいUNIX時間 
         *    @return      ローカル時間情報 
         */
        //==========================================================================
        private static System.DateTime UNIX_EPOCH = new System.DateTime(1970,1,1,0,0,0,System.DateTimeKind.Utc);
        public static System.DateTime GetLocalDateTime( long unixTime )
        {
            System.DateTime dt = UNIX_EPOCH.AddSeconds( (double)unixTime );
            System.DateTime localTime = dt.ToLocalTime();
            return localTime;
        }
        //==========================================================================
        /**
         *    @brief       現在の時刻からUNIX時間を得る.
         * 
         *    @param[in]   year    西暦(1970～)
         *    @param[in]   month   月(1～)
         *    @param[in]   day     日(1～)
         *    @param[in]   hour    時
         *    @param[in]   minute  分
         *    @param[in]   second  秒
         *    @return      UNIX時間
         */
        //==========================================================================
        public static long GetTargetUnixTime( int year, int month, int day, int hour, int minute, int second )
        {
            System.DateTime targetDt = new System.DateTime(year, month, day, hour, minute, second, System.DateTimeKind.Local);
            System.TimeSpan span = targetDt.ToUniversalTime() - UNIX_EPOCH;
            return (long)span.TotalSeconds;
        }

        //==========================================================================
        /**
         *    @brief       現在の時刻からUNIX時間を得る.
         */
        //==========================================================================
        public static long GetCurrentUnixTime()
        {
            System.DateTime now = System.DateTime.UtcNow;
            System.TimeSpan elapsedTime = now - UNIX_EPOCH;
            long time = (long)elapsedTime.TotalSeconds;
            return time;
        }
            
        //==========================================================================
        /**
         *    @brief       指定した期間内か判定する.
         *    @param[in]   current  調査対象時間.
         *    @param[in]   start    開始時間.
         *    @param[in]   end      終了時間.
         *    @retval      true     期間内.
         *    @retval      false    期間外.
         *
         *    start <= current <= endである場合trueを返します.
         *    開始未定義の場合は、current <= endかを判定し.
         *    終了未定義の場合は、start <= currentかを判定します.
         */
        //==========================================================================
        public static bool IsWithinPeriod( long current, long start, long end )
        {
            if ( 0==start && 0==end )
            {
                //両方未定義は必ず範囲内.
                return true;
            }
            
            if ( 0==start )
            {
                //開始期間が未定義.
                if ( current <= end )
                {
                    return true;
                }
            }
            else if( 0==end )
            {
                //終了期間が未定義.
                if ( start <= current )
                {
                    return true;
                }
            }
            else
            {
                //両方定義.
                if ( (start <= current) && (current <= end) )
                {
                     return true;
                }
            }

            return false;
        }
        
        /// <summary>
        /// UNIX時間が示す時間を hh:mm 形式で取得する
        /// </summary>
        /// <param name="unix_time">取得するUNIX時間</param>
        /// <returns>hh:mm 形式の文字列</returns>
        public static string GetHMTimeStringFromUNIXTime( long unix_time )
        {
            System.DateTime date = Utility.GetLocalDateTime(unix_time);
            return System.String.Format("{0:D2}:{1:D2}", date.Hour, date.Minute );
        }

        /// <summary>
        /// UNIX時間が示す時間を hh:mm:ss 形式で取得する
        /// </summary>
        /// <param name="unix_time">取得するUNIX時間</param>
        /// <returns>hh:mm:ss 形式の文字列</returns>
        public static string GetHMSTimeStringFromUNIXTime( long unix_time )
        {
            System.DateTime date = Utility.GetLocalDateTime(unix_time);
            return System.String.Format("{0:D2}:{1:D2}:{2:D2}", date.Hour, date.Minute, date.Second );
        }

        /// <summary>
        /// UNIX時間から日付と時間を yyyy/mm/dd hh:mm:ss 形式で取得する
        /// </summary>
        /// <param name="unix_time">取得するUNIX時間</param>
        /// <returns>yyyy/mm/dd hh:mm:ss 形式の文字列</returns>
        public static string GetDayTimeStringFromUNIXTime( long unixTime )
        {
            System.DateTime date = Utility.GetLocalDateTime(unixTime);
            return string.Format("{0:D4}/{1:D2}/{2:D2} {3:D2}:{4:D2}:{5:D2}", date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second );
        }

        /// <summary>
        /// UNIX時間を年月日（yyyy/mm/dd）形式で取得する
        /// </summary>
        /// <param name="unix_time">取得するUNIX時間</param>
        /// <returns>yyyy/mm/dd 形式の文字列</returns>
        public static string GetDayStringFromUNIXTime( long unix_time )
        {
            System.DateTime date = Utility.GetLocalDateTime(unix_time);
            return string.Format("{0:D4}/{1:D2}/{2:D2}", date.Year, date.Month, date.Day );
        }
        
        //==========================================================================
        /**
         *    @brief       2つのUNIX時間がHOUR部分で跨いだか判定する.
         *    @param[in]   prevUnixTime  前回のUNIX時間.
         *    @param[in]   nextUnixTime  次回のUNIX時間.
         *    @retval      true          跨いだ.
         *    @retval      false         跨いでない.
         * 
         *    prevUnixTime 12:30:45   nextUnixTime 13:00:00  => 跨いだ.
         *    prevUnixTime 12:30:45   nextUnixTime 12:59:59  => 跨いでない.
         * 
         *    2つ時刻情報で毎時1度何かしたい時等に利用します.
         */
        //==========================================================================
        public static bool IsAcrossHour( long prevUnixTime, long nextUnixTime )
        {
            System.DateTime prevTime = GetLocalDateTime(prevUnixTime);
            System.DateTime nextTime = GetLocalDateTime(nextUnixTime);

            if ( prevTime.Hour != nextTime.Hour )
            {
                //時間値が異なるなら確実に跨いでいる.
                return true;
            }

            long diff = nextUnixTime - prevUnixTime;
            if ( 60*60 <= System.Math.Abs(diff) ) {
                //前回の値より1時間以上経過しているのも跨いでいると言える.
                //24時間後にIsAcrossHourを呼び出した時の対策.
                return true;
            }
            
            return false;
        }
        
        //==========================================================================
        /**
         *    @brief       2つのUNIX時間が日付を跨いだか判定する.
         *    @param[in]   prevUnixTime  前回のUNIX時間.
         *    @param[in]   nextUnixTime  次回のUNIX時間.
         *    @retval      true          跨いだ.
         *    @retval      false         跨いでない.
         * 
         *    prevUnixTime 2012/4/25  nextUnixTime 2012/4/26 => 跨いだ.
         *    prevUnixTime 2012/4/25  nextUnixTime 2013/4/25 => 跨いだ.
         *                                                   <- 次の年の同日
         *    prevUnixTime 2012/4/25  nextUnixTime 2012/4/25 => 跨いでない.
         *
         *    2つ時刻情報で毎日1度何かしたい時等に利用します.
         */
        //==========================================================================
        public static bool IsAcrossDay( long prevUnixTime, long nextUnixTime )
        {
            System.DateTime prevTime = GetLocalDateTime(prevUnixTime);
            System.DateTime nextTime = GetLocalDateTime(nextUnixTime);

            if ( prevTime.DayOfYear != nextTime.DayOfYear )
            {
                //1年内での経過日数が異なるなら確実に跨いでいる.
                return true;
            }

            long diff = nextUnixTime - prevUnixTime;
            if ( 60*60*24 <= System.Math.Abs(diff) && prevTime.Date != nextTime.Date )
            {
                //前回の値より24時間以上経過しているのも跨いでいると言える.
                //1年後にIsAcrossDayを呼び出した時の対策.
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// バイト配列を指定したストレージパスにファイル出力
        /// </summary>
        /// <param name="path">出力パス</param>
        /// <param name="bytes">出力するデータ</param>
        /// <param name="length">出力するサイズ</param>
        /// <param name="overwrite">pathが存在する場合に上書きするか</param>
        public static void SaveToStorage( string path, byte[] bytes, int length, bool overwrite )
        {
#if GAME_DEBUG
            DebugLog.Info("Utility.SaveToStorage : ["+path+"]" );
#endif
        
            path = RemoveFileProtocol(path);
        
            //フォルダが無い場合作成.
            string dirName = System.IO.Path.GetDirectoryName( path );
            if ( !string.IsNullOrEmpty(dirName) && !System.IO.Directory.Exists( dirName ) )
            {
#if GAME_DEBUG
                DebugLog.Info("Utility.SaveToStorage CreateDirectory : " +dirName );
#endif
                System.IO.Directory.CreateDirectory( dirName ) ;
            }

            if ( !overwrite )
            {
                if ( System.IO.File.Exists(path) )
                {
                    return;
                }
            }

            //ファイルの出力.
            if (bytes == null) {
                throw new System.ArgumentNullException(nameof(bytes));
            }
            if (length < 0 || length > bytes.Length) {
                throw new System.ArgumentOutOfRangeException(nameof(length));
            }
            using System.IO.FileStream fs = new System.IO.FileStream(
                path,
                System.IO.FileMode.Create,
                System.IO.FileAccess.Write );
            fs.Write( bytes, 0, length );
            fs.Close();
        }

        /// <summary>
        /// バイト配列を指定したストレージパスにファイル出力
        /// </summary>
        /// <param name="path">出力パス</param>
        /// <param name="bytes">出力するデータ</param>
        /// <param name="overwrite">pathが存在する場合に上書きするか</param>
        public static void SaveToStorage( string path, byte[] bytes, bool overwrite )
        {
            SaveToStorage( path, bytes, bytes.Length, overwrite );
        }

        /// <summary>
        /// ストレージ内の既存のファイルに対して更新を行う
        /// </summary>
        /// <remarks>
        /// 指定したファイルが存在しない場合はSystem.IO.FileNotFoundException例外になりますので
        /// あらかじめファイル存在を確認の上呼び出します。
        /// コールバックでは渡されたFileStreamに対してSeekやWriteを使用して書き込みを行います。
        /// サイズの大きなファイルの一部を更新したい場合、SaveToStorageより高速です。
        /// </remarks>
        /// <param name="path">ファイルパス</param>
        /// <param name="callback">ファイル操作コールバック</param>
        public static void UpdateToStorage( string path, System.Action<System.IO.FileStream> callback )
        {
#if GAME_DEBUG
            DebugLog.Info("Utility.UpdateToStorage Begin : [" + path + "]");
#endif
            path = RemoveFileProtocol( path );

            using System.IO.FileStream fs = new System.IO.FileStream(
                path,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Write);
            callback(fs);
            fs.Close();
        }

        /// <summary>
        /// file:// のプロトコルを除外する
        /// </summary>
        /// <param name="path">除外前のパス</param>
        /// <returns>除外後のパス</returns>
        public static string RemoveFileProtocol( string path )
        {
            if ( path.StartsWith("file://", System.StringComparison.OrdinalIgnoreCase) )
            {
                return new System.Uri(path).LocalPath;
            }

            return path;
        }

        /// <summary>
        /// ストレージ内のファイルを読みだす
        /// </summary>
        /// <remarks>
        /// 指定したファイルが存在しない場合はSystem.IO.FileNotFoundException例外になりますので
        /// あらかじめファイル存在を確認の上呼び出します。
        /// </remarks>
        /// <param name="path">ファイルパス</param>
        /// <returns>読みだしたバイトデータ</returns>
        public static byte[] LoadFromStorage(string path)
        {
#if GAME_DEBUG
            DebugLog.Info("Utility.LoadFromStorage Begin : [" + path + "]");
#endif
            path = RemoveFileProtocol(path);

            using System.IO.FileStream fs = new System.IO.FileStream(
                path,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read);
            byte[] bytes = new byte[fs.Length];
            // Read は指定したバイト数を必ず一度で読み切るとは限らないため、戻り値を無視すると警告になる
            int offset = 0;
            while (offset < bytes.Length)
            {
                int read = fs.Read(bytes, offset, bytes.Length - offset);
                if (read == 0)
                {
                    throw new System.IO.EndOfStreamException();
                }
                offset += read;
            }
            fs.Close();
            return bytes;
        }

        /// <summary>
        /// ストレージ内のファイルを削除
        /// </summary>
        /// <param name="path"></param>
        public static void DeleteFile( string path )
        {
#if GAME_DEBUG
            DebugLog.Info("Utility.DeleteFromStorage Begin : [" + path + "]");
#endif
            path = RemoveFileProtocol(path);
            System.IO.File.Delete(path);
        }

        [System.Obsolete("Use DeleteFile()")]
        public static void DeleteFromStorage(string path)
        {
            DeleteFile( path );
        }

        /// <summary>
        /// ストレージ内の指定したディレクトリを削除
        /// 
        /// pathで指定するディレクトリを削除する際、その中にファイルや子ディレクトリが存在する場合
        /// recursive=trueにしていないと、削除する事が出来ないので注意が必要です。
        /// </summary>
        /// <param name="path">削除するディレクトリパス</param>
        /// <param name="recursive">path以下を再帰的に削除するか</param>
        public static void DeleteDirectory( string path, bool recursive=true )
        {
            path = RemoveFileProtocol(path);
            if ( !System.IO.Directory.Exists(path) )
            {
                return;
            }
#if GAME_DEBUG
            DebugLog.Info("Utility.DeleteFile : " + path);
#endif
            System.IO.Directory.Delete( path, recursive );
        }

        /// <summary>
        /// 指定したファイルパスの有無を判定する
        /// </summary>
        /// <param name="path">判定対象のパス</param>
        /// <returns>trueの時、存在する</returns>
        public static bool ExistsFile( string path )
        {
            path = RemoveFileProtocol(path);
            return System.IO.File.Exists( path );
        }

        /// <summary>
        /// 指定したディレクトリパスの有無を判定する
        /// </summary>
        /// <param name="path">判定対象のパス</param>
        /// <returns>trueの時、存在する</returns>
        public static bool ExistsDirectory( string path )
        {
            path = RemoveFileProtocol(path);
            return System.IO.Directory.Exists(path);
        }

        /// <summary>
        /// 指定したファイルパスのディレクトを作成する
        /// </summary>
        /// <param name="path"></param>
        public static void CreateFileDirectory( string path )
        {
            var dir = RemoveFileProtocol(path);
            dir = System.IO.Path.GetDirectoryName(dir);
            if ( !string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir) ) {
                System.IO.Directory.CreateDirectory( dir );
            }
        }
        
        //==========================================================================
        /**
         *    @brief       値のスワップ処理
         */
        //==========================================================================
        public static void Swap<T>( ref T lhs, ref T rhs )
        {
            T temp;
            temp = lhs;
            lhs = rhs;
            rhs = temp;
        }
        
        //==========================================================================
        /**
         *    @brief       列挙の数を返す
         */
        //==========================================================================
        public static int EnumCount<T>()
        {
            int count = System.Enum.GetNames(typeof(T)).Length;
            return count;
        }
        
        
        //==========================================================================
        /**
         *    @brief       見つからなかった場合、警告を発するGameObject.Find処理
         *
         *    @param[in]   name   探すGameObject
         *    @return      対象のGameObject
         */
        //==========================================================================
        public static GameObject FindGameObject( string name )
        {
            GameObject go = GameObject.Find( name );
            if ( null == go )
            {
                DebugLog.Error("Utility.FindGameObject : ["+name+"] is not exist!");
            }

            return go;
        }
        
        //==========================================================================
        /**
         *    @brief       指定したゲームオブジェクトにアタッチされているComponentを取得する
         *
         *    @param[in]   gameObjectName スクリプト取得対象GameObject
         *    @return      対象のComponent
         */
        //==========================================================================
        public static T GetGameObjectComponent<T>( string gameObjectName ) where T : Component
        {
            GameObject go = GameObject.Find(gameObjectName) as GameObject;
            T component = null;
            if ( null != go ) {
                component = go.GetComponent<T>();
                if ( null == component )
                {
                    DebugLog.Error( SystemConst.DebugGroup.System, "Utility.GetGameObjectComponent : GameObject[" + go.GetType().FullName + "] Component["+typeof(T)+"] :  is not exist");
                }
            }

            return component;
        }

        //==========================================================================
        /**
         *    @brief       nullチェックのあるGetComponent
         *
         *    @param[in]   go       Componentを取得するGameObject
         *    @return      取得したComponent
         */
        //==========================================================================
        public static T GetSafeComponent<T>( GameObject go ) where T : Component
        {
            T component = go.GetComponent<T>();
            if ( null == component )
            {
                DebugLog.Error( SystemConst.DebugGroup.System, "Utility.GetSafeComponent : GameObject["+go.name+"] Component["+typeof(T)+"] : not exist");
            }

            return component;
        }
        
        //==========================================================================
        /**
         *    @brief       GameObjectに対するactive設定
         *
         *    Unity3.XとUnity4.Xの互換性を持たせる為の処理
         *    Unity3.XではGameObject.activeを実施した場合、子のactive状態に関わらず
         *    親の情報を上書きするが、Unity4.Xでは元状態への変更が出来るようになったため
         *    後のアップデートを考慮してこの処理を利用します。
         */
        //==========================================================================
        public static void SetActiveGameObject( GameObject go, bool active )
        {
#if UNITY_3_5
            go.active = active;
#else
            go.SetActive( active );
//			DebugLog.Error( SystemConst.DebugGroup.System, "Utility.SetActiveGameObject : Unimplement!!");
#endif
        }
        //==========================================================================
        /**
         *    @brief       再帰的にGameObjectのactive設定
         */
        //==========================================================================
        public static void SetActiveGameObjectRecursively( GameObject go, bool flag )
        {
#if UNITY_3_5
            go.SetActiveRecursively( flag );
#else
            go.SetActive( flag );
            foreach (Transform child in go.transform) {
                SetActiveGameObjectRecursively(child.gameObject, flag);
            }
//			DebugLog.Error( SystemConst.DebugGroup.System, "Utility.SetActiveGameObject : Unimplement!!");
#endif
        }

        //==========================================================================
        /**
         *    @brief       GameObjectのアクティブ判定処理
         *
         *    Unity3.X のGameObject.active、Unity4.XのactiveSelfに相当します
         *
         *    @retval      true    アクティブ
         *    @retval      false   非アクティブ
         */
        //==========================================================================
        public static bool IsActiveGameObject( GameObject go )
        {
#if UNITY_3_5
            return go.active;
#else
            return go.activeSelf;
//			DebugLog.Error( SystemConst.DebugGroup.System, "Utility.IsActiveGameObject : Unimplement!!");
#endif
        }

        /// <summary>
        /// レイヤーを再帰的に設定
        /// </summary>
        /// <param name="go">対象のGameObject</param>
        /// <param name="layer">設定レイヤー値</param>
        public static void SetLayerRecursively( GameObject go, int layer )
        {
            go.layer = layer;
            foreach( Transform tform in go.transform )
            {
                SetLayerRecursively( tform.gameObject, layer );
            }
        }
        
        //==========================================================================
        /**
         *    @brief       子オブジェクトを探す
         *
         *    @param[in]   name       探すオブジェクト名
         *    @param[in]   rootTrans  親オブジェクトのTransform
         *    @return      見つけたオブジェクト
         */
        //==========================================================================
        public static GameObject SearchGameObjectRecursively( string name, Transform rootTrans )
        {
            if ( null == rootTrans )
            {
                return null;
            }

            foreach ( Transform tr in rootTrans )
            {
                if ( tr.gameObject.name == name )
                {
                    return tr.gameObject;
                }
                else
                {
                    GameObject go = SearchGameObjectRecursively( name, tr );
                    if ( null != go )
                    {
                        return go;
                    }
                }
            }

            return null;
        }		

        /// <summary>
        /// 指定した名前を含むTransformを探す
        /// </summary>
        /// <param name="name"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Transform SearchContainsTransformRecursively( string name, Transform root )
        {
            if (root == null) {
                return null;
            }
            for ( int i=0; i<root.childCount; ++i )
            {
                Transform tf = root.GetChild(i);
                if ( tf.name.Contains(name) )
                {
                    return tf;
                }
                else
                {
                    tf = SearchContainsTransformRecursively( name, tf );
                    if ( null != tf )
                    {
                        return tf;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 指定した名前と一致するTransformを探す
        /// </summary>
        /// <param name="name"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Transform SearchTransformRecursively( string name, Transform root )
        {
            if (root == null) {
                return null;
            }
            for ( int i=0; i<root.childCount; ++i )
            {
                Transform tf = root.GetChild(i);
                if ( tf.name == name )
                {
                    return tf;
                }
                else
                {
                    tf = SearchTransformRecursively( name, tf );
                    if ( null != tf )
                    {
                        return tf;
                    }
                }
            }

            return null;
        }
        /// <summary>
        /// 指定した名前と一致するTransformを探す
        /// </summary>
        /// <remarks>
        /// 大文字小文字を無視します
        /// </remarks>
        /// <param name="name"></param>
        /// <param name="root"></param>
        /// <returns></returns>
        public static Transform SearchTransformRecursivelyIgnoreCase( string name, Transform root )
        {
            if (root == null) {
                return null;
            }
            name = name.ToLowerInvariant();

            for ( int i=0; i<root.childCount; ++i )
            {
                Transform tf = root.GetChild(i);
                var tfName = tf.name.ToLowerInvariant();
                if ( tfName == name )
                {
                    return tf;
                }
                else
                {
                    tf = SearchTransformRecursivelyIgnoreCase( name, tf );
                    if ( null != tf )
                    {
                        return tf;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 指定した名前を含むTransformを全て探してリストにして返します
        /// </summary>
        /// <param name="name">Transformの名前に含まれる文字列</param>
        /// <param name="root">検索対象の基準Transform</param>
        /// <returns>見つかったTransformのリスト。なかった場合Count=0になります。</returns>
        public static List<Transform> SearchContainsAllTransform( string name, Transform root )
        {
            List<Transform> list = new List<Transform>();
            if (root == null) {
                return list;
            }
            var tforms = root.GetComponentsInChildren<Transform>(true);
            foreach( var tf in tforms )
            {
                if ( tf.name.Contains(name) )
                {
                    list.Add( tf );
                }
            }

            return list;
        }

        /// <summary>
        /// 指定したゲームオブジェクトを探し、その中に含まれているコンポーネントを取得する
        /// </summary>
        /// <remarks>
        /// GameObject.Find等と異なりTransformから探す為、activeSelf=falseなGameObjectのコンポーネントも探す事が可能です
        /// </remarks>
        /// <typeparam name="T">UnityEngine.Component</typeparam>
        /// <param name="gameObjectName">探すGameObject名</param>
        /// <param name="rootTform">検索の基点Transform</param>
        /// <returns>gameObjectNameのGameObjectが持つコンポーネントT</returns>
        public static T SearchComponentRecursively<T>( string gameObjectName, Transform rootTform ) where T : Component {
            GameObject gobj = SearchGameObjectRecursively( gameObjectName, rootTform );
            if ( null == gobj )
            {
                return null;
            }
            T t = gobj.GetComponent<T>();
            return t;
        }
        
        //==========================================================================
        /**
         *    @brief       SDBMハッシュ値を求める
         *
         *    @param[in]   bytes      ハッシュ値を求めるバイト配列
         */
        //==========================================================================
        public static int CalculateSdbmHash( byte[] bytes )
        {
            return CalculateSdbmHash( bytes, 65599 );
        }

        //==========================================================================
        /**
         *    @brief       SDBMハッシュ値を求める
         *
         *    @param[in]   bytes      ハッシュ値を求めるバイト配列
         *    @param[in]   initValue  初期値
         */
        //==========================================================================
        public static int CalculateSdbmHash( byte[] bytes, int initValue )
        {
            int result = initValue;
            foreach( byte b in bytes )
            {
                result = b + (result<<6) + (result<<16) - result;
            }
            
            return result;
        }

        /// <summary>
        /// 指定した値が範囲内にあるか判定
        /// </summary>
        /// <param name="val"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static bool IsRanged( float val, float min, float max )
        {
            if ( !(min <= val) )
            {
                return false;
            }

            if ( !(val <= max) )
            {
                return false;
            }

            return true;
        }
        public static bool IsRanged( int val, int min, int max )
        {
            if ( val < min )
            {
                return false;
            }

            if ( val > max )
            {
                return false;
            }

            return true;
        }
        
        //==========================================================================
        /**
         *    @brief       数字を各桁に分割
         *                 入力した数字を上の桁から順の配列に分割します。
         *                 例）12345→{1,2,3,4,5}の配列
         *
         *    @param[in]   number     数字
         *    @param[in]   inputList  入力先リストの指定（なければnewする）
         *    @return      分割した数字のリスト
         */
        //==========================================================================
        public static List<int> SepalateDigit(int number)
        {
            return SepalateDigit(number, new List<int>());
        }

        public static List<int> SepalateDigit(int number, List<int> inputList)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder(32);
            System.Text.StringBuilder strDigit = new System.Text.StringBuilder(2);
            str.Append(System.Math.Abs((long)number).ToString(System.Globalization.CultureInfo.InvariantCulture));
            inputList.Clear();
            for (int i = 0; i < str.Length; ++i)
            {
                strDigit.Length = 0;
                strDigit.Append(str[i]);
                inputList.Add(int.Parse(strDigit.ToString()));
            }

            return inputList;
        }

        /// <summary>
        /// SepalateDigitの格納順を逆にしたもの
        /// 下の桁から順に格納する
        /// </summary>
        /// <param name="number"></param>
        /// <param name="inputList"></param>
        /// <returns></returns>
        public static List<int> SepalateDigitRev(int number, List<int> inputList)
        {
            System.Text.StringBuilder str = new System.Text.StringBuilder(32);
            System.Text.StringBuilder strDigit = new System.Text.StringBuilder(2);
            str.Append(System.Math.Abs((long)number).ToString(System.Globalization.CultureInfo.InvariantCulture));
            inputList.Clear();
            for (int i = str.Length-1; i >=0 ; --i)
            {
                strDigit.Length = 0;
                strDigit.Append(str[i]);
                inputList.Add(int.Parse(strDigit.ToString()));
            }

            return inputList;
        }

        /// 距離の２乗を返す（Vector3.Distanceの２乗バージョン）
        public static float SqrDistance(Vector3 a, Vector3 b)
        {
            Vector3 sub = a - b;
            return sub.sqrMagnitude;
        }

        /// <summary>
        /// テキストの行数を取得
        /// </summary>
        /// <param name="text"></param>
        /// <returns>行数</returns>
        public static int GetTextLineCount(string text)
        {
            text = text.Replace("\r\n", "\n").Replace('\r', '\n');
            // 改行コードの数+1
            int count = text.Length - text.Replace("\n", "").Length + 1;
            return count;
        }

        static System.Text.RegularExpressions.Regex s_regex;
        /// <summary>
        /// テキストからリッチテキストのタグを削除
        /// </summary>
        /// <remarks>
        /// UI.Textで使用できるリッチテキスト用のタグを外します。
        /// （<>で括られたテキストを削除します）
        /// </remarks>
        /// <param name="text">元テキスト</param>
        /// <returns>リッチテキストを削除したテキスト</returns>
        public static string RemoveRichTextTag(string text) 
        {
            // １回作ったら置いておく
            if (s_regex == null) {
                string pattern = @"<[^>]*>";
                s_regex = new System.Text.RegularExpressions.Regex(pattern);
            }
            text = s_regex.Replace(text, "");
            return text;
        }

        /// <summary>
        /// EventSystemタッチ中判定
        /// </summary>
        /// <remarks>
        /// uGUIのタッチ判定を行うEventSystemがタッチ中かどうかを判定します。
        /// uGUI操作中に他のタッチ操作をさせたくないときに使用します。
        /// UnityEditor（マウス）時と端末（タッチ）時で扱いが異なるのを吸収します。
        /// </remarks>
        /// <returns>タッチ中ならtrue</returns>
        public static bool IsPointerOverGameObject()
        {
            // EventSystemをenabled = falseするとnullになるらしい
            if (UnityEngine.EventSystems.EventSystem.current == null)
            {
                return false;
            }
            bool ret = false;
#if ENABLE_INPUT_SYSTEM
            if (EventSystem.current.currentInputModule is UnityEngine.InputSystem.UI.InputSystemUIInputModule) {
                var touchscreen = UnityEngine.InputSystem.Touchscreen.current;
                if (touchscreen != null) {
                    foreach (var touch in touchscreen.touches) {
                        if ((touch.press.isPressed || touch.press.wasReleasedThisFrame) && EventSystem.current.IsPointerOverGameObject(touch.touchId.ReadValue())) {
                            return true;
                        }
                    }
                }
                var mouse = UnityEngine.InputSystem.Mouse.current;
                return mouse != null && EventSystem.current.IsPointerOverGameObject(mouse.deviceId);
            }
#endif
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
#if UNITY_EDITOR || UNITY_STANDALONE
            ret = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
#else
            for (int i = 0; i < Input.touchCount; ++i)
            {
                // どうもあてにならないので変更
                //ret = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
                eventDataCurrentPosition.position = Input.GetTouch(i).position;
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
                ret = results.Count > 0;
                if (ret) {
                    break;
                }
            }
#endif
#endif
            return ret;
        }

        /// <summary>
        /// 指定したGameObjectに含まれるコンポーネント全てに同じ処理を実行させる
        /// </summary>
        /// <typeparam name="T">コンポーネント</typeparam>
        /// <param name="gobj">対象のGameObject</param>
        /// <param name="includeInactive">Activeになってないオブジェクトも含むか</param>
        /// <param name="action">含んでいるコンポーネントを引数とする実行させる処理</param>
        public static void ForEachComponent<T>( GameObject gobj, bool includeInactive, System.Action<T> action )
        {
            T[] components = gobj.GetComponentsInChildren<T>(includeInactive);
            for ( int i=0; i<components.Length; ++i )
            {
                action( components[i] );
            }
        }

        /// <summary>
        /// webセーブなbase64文字列を作成
        /// </summary>
        /// <remarks>
        /// 元のテキストをBase64化しかつwebセーフにする為
        /// +（プラス）を -（マイナス）に
        /// /（スラッシュ）を _ (アンダーライン）に
        /// 変換します
        /// </remarks>
        /// <returns></returns>
        public static string MakeWebsafeBase64( string input )
        {
            string base64 = System.Convert.ToBase64String( System.Text.Encoding.UTF8.GetBytes( input ) ) ;
            base64 = base64.Replace( '+', '-' );
            base64 = base64.Replace( '/', '_' );
            return base64;
        }

        /// <summary>
        /// Object名の先頭がstartNameであるオブジェクトを探す
        /// </summary>
        /// <param name="list">探し出すオブジェクトリスト</param>
        /// <param name="startName">探し出す先頭文字列</param>
        /// <returns>見つかったオブジェクト</returns>
        public static Object SearchStartWithObject( Object[] list, string startName )
        {
            for ( int i=0; i<list.Length; ++i ) {
                if ( list[i] != null && list[i].name.StartsWith( startName ) )
                {
                    return list[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Object名にcontainNameが含まれるオブジェクトを探す
        /// </summary>
        /// <param name="list">探し出すオブジェクトリスト</param>
        /// <param name="containsName">探し出すオブジェクト名に含まれる文字列</param>
        /// <returns>見つかったオブジェクト</returns>
        public static Object SearchContainsObject( Object[] list, string containsName )
        {
            for ( int i=0; i<list.Length; ++i )
            {
                if ( list[i] != null && list[i].name.Contains( containsName ) )
                {
                    return list[i];
                }
            }

            return null;
        }

        /// <summary>
        /// GameObjectに含まれるマテリアルの削除
        /// </summary>
        /// <remarks>
        /// マテリアルに紐づくテクスチャがメモリリークするため
        /// </remarks>
        public static void DestroyMaterial(GameObject go)
        {
            Renderer[] renderers = go.GetComponentsInChildren<Renderer>(true);
            for (int rc = 0; rc < renderers.Length; ++rc)
            {
                Renderer rd = renderers[rc];
                Material[] materials = rd.materials;
                if (materials != null)
                {
                    for (int mc = 0; mc < materials.Length; ++mc)
                    {
                        if (Application.isPlaying) {
                            Material.Destroy(materials[mc]);
                        } else {
                            Material.DestroyImmediate(materials[mc]);
                        }
                    }
                }
                // UnityのプロファイラでDestroyImmediateしないとテクスチャ参照が残るのだが、端末でもマテリアル壊れたのでオミット
                // →テクスチャ参照残ると思われる…
#if !UNITY_EDITOR
//				DebugLog.Info(string.Format("==============Utility.DestroyMaterial! ({0})===========", go.name));
#endif
            }
        }

        /// <summary>
        /// 本来UGUIのrichtextフラグを切れば解消するが
        /// 申請時期の都合で、ひとまず文字列を置換する方法で対応するための処理
        /// </summary>
        /// <param name="text"></param>
        public static void ReplaceRichTextTag( ref string text )
        {
            text = text.Replace("<size","");
            text = text.Replace("<b>","");
        }


        static System.Text.StringBuilder mSbWork = new System.Text.StringBuilder(256);
        /// <summary>
        /// 親Transformから子供Transformまでの相対パスを取得する
        /// parent.find()で子供が見つけられるパス
        /// childがparentの下になければ失敗する
        /// 失敗した場合は空文字を返す
        /// </summary>
        /// <param name="parent">基準となる親Transform</param>
        /// <param name="child">目標となる子Transform</param>
        /// <returns>相対パスの文字列 見つからなければ空文字</returns>
        public static string GetRelativeTransformPath(Transform parent, Transform child)
        {
            if (parent == null || child == null || parent == child) {
                return "";
            }
            bool isFind = false;
            mSbWork.Clear();
            Transform part = child;
            if (part != null) {
                //相対パスを取得
                Transform tmp = part;
                while (tmp != null) {
                    mSbWork.Insert(0, tmp.name);
                    tmp = tmp.parent;
                    //ルートを見つけたなら抜ける
                    if (tmp == parent)
                    {
                        isFind = true;
                        break;
                    }
                    else
                    {
                        mSbWork.Insert(0, "/");
                    }
                }
            }

            if (isFind)
            {
                return mSbWork.ToString();
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// トランスフォームの値をコピー
        /// 同名の子供があるならすべてコピーする
        /// </summary>
        /// <param name="src">コピー元</param>
        /// <param name="dest">コピー先</param>
        public static void DeepCopyTransform(Transform src, Transform dest)
        {
            dest.localPosition = src.localPosition;
            dest.localRotation = src.localRotation;
            dest.localScale = src.localScale;

            int len = src.childCount;
            for (int i = 0; i < len; i++)
            {
                Transform srcChild = src.GetChild(i);
                Transform destChild = dest.Find(srcChild.name);
                if (destChild != null)
                {
                    DeepCopyTransform(srcChild, destChild);
                }
            }
        }
    }

}	//namespace SGLib

