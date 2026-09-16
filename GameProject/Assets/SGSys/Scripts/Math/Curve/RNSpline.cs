using UnityEngine;
using System.Collections.Generic;

namespace SGSys
{
	public static partial class Math
	{
		/// <summary>
		/// Rounded Nonuniformed Spline
		/// 丸み不均一スプライン
		/// </summary>
		/// <remarks>
		/// 曲線上をなるべく等速で移動したい場合に利用します。
		/// ただし曲線は緩やかになる傾向があります。
		/// </remarks>
		public class RNSpline : ICurveEvaluator
		{
			private static Matrix4x4	mHermite;

			protected class Node
			{
				public	Vector3	position;
				public	Vector3	velocity;
				public	float	distance;

				public Node( Vector3 p )
				{
					this.position = p;
				}

				public void MakeStartVelocity( Node next )
				{
					if ( this.distance == 0.0f )
					{
						this.velocity = Vector3.zero;
						return;
					}
					Vector3 temp = ((next.position - this.position) * 3.0f) / this.distance;
					this.velocity = (temp - next.velocity) * 0.5f;
				}

				public void MakeEndVelocity( Node prev )
				{
					if ( prev.distance == 0.0f )
					{
						this.velocity = Vector3.zero;
						return;
					}
					Vector3 temp = ((this.position - prev.position) * 3.0f) / prev.distance;
					this.velocity = (temp - prev.velocity) * 0.5f;
				}

				public void MakeVelocity( Vector3 p0, Vector3 p1, Vector3 p2 )
				{
					this.velocity = (p2-p1).normalized - (p0-p1).normalized;
					this.velocity.Normalize();
				}
			}

            /// <summary>
            /// スプライン曲線の長さ
            /// </summary>
			protected	float		maxDistance		{ get; set; }

			protected	List<Node>	mNodeList;
			private Node mLoopEndNode;

			protected	int			nodeCount {
				get {
					return mNodeList.Count;
				}
			}

            protected bool loop { get; private set; }
			
            protected Node startNode { get { return mNodeList[0]; } }
            protected Node endNode { get { return mNodeList[mNodeList.Count-1]; } }


			/// <summary>
			/// コンストラクタ
			/// </summary>
			public RNSpline()
			{
				mNodeList = new List<Node>(16);
			}

            /// <summary>
            /// 登録されているノードをクリア
            /// </summary>
            public void ClearNodes()
			{
                mNodeList.Clear();
                this.maxDistance = 0.0f;
                this.loop = false;
                mLoopEndNode = null;
            }

            protected void RemoveLoopEnd()
            {
                if ( mLoopEndNode != null )
                {
                    this.maxDistance -= mNodeList[mNodeList.Count-2].distance;
                    mNodeList.RemoveAt(mNodeList.Count-1);
                    this.endNode.distance = 0.0f;
                    mLoopEndNode = null;
                }
            }

			/// <summary>
			/// ノードの追加
			/// </summary>
			/// <param name="p">曲線通過座標</param>
			public void AddNode( Vector3 p )
			{
                RemoveLoopEnd();
				if ( 0 < mNodeList.Count )
				{
					Node prevNode = mNodeList[mNodeList.Count-1];
					prevNode.distance = (prevNode.position-p).magnitude;
					this.maxDistance += prevNode.distance;
				}

				mNodeList.Add( new Node(p) );
			}

            /// <summary>
            /// 指定ノードの座標を取得
            /// </summary>
            /// <param name="index">ノード番号</param>
            /// <returns>指定ノードの座標</returns>
            public Vector3 GetPosition( int index )
			{
                Node node = mNodeList[index];
                return node.position;
            }

            /// <summary>
            /// 指定ノードの座標設定
            /// 
            /// SetPositionにて設定した座標はUpdateDistance, Buildされるまで反映されません
            /// </summary>
            /// <param name="index">ノード番号</param>
            /// <param name="p">設定座標</param>
            public void SetPosition( int index, Vector3 p )
			{
                Node node = mNodeList[index];
                node.position = p;
            }

            /// <summary>
            /// ノードの座標を変更した時の各ノードの距離情報更新
            /// </summary>
            public void UpdateDistance()
			{
                if ( mLoopEndNode != null )
                {
                    mLoopEndNode.position = this.startNode.position;
                }
                this.maxDistance = 0.0f;
                for( int i=1; i<mNodeList.Count; ++i )
				{
                    var prev = mNodeList[i-1];
                    var current = mNodeList[i];
                    var d = (prev.position - current.position).magnitude;
                    prev.distance = d;
                    this.maxDistance += d;
                }
            }

