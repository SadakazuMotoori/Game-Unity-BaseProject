//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     CurveBezier.cs
 *    @brief    ベジェ曲線
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
		//==========================================================================
		//==========================================================================
		//２次ベジェ曲線
		//==========================================================================
		//==========================================================================
		//==========================================================================
		public class CurveBezier2 : ICurveEvaluator
		{
			public	Vector3		p0			{ get; set; }
			public	Vector3		p1			{ get; set; }
			public	Vector3		p2			{ get; set; }

			//==========================================================================
			/**
			 *    @brief       コンストラクタ
			 */
			//==========================================================================
			public CurveBezier2() : this( Vector3.zero, Vector3.zero, Vector3.zero )
			{
			}

			//==========================================================================
			/**
			 *    @brief       コンストラクタ
			 *
			 *    @param[in]   p0 始点
			 *    @param[in]   p1 制御点
			 *    @param[in]   p2 終点
			 */
			//==========================================================================
			public CurveBezier2( Vector3 p0, Vector3 p1, Vector3 p2 )
			{
				this.p0 = p0;
				this.p1 = p1;
				this.p2 = p2;
			}

			//==========================================================================
			/**
			 *    @brief       座標計算
			 *
			 *    @param[in]   t        媒介変数(0.0～1.0)
			 *    @return      tの時の座標
			 */
			//==========================================================================
			public Vector3 Evaluate( float t )
			{
				return Evaluate(t, this.p0, this.p1, this.p2);
			}

			/// <summary>
			/// 係数計算
			/// </summary>
			/// <param name="t">媒介変数(0.0～1.0)</param>
			/// <param name="tA">係数A</param>
			/// <param name="tB">係数B</param>
			/// <param name="tC">係数C</param>
			public static void CalcCoefficient(float t, out float tA, out float tB, out float tC)
			{
				t = Mathf.Clamp01(t);
				tA = t * t;
				float tR = 1.0f - t;
				tB = 2.0f * t * tR;
				tC = tR * tR;
			}

			/// <summary>
			/// 座標計算
			/// </summary>
			/// <param name="t">媒介変数(0.0～1.0)</param>
			/// <param name="p0">制御点0</param>
			/// <param name="p1">制御点1</param>
			/// <param name="p2">制御点2</param>
			/// <returns>座標</returns>
			public static float Evaluate(float t, float p0, float p1, float p2)
			{
				float tA, tB, tC;
				CalcCoefficient(t, out tA, out tB, out tC);
				float v = (p2 * tA) + (p1 * tB) + (p0 * tC);
				return v;
			}

			public static Vector2 Evaluate(float t, Vector2 p0, Vector2 p1, Vector2 p2)
			{
				float tA, tB, tC;
				CalcCoefficient(t, out tA, out tB, out tC);
				Vector2 v = (p2 * tA) + (p1 * tB) + (p0 * tC);
				return v;
			}

			public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2)
			{
				float tA, tB, tC;
				CalcCoefficient(t, out tA, out tB, out tC);
				Vector3 v = (p2 * tA) + (p1 * tB) + (p0 * tC);
				return v;
			}
		}

		//==========================================================================
		//==========================================================================
		//==========================================================================
		//３次ベジェ曲線
		//==========================================================================
		//==========================================================================
		//==========================================================================
		public class CurveBezier3 : ICurveEvaluator
		{
			public	Vector3		p0			{ get; set; }
			public	Vector3		p1			{ get; set; }
			public	Vector3		p2			{ get; set; }
			public	Vector3		p3			{ get; set; }

			//==========================================================================
			/**
			 *    @brief       コンストラクタ
			 *
			 *    @param[in]   p0   始点
			 *    @param[in]   p1   始点側制御点
			 *    @param[in]   p2   終点側制御点
			 *    @param[in]   p3   終点
			 */
			//==========================================================================
			public CurveBezier3( Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 )
			{
				this.p0 = p0;
				this.p1 = p1;
				this.p2 = p2;
				this.p3 = p3;
			}

			//==========================================================================
			/**
			 *    @brief       指定した媒介変数における座標を得る
			 *
			 *    @param[in]   t          0.0～1.0
			 *    @return      ベジェ曲線上の座標
			 */
			//==========================================================================
			public Vector3 Evaluate( float t )
			{
				Vector3 v = Evaluate(t, this.p0, this.p1, this.p2, this.p3);
				return v;
			}

			/// <summary>
			/// 係数計算
			/// </summary>
			/// <param name="t">媒介変数(0.0～1.0)</param>
			/// <param name="tA">係数A</param>
			/// <param name="tB">係数B</param>
			/// <param name="tC">係数C</param>
			/// <param name="tD">係数D</param>
			public static void CalcCoefficient(float t, out float tA, out float tB, out float tC, out float tD)
			{
				t = Mathf.Clamp01(t);
				float tt = t * t;
				float tR = 1.0f - t;
				float tRR = tR * tR;

				tA = tt * t;
				tB = 3.0f * tt * tR;
				tC = 3.0f * t * tRR;
				tD = tRR * tR;
			}

			/// <summary>
			/// 座標計算
			/// </summary>
			/// <param name="t">媒介変数(0.0～1.0)</param>
			/// <param name="p0">制御点0</param>
			/// <param name="p1">制御点1</param>
			/// <param name="p2">制御点2</param>
			/// <param name="p3">制御点3</param>
			/// <returns>座標</returns>
			public static float Evaluate(float t, float p0, float p1, float p2, float p3)
			{
				float tA, tB, tC, tD;
				CalcCoefficient(t, out tA, out tB, out tC, out tD);
				float v = (p3 * tA) + (p2 * tB) + (p1 * tC) + (p0 * tD);
				return v;
			}

			public static Vector2 Evaluate(float t, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
			{
				float tA, tB, tC, tD;
				CalcCoefficient(t, out tA, out tB, out tC, out tD);
				Vector2 v = (p3 * tA) + (p2 * tB) + (p1 * tC) + (p0 * tD);
				return v;
			}

			public static Vector3 Evaluate(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
			{
				float tA, tB, tC, tD;
				CalcCoefficient(t, out tA, out tB, out tC, out tD);
				Vector3 v = (p3 * tA) + (p2 * tB) + (p1 * tC) + (p0 * tD);
				return v;
			}
		}
	}
}
