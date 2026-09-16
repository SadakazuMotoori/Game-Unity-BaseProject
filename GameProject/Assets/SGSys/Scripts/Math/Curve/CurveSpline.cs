//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!

 *    @file     CurveSpline.cs
 *    @brief    Catmull-Romスプライン曲線
 *
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;

namespace SGSys {
	public static partial class Math
	{
		public class CurveSpline : ICurveEvaluator
		{	

			private static Vector4[] mMatrixIntermediate = new Vector4[4]
			{
				new Vector4(-1.0f, 3.0f, -3.0f, 1.0f),
				new Vector4( 2.0f,-5.0f,  4.0f,-1.0f),
				new Vector4(-1.0f, 0.0f,  1.0f, 0.0f),
				new Vector4( 0.0f, 2.0f,  0.0f, 0.0f)
			};
			
			private static Vector4[] mMatrixStart = new Vector4[4]
			{
				new Vector4( 0.0f, 0.0f, 0.0f, 0.0f),
				new Vector4( 0.0f, 1.0f,-2.0f, 1.0f),
				new Vector4( 0.0f,-3.0f, 4.0f,-1.0f),
				new Vector4( 0.0f, 2.0f, 0.0f, 0.0f)
			};

			private static Vector4[] mMatrixEnd = new Vector4[4]
			{
				new Vector4( 0.0f, 0.0f, 0.0f, 0.0f),
				new Vector4( 1.0f,-2.0f, 1.0f, 0.0f),
				new Vector4(-1.0f, 0.0f, 1.0f, 0.0f),
				new Vector4( 0.0f, 2.0f, 0.0f, 0.0f) 
			};

			private static Vector4[][] mMatrixTable = new Vector4[][]
			{
				mMatrixStart,
				mMatrixIntermediate,
				mMatrixEnd
			};

			private enum Section
			{
				  Start
				, Intermediate
				, End
			};

			private Vector4		mTime;
			private	Vector4[]	mWork = new Vector4[3];

			public	Vector3		p0		{ get; set; }
			public	Vector3		p1		{ get; set; }
			public	Vector3		p2		{ get; set; }
			public	Vector3		p3		{ get; set; }
			
			//==========================================================================
			/**
			 *    @brief       コンストラクタ
			 */
			//==========================================================================
			public CurveSpline() : this( Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero )
			{
			}

			//==========================================================================
			/**
			 *    @brief       コンストラクタ
			 *
			 *    @param[in]   p0_    補助座標1
			 *    @param[in]   p1_    始点
			 *    @param[in]   p2_    終点
			 *    @param[in]   p3_    補助座標2
			 *
			 *    こちらの場合、p1-p2間の補間を行うだけになります
			 */
			//==========================================================================
			public CurveSpline( Vector3 p0_, Vector3 p1_, Vector3 p2_, Vector3 p3_ )
			{
				this.p0 = p0_;
				this.p1 = p1_;
				this.p2 = p2_;
				this.p3 = p3_;
			}

			//==========================================================================
			/**
			 *    @brief       コンストラクタ
			 *
			 *    @param[in]   array   通過する座標の配列
			 *
			 *    array[0]からarray[ array.Length-1 ]の全頂点を補間します
			 */
			//==========================================================================
			public CurveSpline( Vector3[] array ) 
			{
				SetPoints( array );
			}
			
			//==========================================================================
			/**
			 *    @brief       座標計算
			 *
			 *    @param[in]   t            媒介変数(0.0～1.0)
			 *    @return      tにおける座標
			 */
			//==========================================================================
			public Vector3 Evaluate( float t )
			{
				Setup( t, this.p0, this.p1, this.p2, this.p3 );
				return Evaluate( mMatrixIntermediate );
			}
			
