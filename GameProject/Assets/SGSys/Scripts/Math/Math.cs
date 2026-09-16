//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     Math.cs
 *    @brief    各種数学処理
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;

namespace SGSys
{
    public static partial class Math
    {
        //==========================================================================
        /**
         *    @brief       確率乱数処理
         *
         *    @param[in]   rate  目標値（百分率）
         *    @retval      true  rate未満だった
         *    @retval      false rate以上だった
         */
        //==========================================================================
        public static bool Random100( float rate )
        {
            rate = Mathf.Clamp( rate, 0.0f, 100.0f );
            float r = Random.Range( 0.0f, 100.0f );
            if ( 0.0f < rate && ( 100.0f <= rate || r < rate ) )
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 値を範囲内でリピートさせる
        /// </summary>
        /// <param name="val">現在の値</param>
        /// <param name="min">最小値</param>
        /// <param name="max">最大値</param>
        /// <returns>min～max内となる値</returns>
        /// <remarks>
        /// valが範囲外を越えた場合リピートさせるように計算します。
        /// </remarks>
        public static int Repeat( int val, int min, int max )
        {
            if ( min > max ) {
                throw new System.ArgumentOutOfRangeException(nameof(max));
            }
            long range = (long)max - min + 1;
            return (int)(min + (((long)val - min) % range + range) % range);
        }

        public static float Repeat( float val, float min, float max )
        {
            if ( float.IsInfinity(val) ) {
                throw new System.ArgumentOutOfRangeException(nameof(val));
            }
            if ( float.IsInfinity(min) ) {
                throw new System.ArgumentOutOfRangeException(nameof(min));
            }
            if ( float.IsInfinity(max) || min > max ) {
                throw new System.ArgumentOutOfRangeException(nameof(max));
            }
            double range = (double)max - min;
            if ( range == 0.0 ) {
                return min;
            }
            if ( val < min ) {
                double offset = ((double)min - val) % range;
                val = offset == 0.0 ? min : (float)(max - offset);
            }
            if ( val > max ) {
                double offset = ((double)val - max) % range;
                val = offset == 0.0 ? max : (float)(min + offset);
            }
            return val;
        }

        public static Vector2 Repeat( Vector2 val, Vector2 min, Vector2 max )
        {
            for( int i=0; i<2; i++ ) {
                val[i] = Repeat( val[i], min[i], max[i] );
            }
            return val;
        }
        public static Vector3 Repeat( Vector3 val, Vector3 min, Vector3 max )
        {
            for( int i=0; i<3; i++ ) {
                val[i] = Repeat( val[i], min[i], max[i] );
            }
            return val;
        }

        public static Color Repeat( Color val, Color min, Color max )
        {
            for( int i=0; i<4; i++ ) {
                val[i] = Repeat( val[i], min[i], max[i] );
            }
            return val;
        }


        public static Vector3 NoClampLerp( Vector3 start, Vector3 end, float t )
        {
            Vector3 result = start + (end - start) * t;
            return result;
        }

        /// <summary>
        /// 小数部を求める
        /// </summary>
        /// <param name="v">対象の実数</param>
        /// <returns>小数部の値</returns>
        public static float GetDecimalPart( float v )
        {
            return v - Mathf.Floor(v);
        }

        /// <summary>
        /// 指定した値が無限大系か判定
        /// </summary>
        /// <param name="v">対象の値</param>
        /// <returns>trueの時、無限大</returns>
        public static bool IsInfinity( float v )
        {
            if ( Mathf.Infinity == v )
            {
                return true;
            }
            if ( Mathf.NegativeInfinity == v )
            {
                return true;
            }
            return false;
        }
    }
}

