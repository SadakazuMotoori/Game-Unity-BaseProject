using System.Collections.Generic;
using TreeViewItem = UnityEditor.IMGUI.Controls.TreeViewItem<int>;

[assembly: UnityEngine.MakeSerializable(typeof(TreeViewItem))]

namespace SGSys
{
    [System.Serializable]
    public class ProjectDefine_TreeViewItem : TreeViewItem
    {
        /// <summary>
        /// 現在の定義有効状態
        /// </summary>
        public bool     isValid;
        /// <summary>
        /// 定義
        /// </summary>
        public string   define;
        /// <summary>
        /// この定義のコメント
        /// </summary>
        public string   comment;
        /// <summary>
        /// isValidの初期状態
        /// </summary>
        public bool     initialValid;


        public ProjectDefine_TreeViewItem( int id, ProjectDefine_TreeViewItem item ) : base(id) {
            Set( item );
        }
        /// <summary>
        /// 新規追加用
        /// </summary>
        /// <param name="id"></param>
        public ProjectDefine_TreeViewItem( int id ) : base(id) {
            this.isValid = false;
            this.define = "NEW_DEFINE_"+id;
            this.comment = "※※ 追加した定義のコメントを記述してください ※※";
            this.initialValid = false;
        }

        public void Set( ProjectDefine_TreeViewItem item ) {
            this.isValid = item.isValid;
            this.define = item.define;
            this.comment = item.comment;
            this.initialValid = item.initialValid;
        }


        public static List<ProjectDefine_TreeViewItem> MakeItemList( ProjectDefineSettings settings ) {
            List<ProjectDefine_TreeViewItem> list = new List<ProjectDefine_TreeViewItem>();
            int id=1;
            foreach( var item in settings.items ) {
                var listItem = new ProjectDefine_TreeViewItem( ++id, item );
                list.Add( listItem );
            }
            return list;
        }
    }
}
