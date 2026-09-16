using UnityEngine;
using System.Collections.Generic;

namespace SGSys
{
	/// <summary>
	/// 乱数用ユーティリティ
	/// </summary>
	public static class RandomUtil
    {
		/// <summary>
		/// bool型の乱数発生
		/// </summary>
		/// <returns>true or false</returns>
		public static bool RandomBool()
        {
			return (Random.Range(0,2) == 0);
		}

		/// <summary>
		/// 百分率の乱数を発生させて、指定した値未満か判定する
		/// </summary>
		/// <param name="v">判定する閾値</param>
		/// <returns>v未満の場合true</returns>
		public static bool Range100( int v )
        {
			if ( v >= 100 )
            {
				return true;
			}

			int r = Random.Range( 0, 100 );
			if ( r < v )
            {
				return true;
			}
			return false;
		}

		/// <summary>
		/// 符号の乱数発生
		/// </summary>
		/// <returns>-1.0f or 1.0f</returns>
		public static float RandomSign()
        {
			float a = Random.value;
			a -= 0.5f;
			return Mathf.Sign(a);
		}

		/// <summary>
		/// ランダムなint配列を作成する
		/// </summary>
		/// <param name="start">配列に入れる値の開始値</param>
		/// <param name="count">個数</param>
		/// <returns>作成した配列</returns>
		/// <remarks>
		/// startからstart+countの範囲の値を配列に入れた上でランダムに並べ替えた後の配列を返します
		/// </remarks>
		public static int[] MakeArray( int start, int count )
        {
			int[] array = new int[count];
			for ( int i=0; i<count; ++i )
            {
				array[i] = start+i;
			}
			for ( int i=0; i<count; ++i )
            {
				int a = i;
				int b = Random.Range(i,count);
				Utility.Swap<int>( ref array[a], ref array[b] );
			}
			return array;
		}

		/// <summary>
		/// ランダムなList<int>を作成する
		/// </summary>
		/// <param name="list">出力先リスト</param>
		/// <param name="start">開始値</param>
		/// <param name="size">サイズ</param>
		/// <remarks>
        /// startからstart+size未満までの数値をランダムに並び替えてlistに保存します
		/// </remarks>
		public static void MakeList( List<int> list, int start, int size )
        {
			list.Clear();
			
			for ( int i=0; i<size; ++i )
            {
				list.Add( start+i );
			}

			for ( int i=0; i<size; ++i )
            {
				int a = i;
				int b = Random.Range(i,size);
				int tmp = list[a];
				list[a] = list[b];
				list[b] = tmp;
			}
		}

		/// <summary>
		/// 指定値は先頭に入れないランダムなList<int>を作成する
		/// </summary>
		/// <param name="list">出力先リスト</param>
		/// <param name="start">開始値</param>
		/// <param name="size">サイズ</param>
        /// <param name="exclude">先頭除外値</param>"
		/// <remarks>
        /// 基本動作はMakeListと同じですが、excludeに指定した値はリストの先頭にならないようにします。
        /// これの利用目的は、前回使用したランダムリストの終端の値と、今回使用するランダムリストの始端の値が
        /// 同じ場合、本来ランダムであってもランダムでない感じがする為、これを回避するのに利用します。
		/// </remarks>
        public static void MakeListExclude( List<int> list, int start, int size, int exclude )
        {
            MakeList( list, start, size );

            if ( list.Count > 0 && list[0] == exclude )
            {
                if ( list.Count == 1 )
                {
                    throw new System.ArgumentException("The only list value cannot be excluded.", nameof(exclude));
                }
                //先頭が除外値の場合、先頭以外の場所と入れ換える
                int index = Random.Range( 1, list.Count );
                list[0] = list[index];
                list[index] = exclude;
            }
        }

		/// <summary>
		/// リストの内容をランダムに並べ替える
		/// </summary>
		/// <param name="list">対象のリスト</param>
		/// <param name="start"></param>
		/// <param name="count"></param>
		public static void SwapList( List<int> list )
        {
			for ( int i=0; i<list.Count; ++i )
            {
				int a = i;
				int b = Random.Range(i,list.Count);
				int tmp = list[a];
				list[a] = list[b];
				list[b] = tmp;
			}
		}
	}
}
