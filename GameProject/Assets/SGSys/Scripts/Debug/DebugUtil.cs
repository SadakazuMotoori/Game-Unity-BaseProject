//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     DebugUtil.cs
 *    @brief    デバッグ用ユーティリティ
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
namespace SGSys
{
    public static class DebugUtil
    {
        [System.Diagnostics.Conditional("GAME_DEBUG")]
        public static void Assert( bool condition, string message )
        {
            if ( !condition )
            {
                throw new System.Exception(message);
            }
        }

        /// <summary>
        /// 指定アクションの処理負荷を計測する
        /// </summary>
        /// <param name="count">actionを実行する回数</param>
        /// <param name="action">現在の処理回数を引数とするアクション</param>
        /// <returns>actionをcount回実施した際の処理時間（秒）</returns>
        public static double AnalyzeProcessingLoad( int count, System.Action<int> action )
        {
#if GAME_DEBUG
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            for ( int i=0; i<count; ++i )
            {
                action(i);
            }
            sw.Stop();
            return sw.Elapsed.TotalSeconds;
#else
            return 0.0;
#endif
        }
    }
} //namespace SGSys