			//==========================================================================
			/**
			 *    @brief       Setupした情報に基づき座標を求める
			 */
			//==========================================================================
			private Vector3 Evaluate( Vector4[] mat )
			{
				Vector3 result;
				
				Vector4 tmp;
				
				tmp.x = Vector4.Dot( mat[0], mWork[0] );
				tmp.y = Vector4.Dot( mat[1], mWork[0] );
				tmp.z = Vector4.Dot( mat[2], mWork[0] );
				tmp.w = Vector4.Dot( mat[3], mWork[0] );
				result.x = Vector4.Dot( tmp, mTime ) * 0.5f;
				
				tmp.x = Vector4.Dot( mat[0], mWork[1] );
				tmp.y = Vector4.Dot( mat[1], mWork[1] );
				tmp.z = Vector4.Dot( mat[2], mWork[1] );
				tmp.w = Vector4.Dot( mat[3], mWork[1] );
				result.y = Vector4.Dot( tmp, mTime ) * 0.5f;

				tmp.x = Vector4.Dot( mat[0], mWork[2] );
				tmp.y = Vector4.Dot( mat[1], mWork[2] );
				tmp.z = Vector4.Dot( mat[2], mWork[2] );
				tmp.w = Vector4.Dot( mat[3], mWork[2] );
				result.z = Vector4.Dot( tmp, mTime ) * 0.5f;
				
				return result;
			}
			
			
			//==========================================================================
			/**
			 *    @brief       座標設定処理
			 */
			//==========================================================================
			private void Setup( float t, Vector3 p0_, Vector3 p1_, Vector3 p2_, Vector3 p3_ )
			{
				mTime.x = t*t*t;
				mTime.y = t*t;
				mTime.z = t;
				mTime.w = 1.0f;
				
				mWork[0].x = p0_.x;
				mWork[0].y = p1_.x;
				mWork[0].z = p2_.x;
				mWork[0].w = p3_.x;
					
				mWork[1].x = p0_.y;
				mWork[1].y = p1_.y;
				mWork[1].z = p2_.y;
				mWork[1].w = p3_.y;
					
				mWork[2].x = p0_.z;
				mWork[2].y = p1_.z;
				mWork[2].z = p2_.z;
				mWork[2].w = p3_.z;
			}
			
			private Vector3[]	mPointArray;
			private int			mSectionCount;
			private	int			mEndIndex;
			
			//==========================================================================
			/**
			 *    @brief       補間座標配列の設定
			 */
			//==========================================================================
			public void SetPoints( Vector3[] array )
			{
				if ( array == null )
				{
					throw new System.ArgumentNullException(nameof(array));
				}
				if ( array.Length < 3 )
				{
					throw new System.ArgumentException("At least three points are required.", nameof(array));
				}
				DebugUtil.Assert( array.Length>2, "Curvespline.SetPonts : points need greater than 2.");
				mPointArray		= array;
				mSectionCount	= array.Length - 1;
				mEndIndex		= array.Length - 2;
			}

			//==========================================================================
			/**
			 *    @brief       座標配列の補間処理
			 *
			 *    SetPointsで設定した座標をtotalTimeかけて移動する為の補間処理を行います。
			 *
			 *    @param[in]   currentTime   補間開始からの経過時間
			 *    @param[in]   totalTime     補間総時間
			 *    @return      現在の座標
			 */
			//==========================================================================
			public	int		section			{ get; set; }
			public	float	sectionRatio	{ get; private set; }	//!< 現在のセクションの進捗率
			public Vector3 EvaluatePoints( float current, float total )
			{
				return Evaluate( current, total );
			}

			public Vector3 Evaluate( float currentTime, float totalTime )
			{
				if ( 0.0f >= currentTime )
				{
					this.section = 0;
					this.sectionRatio = 0.0f;
					return mPointArray[0];
				}

				if ( totalTime <= currentTime )
				{
					this.section = mEndIndex;
					this.sectionRatio = 1.0f;
					return mPointArray[ mPointArray.Length-1 ];
				}
				
				float sectionTime = totalTime / (float)mSectionCount;
				int index = (int)(currentTime / sectionTime); 
				if ( mEndIndex < index )
				{
					index = mEndIndex;
				}

				this.section = index;
				
				float offsetTime = currentTime - index * sectionTime;

				float t = offsetTime / sectionTime;
				this.sectionRatio = t;
				
				Vector4[] mat;
				if ( 0 == index )
				{
					//開始
					mat = mMatrixTable[(int)Section.Start];
					Setup( t, Vector3.zero, mPointArray[0], mPointArray[1], mPointArray[2] );
				}
				else if ( mEndIndex == index )
				{
					//終了
					mat = mMatrixTable[(int)Section.End];
					Setup( t, mPointArray[index-1], mPointArray[index], mPointArray[index+1], Vector3.zero );
				}
				else
				{
					//中間
					Setup( t, mPointArray[index-1], mPointArray[index], mPointArray[index+1], mPointArray[index+2] );
					mat = mMatrixTable[(int)Section.Intermediate];
				}
				
				return Evaluate( mat );
			}
		}
	}
}

