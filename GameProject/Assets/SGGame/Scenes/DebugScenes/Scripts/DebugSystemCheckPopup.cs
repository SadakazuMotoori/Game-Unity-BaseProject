using Cysharp.Threading.Tasks;
using SGGames.Game.Sys;

namespace SGGames.Game.Develop
{
    public sealed class DebugSystemCheckPopup : PopupWindow
    {
        public const string AssetAddress = "Debug/SystemCheckPopup";

        public override UniTask<bool> OnDecide(UISelectable selectable)
        {
            return UniTask.FromResult(false);
        }
    }
}
