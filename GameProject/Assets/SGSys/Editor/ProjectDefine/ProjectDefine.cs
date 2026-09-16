#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SGSys
{
    public partial class ProjectDefine : EditorWindow
    {
        /// <summary>
        /// ウィンドウタイトル
        /// </summary>
        static readonly string TITLE = "ProjectDefine";
        /// <summary>
        /// 保存先アセット名（プロジェクトで唯一）
        /// </summary>
        static readonly string ASSET_NAME = "ProjectDefineSettings.asset";

        string mAssetPath;
        ProjectDefineSettings    mSettings;
        int mLastId;

        /// <summary>
        /// ウィンドウ起動
        /// </summary>
        [MenuItem( "SGG/プロジェクト用プリプロセッサ定義", false, 0)]
        static void Open() {
            GetWindow<ProjectDefine>(TITLE);
        }

        enum LoadState {
            Success,
            NotExistAsset,
            FailLoadAsset,
        }

        static LoadState LoadSettingsInner( out string assetPath, out ProjectDefineSettings settings ) {
            assetPath = "";
            settings = null;

            string[] guidList = AssetDatabase.FindAssets("ProjectDefineSettings t:ScriptableObject");
            if ( guidList.Length == 0 ) {
                return LoadState.NotExistAsset;
            }

            assetPath= AssetDatabase.GUIDToAssetPath( guidList[0] );
            settings = AssetDatabase.LoadAssetAtPath<ProjectDefineSettings>( assetPath );
            if ( null == settings ) {
                return LoadState.FailLoadAsset;
            }
            return LoadState.Success;
        }

        void LoadSettings() {
            switch( LoadSettingsInner( out mAssetPath, out mSettings ) ) {
            case LoadState.Success:
                break;
            case LoadState.NotExistAsset:
                EditorUtility.DisplayDialog(
                    TITLE,

                    ASSET_NAME+"が\n" +
                    "見つからなかったので終了します。",

                    "了解");
                break;
            case LoadState.FailLoadAsset:
                EditorUtility.DisplayDialog(
                    TITLE,
                    ASSET_NAME+"の読み込みに失敗しました",
                    "了解" );
                break;
            }
        }

        ProjectDefine_TreeView    mTreeView;
        List<ProjectDefine_TreeViewItem> mItemList;

        private void OnEnable () {
            if ( null == mSettings ) {
                LoadSettings();
            }
            if ( null != mTreeView ) {
                return;
            }
            mItemList = ProjectDefine_TreeViewItem.MakeItemList( mSettings );
            mLastId = 1;
            if ( 0 != mItemList.Count ) {
                mLastId = mItemList[ mItemList.Count-1 ].id;
            }
            mTreeView = ProjectDefine_TreeView.MakeTreeView( mItemList );
        }

        void OnGUI() {
            using ( new EditorGUILayout.HorizontalScope() ) {
                if ( GUILayout.Button( "追加", GUILayout.MaxWidth(100) ) ) {
                    AddItem();
                }
                if ( GUILayout.Button( "削除", GUILayout.MaxWidth(100) ) ) {
                    DeleteItem();
                }
            }

            float topSpace = 10;
            float bottomSpace = 60;

            var singleLineHeight = EditorGUIUtility.singleLineHeight;
            float viewY = topSpace + singleLineHeight;

            var treeViewRect = new Rect
            {
                x      = 0,
                y      = viewY,
                width  = position.width,
                height = position.height - (viewY+bottomSpace)
            };

            mTreeView?.OnGUI( treeViewRect );

            float btnSpace = 10;
            float btnH = bottomSpace-btnSpace;
            float btnY = treeViewRect.y + treeViewRect.height + btnSpace;
            float btnW = position.width/2;

            if ( GUI.Button( new Rect(   0, btnY, btnW, btnH), "中止" ) ) {
                if ( EditorUtility.DisplayDialog( TITLE,"プロジェクトに保存せず中止して良いですか？", "はい", "いいえ" ) ) {
                    Close();
                }
            }
            if ( GUI.Button( new Rect(btnW, btnY, btnW, btnH), "保存" ) ) {
                if ( EditorUtility.DisplayDialog( TITLE,"プロジェクトに保存して良いですか？", "はい", "いいえ" ) ) {
                    Save();
                    EditorUtility.DisplayDialog( TITLE,
                        "プロジェクトに保存しました\n"+
                        "保存先："+mAssetPath,
                        "了解" );
                    Close();
                }
            }

        }
        /// <summary>
        /// 選択した列の後ろに追加
        /// </summary>
        void AddItem() {
    #if true //選択した後ろに追加型
            if ( 0 == mItemList.Count ) {
                var item = new ProjectDefine_TreeViewItem( mLastId );
                mItemList.Add( item );
            } else {
                var lastItem = mItemList[mItemList.Count-1];

                mLastId++;
                var selection = mTreeView.GetSelection();
                if ( selection.Count == 0 ) {
                    //選択が無いなら最後尾へ
                    var item = new ProjectDefine_TreeViewItem( mLastId );
                    mItemList.Add( item );
                } else {
                    //選択があるならその後ろへ
                    var index= SearchItemIndex( selection[ selection.Count-1 ] );
                    var item = new ProjectDefine_TreeViewItem( mLastId );
                    mItemList.Insert( index+1, item );
                }
            }

    #else  //最後尾追加型
            var lastItem = mItemList[mItemList.Count-1];
            var item = new GlobalDefinesWizard_TreeViewItem( lastItem.id+1 );
            mItemList.Add( item );
    #endif
            mTreeView.Reload();
        }

        /// <summary>
        /// アイテムリストの中から該当するIDを持つアイテムのインデックスを探す
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int SearchItemIndex( int id ) {
            for ( int i=0; i<mItemList.Count; ++i ) {
                if ( mItemList[i].id == id ) {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// アイテムリストの中から該当するIDを持つアイテムを探す
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        ProjectDefine_TreeViewItem SearchItem( int id ) {
            foreach( var item in mItemList ) {
                if ( id == item.id ) {
                    return item;
                }
            }
            return null;
        }
        /// <summary>
        /// 選択した列の削除
        /// </summary>
        void DeleteItem() {
            var list = mTreeView.GetSelection();
            if ( list.Count == 0 ) {
                EditorUtility.DisplayDialog( TITLE, "削除したい行が選択されていません。", "了解");
                return;
            }

            var removeList = new List<ProjectDefine_TreeViewItem>();

            foreach ( var removeId in list ) {
                var removeItem = SearchItem( removeId );
                if ( null == removeItem ) {
                    continue;
                }
                removeList.Add( removeItem );
            }
            foreach( var removeItem in removeList ) {
                mItemList.Remove( removeItem );
            }
            mTreeView.Reload();
        }

        void Save() {

            string defines = "";

            mSettings.items = new ProjectDefine_TreeViewItem[mItemList.Count];
            for ( int i=0; i<mItemList.Count; ++i ) {
                mSettings.items[i] = new ProjectDefine_TreeViewItem(i+1, mItemList[i]);
            }

            foreach( var item in mSettings.items ) {
                if ( !item.isValid ) {
                    continue;
                }
                defines += item.define + ";";
            }

            ApplyDefines( defines );

            EditorUtility.SetDirty( mSettings );
            AssetDatabase.SaveAssets();

        }
        /// <summary>
        /// Unityのプロジェクトにプリプロセッサ定義を行う
        /// 呼び出した後、スクリプトのコンパイルが働きます。
        /// </summary>
        /// <param name="defines">設定する定義文字列。
        /// セミコロンで区切って定義文字列を記述します。
        /// 
        /// 例：
        /// defines = "GAME_DEBUG;SGSYS_DEBUG;TEST_MODE;";
        /// ApplyDefines( defines );
        /// </param>
        private static void ApplyDefines( string defines ) {
            PlayerSettings.SetScriptingDefineSymbols( UnityEditor.Build.NamedBuildTarget.iOS, defines );
            PlayerSettings.SetScriptingDefineSymbols( UnityEditor.Build.NamedBuildTarget.Android, defines );
            PlayerSettings.SetScriptingDefineSymbols( UnityEditor.Build.NamedBuildTarget.Standalone, defines );
        }

        /// <summary>
        /// 現在の設定を変更する
        /// </summary>
        /// <param name="defines"></param>
        public static void SetValidDefines( string[] defines ) {
            ProjectDefineSettings settings;
            string path;
            switch ( LoadSettingsInner( out path, out settings ) ) {
            case LoadState.Success:
                break;
            default:
                return;
            }

            string txt = "";
            foreach( var def in defines ) {
                txt += def + ";";
            }
            ApplyDefines( txt );
        }

    }
}
#endif //UNITY_EDITOR