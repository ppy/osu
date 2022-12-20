using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts.TypeA;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts
{
    public partial class TypeALayout : DrawableVisualizerLayout
    {
        private readonly Bindable<int> radius = new Bindable<int>(350);
        private readonly Bindable<string> colour = new Bindable<string>("#ffffff");
        private readonly Bindable<string> progressColour = new Bindable<string>("#ffffff");

        private TypeAVisualizerController visualizerController;
        private CircularBeatmapLogo logo;

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                visualizerController = new TypeAVisualizerController
                {
                    Position = new Vector2(0.5f),
                },
                logo = new CircularBeatmapLogo
                {
                    Position = new Vector2(0.5f),
                    Size = { BindTarget = radius }
                }
            };

            config?.BindWith(SandboxRulesetSetting.Radius, radius);
            config?.BindWith(SandboxRulesetSetting.TypeAColour, colour);
            config?.BindWith(SandboxRulesetSetting.TypeAProgressColour, progressColour);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            radius.BindValueChanged(r =>
            {
                visualizerController.Size = new Vector2(r.NewValue - 2);
            }, true);

            colour.BindValueChanged(c => visualizerController.Colour = Colour4.FromHex(c.NewValue), true);
            progressColour.BindValueChanged(c => logo.ProgressColour = Colour4.FromHex(c.NewValue), true);
        }
    }
}
