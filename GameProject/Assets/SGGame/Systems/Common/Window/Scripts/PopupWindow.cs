//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
/*!
 *    @file     PopupWindow.cs
 *    @brief    PopupWindow基底
 *
 *    @date     2026/05/01
 *    @author   Sadakazu Motoori
 */
//*****************************************************************************************************************
//*****************************************************************************************************************
//*****************************************************************************************************************
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using DG.Tweening;
using R3;
using R3.Triggers;
using UnityEngine;

namespace SGGames.Game.Sys
{
    //==========================================================================
    /**
     *    @brief       前面に重ねるタイプのWindow基底クラス.
     */
    //==========================================================================
    public abstract class PopupWindow : WindowBase
    {
        // UIのTopのTransform.
        [SerializeField] RectTransform _topUITransform;
        public RectTransform TopUITransform => _topUITransform;

        // 選択グループ.
        [SerializeField] UISelectableGroup _selectableGroup;
        public UISelectableGroup SelectableGroup => _selectableGroup;

        // 入力マップ名.
        [SerializeField] string _inputActionMap = "UI";
        public string InputActionMap => _inputActionMap;

        // 親PopupWindowへの参照.
        public PopupWindow PrevWindow { get; set; }

        //==========================================================================
        /**
         *    @brief       自分より前に積まれていた親Popupを閉じる.
         */
        //==========================================================================
        public async UniTask CloseTopOwnerWindow()
        {
            // 自分を開く前に積まれていた親Popupを、上から順番に閉じる.
            var wnd = PrevWindow;
            while(wnd != null)
            {
                await wnd.CloseWindow();

                wnd = wnd.PrevWindow;
            }
        }

        public override async UniTask OnInitialize()
        {
            await base.OnInitialize();

        }

        public override async UniTask OnShow()
        {
            await TopUITransform.DOLocalMoveY(500, 0.1f).From();
        }

        public override async UniTask OnClose()
        {
            await TopUITransform.DOLocalMoveY(-500, 0.1f).SetRelative();
        }

        //==========================================================================
        /**
         *    @brief       Run中の更新処理.
         */
        //==========================================================================
        public virtual async UniTask OnUpdate()
        {
            await UniTask.DelayFrame(1);
        }

        //==========================================================================
        /**
         *    @brief       選択項目の決定時に実行される.
         *    @param[in]   selectable 決定された選択項目.
         *    @return      falseの場合はWindowを閉じる.
         */
        //==========================================================================
        public virtual async UniTask<bool> OnDecide(UISelectable selectable)
        {
            // falseを返すとRun側でこのWindowを閉じ、選択結果を呼び出し元へ返す.
            await UniTask.DelayFrame(1);
            return true;
        }

        //==========================================================================
        /**
         *    @brief       選択処理を実行する.
         *    @return      押したボタンのID.
         */
        //==========================================================================
        public async UniTask<(int IDInt, string IDString)> Run()
        {
            // SelectableGroupを使い、カーソル移動・決定・Window終了までを1つのループで扱う.
            var cancelToken = this.GetCancellationTokenOnDestroy();

            // SelectableGroupの初期化完了を待つ.
            await _selectableGroup.WaitForInitialized();

            // Windowが破棄されるまで入力処理を続ける.
            while (cancelToken.IsCancellationRequested == false)
            {
                // カーソル入力を処理する.
                var retCursor = await _selectableGroup.UpdateCursor();

                // Window固有の更新処理を実行する.
                await OnUpdate();

                // 決定入力があれば選択結果を処理する.
                if (retCursor.action == UISelectable.Actions.Decide)
                {
                    // 決定処理がfalseを返した場合はWindowを閉じる.
                    if(await OnDecide(retCursor.select) == false)
                    {
                        // Windowを閉じる.
                        await CloseWindow();
                        // 選択結果を返す.
                        return (retCursor.select.IDInt, retCursor.select.IDString);
                    }
                }

                await UniTask.DelayFrame(1);
            }

            return (-1, "");
        }

    }
}
