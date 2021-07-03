using Mvis.Plugin.Sandbox.Components.Layouts.TypeA;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace Mvis.Plugin.Sandbox.Components.Layouts
{
    public class TypeALayout : DrawableVisualizerLayout
    {
        private readonly Bindable<int> radius = new Bindable<int>(350);

        private TypeAVisualizerController visualizerController;

        [BackgroundDependencyLoader]
        private void load(SandboxConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                visualizerController = new TypeAVisualizerController
                {
                    Position = new Vector2(0.5f),
                },
                new CircularBeatmapLogo
                {
                    Position = new Vector2(0.5f),
                    Size = { BindTarget = radius }
                }
            };

            config?.BindWith(SandboxSetting.Radius, radius);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            radius.BindValueChanged(r =>
            {
                visualizerController.Size = new Vector2(r.NewValue - 2);
            }, true);
        }
    }
}
