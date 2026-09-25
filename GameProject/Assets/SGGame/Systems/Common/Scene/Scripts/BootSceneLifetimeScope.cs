using MackySoft.Navigathena.SceneManagement.VContainer;
using VContainer;
using VContainer.Unity;

namespace SGGames.Game.Sys
{
    public sealed class BootSceneLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(GetComponentInParent<BootSceneEntryPoint>());
            builder.RegisterSceneLifecycle<BootSceneLifecycle>();
        }
    }
}