			/// <summary>
			/// スプライン情報の構築
			/// </summary>
			public virtual void Build( bool _loop )
			{
                RemoveLoopEnd();
                if ( mNodeList.Count < 2 )
                {
                    throw new System.InvalidOperationException("At least two nodes are required to build a spline.");
                }
                this.loop = _loop;

                if ( this.loop )
				{
				    this.startNode.MakeVelocity( this.endNode.position, this.startNode.position, mNodeList[1].position );

				    for ( int i=1; i<mNodeList.Count-1; ++i )
					{
					    mNodeList[i].MakeVelocity( mNodeList[i-1].position, mNodeList[i].position, mNodeList[i+1].position );
				    }

                    this.endNode.MakeVelocity( mNodeList[mNodeList.Count-2].position, this.endNode.position, this.startNode.position );

                    AddNode( this.startNode.position );
                    mLoopEndNode = this.endNode;
                    this.endNode.velocity = this.startNode.velocity;

                } 
				else
				{
				    for ( int i=1; i<mNodeList.Count-1; ++i )
					{
					    mNodeList[i].MakeVelocity( mNodeList[i-1].position, mNodeList[i].position, mNodeList[i+1].position );
				    }

				    if ( mNodeList.Count == 2 )
				    {
				        mNodeList[1].velocity = mNodeList[0].distance == 0.0f
				            ? Vector3.zero
				            : (mNodeList[1].position - mNodeList[0].position) / mNodeList[0].distance;
				    }
				    mNodeList[0].MakeStartVelocity( mNodeList[1] );
				    mNodeList[mNodeList.Count-1].MakeEndVelocity( mNodeList[mNodeList.Count-2] );
                }
			}

			/// <summary>
			/// 正規化時間における値を求める
			/// </summary>
			/// <param name="normalizedTime">正規化時間</param>
			/// <returns>normalizedTime時の位置</returns>
			public Vector3 Evaluate( float normalizedTime )
			{
				normalizedTime = Mathf.Clamp01( normalizedTime );
                if ( normalizedTime <= 0.0f || this.maxDistance == 0.0f )
                {
                    return this.startNode.position;
                }
                if ( normalizedTime >= 1.0f )
                {
                    return this.endNode.position;
                }
				float dist = normalizedTime * this.maxDistance;
				float currentDist = 0.0f;

				int i = 0;
                while ( i < mNodeList.Count-1 )
				{
	                float l = currentDist+mNodeList[i].distance;
                    if ( mNodeList[i].distance > 0.0f && l >= dist )
					{
                        break;
                    }
					currentDist += mNodeList[i].distance;
					++i;
                }

                if ( i==mNodeList.Count-1 )
				{
                    return mNodeList[i].position;
                }
				else
				{
    				float t;
                    t  = dist - currentDist;
				    t /= mNodeList[i].distance;
                    t = Mathf.Clamp01(t);
				    Vector3 v0 = mNodeList[i].velocity * mNodeList[i].distance;
				    Vector3 v1 = mNodeList[i+1].velocity * mNodeList[i].distance;
				    return GetPosition( mNodeList[i].position, v0, mNodeList[i+1].position, v1, t );
                }
			}

			private Vector3 GetPosition( Vector3 p0, Vector3 v0, Vector3 p1, Vector3 v1, float t )
			{
				float t2 = t * t;
				float t3 = t2 * t;
				
				Vector3	v = ( (2.0f*p0)  + v0 - (2.0f*p1) + v1 ) * t3
				          + ( (-3.0f*p0) - (2.0f*v0) + (3.0f*p1) - v1 ) * t2
				          + v0 * t
				          + p0;
				return v;
			}

            public Vector3 GetVelocity( int index )
			{
                return mNodeList[index].velocity;
            }

			public Vector3 GetStartVelocity( int index )
			{
				if ( mNodeList[index].distance == 0.0f )
				{
					return Vector3.zero;
				}
				Vector3 temp = 3.0f * (mNodeList[index+1].position - mNodeList[index].position) / mNodeList[index].distance;
				return (temp - mNodeList[index+1].velocity) * 0.5f;
			}

			public Vector3 GetEndVelocity( int index )
			{
				if ( mNodeList[index-1].distance == 0.0f )
				{
					return Vector3.zero;
				}
				Vector3 temp = 3.0f * (mNodeList[index].position - mNodeList[index-1].position) / mNodeList[index-1].distance;
				return (temp - mNodeList[index-1].velocity) * 0.5f;
			}
		}
	}
}

