/**
 * @file
 * @brief xorshift によるランダム
 */
using System;

namespace SGSys
{
	/// xorshift によるランダム
	public class XorShift128 : Random
	{
		int W, X, Y, Z;

		/// シード明示版
		public XorShift128(int x, int y, int z, int w)
        {
			if ((x | y | z | w) == 0)
			{
				throw new ArgumentException("At least one seed must be non-zero.");
			}
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		/// クロックチックをシードにする
		public XorShift128() : this((int)DateTime.Now.Ticks)
        {
		}

		public XorShift128(int seed)
        {
			W = 123456789 ^ seed;
			X = 362436069 ^ (seed << 16 + seed >> 16);
			Y = 521288629 ^ (W + X);
			Z = 88675123 ^ (X ^ Y);
		}

		/// 乱数取得 0 ～ 0x7fffffff
		public override int Next()
        {
			int t = X ^ (X << 11);
			X = Y;
			Y = Z;
			Z = W;
			W = (W ^ (int)((uint)W >> 19)) ^ (t ^ (int)((uint)t >> 8));
			int value = W & 0x7fffffff;
			return value == int.MaxValue ? Next() : value;
		}

		/// 範囲内の乱数を取得 [min,max)
		public override int Next(int min, int max)
        {
			if (min > max)
			{
				throw new ArgumentOutOfRangeException(nameof(max));
			}
			if (min == max)
			{
				return min;
			}
			long range = (long)max - min;
			long limit = (long)int.MaxValue * int.MaxValue;
			limit -= limit % range;
			long value;
			do
			{
				value = (long)this.Next() * int.MaxValue + this.Next();
			} while (value >= limit);
			return (int)(value % range + min);
		}

		/// 範囲内の乱数を取得 [0,max)
		public override int Next(int max)
        {
			return Next(0, max);
		}

		protected override double Sample()
        {
			return this.Next() / (double)int.MaxValue;
		}

		public override void NextBytes(byte[] buffer)
        {
			if (buffer == null)
			{
				throw new ArgumentNullException(nameof(buffer));
			}
			for (int i = 0; i < buffer.Length; ++i)
			{
				buffer[i] = (byte)this.Next(256);
			}
		}
	}
}
