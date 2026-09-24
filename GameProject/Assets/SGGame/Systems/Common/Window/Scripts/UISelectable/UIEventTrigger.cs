using UnityEngine.EventSystems;

namespace SGGames.Game.Sys
{
    //==================================================================
    /// <summary>
    /// EventTriggerを継承することで、親への伝播はなくなるよう(なんでやねん)
    /// ※結局、EventTriggerが階層になると伝播するのであまり意味ない…
    /// </summary>
    //==================================================================
    public class UIEventTrigger : EventTrigger, IPointerMoveHandler
    {
        public virtual void OnPointerMove(PointerEventData eventData)
        {
            // PointerMoveを受けるための受け皿。必要になった時だけここに処理を追加する。
    //        DebugLogger.Log($"OnPointerMove {name}", DebugLogger.Colors.orange);
        }
    }
}
