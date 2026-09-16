using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using TreeView = UnityEditor.IMGUI.Controls.TreeView<int>;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;
using TreeViewState = UnityEditor.IMGUI.Controls.TreeViewState<int>;

namespace SGSys
{
    public class ProjectDefine_TreeView : TreeView
    {
        readonly List<ProjectDefine_TreeViewItem> mList;

        public ProjectDefine_TreeView( List<ProjectDefine_TreeViewItem> list, TreeViewState state, MultiColumnHeader header ) : base( state, header ) {
            mList = list;
            this.rowHeight = EditorGUIUtility.singleLineHeight * 1.5f;
            header.ResizeToFit();
            Reload();
        }

        protected override TreeViewItem BuildRoot () {
            var root = new TreeViewItem { id=0, depth=-1, displayName="Root" };
            return root;
        }
        protected override IList<TreeViewItem> BuildRows (TreeViewItem root) {
            var rows = GetRows() ?? new List<TreeViewItem>();
            rows.Clear();
            foreach ( var item in mList ) {
                root.AddChild( item );
                rows.Add( item );
            }
            SetupDepthsFromParentsAndChildren(root);
            return rows;
        }

        enum ColumnType {
            Blank,
            IsValid,
            Title,
            Comment,
            Default,
        }

        protected override void RowGUI (RowGUIArgs args) {
            var item = args.item as ProjectDefine_TreeViewItem;

            for ( int i=0; i<args.GetNumVisibleColumns(); ++i ) {
                var cellRect = args.GetCellRect(i);
                var columnIndex = (ColumnType)args.GetColumn(i);
                
                CenterRectUsingSingleLineHeight( ref cellRect );

                switch ( columnIndex ) {
                case ColumnType.IsValid:
                    item.isValid = EditorGUI.Toggle( cellRect, item.isValid );
                    break;
                case ColumnType.Title:
                    item.define = EditorGUI.TextField( cellRect, item.define );
                    break;
                case ColumnType.Comment:
                    item.comment = EditorGUI.TextField( cellRect, item.comment );
                    break;
                case ColumnType.Default:
                    item.initialValid = EditorGUI.Toggle( cellRect, item.initialValid );
                    break;
                }
            }
        }

        /// <summary>
        /// 複数列のTreeView作成
        /// </summary>
        /// <returns></returns>
        public static ProjectDefine_TreeView MakeTreeView( List<ProjectDefine_TreeViewItem> itemList ) {
            var columnNonTitle = new MultiColumnHeaderState.Column() {
                width = 18,
                minWidth = 18,
                maxWidth = 18,
                headerContent = new GUIContent( string.Empty ),
                headerTextAlignment = TextAlignment.Center,
                autoResize = false,
            };
            var columnTitle = new MultiColumnHeaderState.Column() {
                headerContent = new GUIContent("定義"),
                headerTextAlignment = TextAlignment.Center,

                sortedAscending = false,
                autoResize = true,
            };
            var columnComment = new MultiColumnHeaderState.Column() {
                headerContent = new GUIContent("コメント"),
                headerTextAlignment = TextAlignment.Center,

                autoResize = true,
                width = 100,
                minWidth = 30,
            };
            var columnInitial = new MultiColumnHeaderState.Column() {
                headerContent = new GUIContent("初期状態"),
                headerTextAlignment = TextAlignment.Center,

                autoResize = false,
            };

            var state = new MultiColumnHeaderState(
                new MultiColumnHeaderState.Column[] {
                    columnNonTitle, //ブランク
                    columnNonTitle, //ProjectDefine_TreeViewItem.isValid
                    columnTitle,    //ProjectDefine_TreeViewItem.title
                    columnComment,  //ProjectDefine_TreeViewItem.comment
    //              columnInitial,  //ProjectDefine_TreeViewItem.initialValid
                }
            );
            var header = new MultiColumnHeader(state) {
                canSort = false
            };

            var treeViewState = new TreeViewState();
            var treeView = new ProjectDefine_TreeView( itemList, treeViewState, header );
            return treeView;
        }
    }
}