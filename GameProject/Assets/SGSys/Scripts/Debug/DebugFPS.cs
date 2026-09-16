//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     DebugFPS.cs
 *    @brief    FPS計測処理
 * 
 *    DebugTextを利用した文字描画からuGUIのテキストに変更
 *
 *    参照ページ:http://docs.unity3d.com/jp/current/ScriptReference/Time-realtimeSinceStartup.html
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using UnityEngine;
using UnityEngine.UI;

namespace SGSys
{
    public class DebugFPS : MonoBehaviour
    {
        private const float UPDATE_INTERVAL = 0.5f; // この時間おきにFPSを計算して表示させる 

        private float	mLastInterval;
        private int		mFrames = 0;
        private float	mFps;

        private static DebugFPS mInstance = null;
        
        public static DebugFPS Instance
        {
            get
            {
                return mInstance;
            }
        }

        [SerializeField]
        private Text m_Label;

        [SerializeField]
        private RectTransform m_baseRect;


        private Shadow mShadow;
        
        //================================================
        //! 直近のFPS値取得 
        //================================================
        public float fps
        {
            get
            {
                return mFps;
            }
        }
        
        public static DebugFPS Create( DebugFPS prefab, Transform parent = null )
        {
#if GAME_DEBUG
            if ( !Debug.isDebugBuild )
            {
                return null;
            }
            if ( false == GameObject.Find( "DebugFPS" ) )
            {
                DebugFPS dfps = parent != null
                    ? DebugFPS.Instantiate( prefab, parent, false ) as DebugFPS
                    : DebugFPS.Instantiate( prefab, new Vector3(0.95f, 0.99f, 0.0f), Quaternion.identity ) as DebugFPS;
                dfps.gameObject.name = "DebugFPS";
                DebugLog.Info( SystemConst.DebugGroup.System,"DebugFPS.Create");
                return dfps;
            } else {
                DebugLog.Info( SystemConst.DebugGroup.System,"DebugFPS.Create : already exist");
            }
#endif
            return null;
        }
        
        public DebugFPS()
        {
        }
        
        private void Awake()
        {
            mInstance = this;
            InitializeView();
            SetAnchorUL();

            mShadow = m_Label.GetComponent<Shadow>();
            if (mShadow == null) {
                mShadow = m_Label.gameObject.AddComponent<Shadow>();
                mShadow.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (mInstance == this) {
                mInstance = null;
            }
        }

        private void OnEnable()
        {
            mInstance = this;
            mLastInterval = Time.realtimeSinceStartup;
            mFrames = 0;
        }
        
        private void Start()
        {
            mLastInterval = Time.realtimeSinceStartup;
            mFrames = 0;
            mFps = 0.0f;

        }
     
        private void Update()
        {
            ++mFrames;
            float time = Time.realtimeSinceStartup - mLastInterval;
            if ( UPDATE_INTERVAL <= time ) {
                mFps = mFrames / time;
                mLastInterval = Time.realtimeSinceStartup;
                mFrames = 0;
                m_Label.text = string.Format("FPS : {0:F3}", mFps);
#if false
                m_Label.text += string.Format("\nusedHeap {0:D} / {1:D} MB", Profiler.usedHeapSize/1048576 , SystemInfo.systemMemorySize );
                m_Label.text += string.Format("\nGC.GetTotal {0:D} MB", System.GC.GetTotalMemory(false) / 1048576 );
#endif
            }
        }

        private void InitializeView()
        {
            if (m_Label != null && m_baseRect == null)
            {
                m_baseRect = m_Label.rectTransform;
            }

            if (m_Label != null && m_baseRect != null)
            {
                return;
            }

            GameObject canvasObject = new GameObject("DebugFPSCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10000;

            GameObject labelObject = new GameObject("FPSLabel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Shadow));
            labelObject.transform.SetParent(canvasObject.transform, false);

            m_baseRect = labelObject.GetComponent<RectTransform>();
            m_baseRect.sizeDelta = new Vector2(200, 30);
            m_baseRect.anchoredPosition = new Vector2(8, -8);

            m_Label = labelObject.GetComponent<Text>();
            m_Label.text = "FPS : 0.000";
            m_Label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            m_Label.fontSize = 16;
            m_Label.alignment = TextAnchor.UpperLeft;
            m_Label.raycastTarget = false;
            m_Label.color = Color.white;
        }

        /// <summary>
        /// フォントサイズ変更
        /// </summary>
        /// <param name="size"></param>
        private void SetFontSize(int size)
        {
            m_Label.fontSize = size;
        }

        /// <summary>
        /// 基準点を左下に
        /// Lower Left
        /// 毎フレーム呼び出すのはあまり推奨しません
        /// </summary>
        public void SetAnchorLL()
        {
            SetAnchor(0,0);
        }
        /// <summary>
        /// 基準点を右下に
        /// Lower Right
        /// 毎フレーム呼び出すのはあまり推奨しません
        /// </summary>
        public void SetAnchorLR()
        {
            SetAnchor(1, 0);
        }

        /// <summary>
        /// 基準点を左上に
        /// Upper Left
        /// 毎フレーム呼び出すのはあまり推奨しません
        /// </summary>
        public void SetAnchorUL()
        {
            SetAnchor(0, 1);
        }

        /// <summary>
        /// 基準点を右上に
        /// Upper Right
        /// 毎フレーム呼び出すのはあまり推奨しません
        /// </summary>
        public void SetAnchorUR()
        {
            SetAnchor(1, 1);
        }

        /// <summary>
        /// 基準点の変更
        /// 毎フレーム呼び出すのはあまり推奨しません
        /// </summary>
        /// <param name="x">0(左)～1(右)</param>
        /// <param name="y">0(下)～1(上)</param>
        private void SetAnchor(float x, float y)
        {
            RectTransform rt = m_baseRect;
            Vector2 work = new Vector2(x, y);
            rt.anchorMax = work;
            rt.anchorMin = work;
            rt.pivot = work;

        }

        /// <summary>
        /// テキストカラーの設定
        /// </summary>
        /// <param name="color"></param>
        public void SetColor( Color color )
        {
            m_Label.color = color;
        }

        /// <summary>
        /// 影カラーの設定
        /// </summary>
        /// <param name="color"></param>
        public void SetShadowColor( Color color )
        {
            mShadow.effectColor = color;
        }

        /// <summary>
        /// 影表示の有効化
        /// </summary>
        /// <param name="enabled"></param>
        public void SetShadow( bool enabled )
        {
            mShadow.enabled = enabled;
        }
    }
}	//namespace SGSys
