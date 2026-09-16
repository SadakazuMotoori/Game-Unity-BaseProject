using UnityEngine;


namespace SGSys
{
	public static partial class Math
	{
		/// <summary>
		/// Smooth Nonuniformed Spline
		/// 平滑不均一スプライン
		/// </summary>
		/// <remarks>
		/// 連続した加速度の移動を行います
		/// 各節点に対して滑らかに加減速を行います。
		/// </remarks>
		public class SNSpline : RNSpline
		{
			/// <summary>
			/// 平滑化
			/// </summary>
			protected virtual void Smooth()
			{
				Vector3 newVel;
				Vector3 oldVel = GetStartVelocity(0);
				for ( int i=1; i<this.nodeCount-1; ++i )
				{

					newVel = GetEndVelocity(i)*mNodeList[i].distance + GetStartVelocity(i)*mNodeList[i-1].distance;
					if ( mNodeList[i-1].distance + mNodeList[i].distance == 0.0f )
					{
						newVel = Vector3.zero;
					}
					else
					{
						newVel /= (mNodeList[i-1].distance + mNodeList[i].distance);
					}
					mNodeList[i-1].velocity = oldVel;
					oldVel = newVel;
				}

				mNodeList[this.nodeCount-1].velocity = GetEndVelocity(this.nodeCount-1);
				mNodeList[this.nodeCount-2].velocity = oldVel;

                if ( this.loop )
				{
                    mNodeList[this.nodeCount-1].velocity = mNodeList[0].velocity;
                }
			}

			/// <summary>
			/// スプライン情報の構築
			/// </summary>
			public override void Build( bool _loop=false )
			{
				base.Build(_loop);
				Smooth();
				Smooth();
				Smooth();
			}
		}
	}
}
