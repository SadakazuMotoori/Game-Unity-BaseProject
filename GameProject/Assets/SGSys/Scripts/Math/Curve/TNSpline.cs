using UnityEngine;

namespace SGSys
{
	public static partial class Math
	{
		/// <summary>
		/// Time Nonuniformed Spline
		/// 時間不均一スプライン
		/// </summary>
		/// <remarks>
		/// </remarks>
		public class TNSpline : SNSpline
		{
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

			/// <summary>
			/// ノードの追加
			/// </summary>
			/// <param name="p">追加するノード座標</param>
			/// <param name="timeperiod"></param>
			public void AddNode( Vector3 p, float timeperiod )
			{
				if ( this.nodeCount > 0 && (!(timeperiod > 0.0f) || float.IsInfinity(timeperiod)) )
				{
					throw new System.ArgumentOutOfRangeException(nameof(timeperiod));
				}
				RemoveLoopEnd();
				if ( 0 == this.nodeCount )
				{
					this.maxDistance = 0.0f;
				} 
				else
				{
					mNodeList[this.nodeCount-1].distance = timeperiod;
					this.maxDistance += timeperiod;
				}

				mNodeList.Add( new Node(p) );
			}


			/// <summary>
			/// 平滑化
			/// </summary>
			protected override void Smooth()
			{
				base.Smooth();
				Constrain();
			}

			private void Constrain()
			{
				for ( int i=1; i<this.nodeCount-1; ++i )
				{
					if ( mNodeList[i-1].distance == 0.0f || mNodeList[i].distance == 0.0f )
					{
						mNodeList[i].velocity = Vector3.zero;
						continue;
					}
					float r0 = (mNodeList[i].position - mNodeList[i-1].position).magnitude / mNodeList[i-1].distance;
					float r1 = (mNodeList[i+1].position - mNodeList[i].position).magnitude / mNodeList[i].distance;
					if ( r0 + r1 == 0.0f )
					{
						mNodeList[i].velocity = Vector3.zero;
					}
					else
					{
						mNodeList[i].velocity *= (4.0f*r0*r1) / ((r0+r1)*(r0+r1));
					}
				}
			}
		}
	}
}