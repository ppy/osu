using Mvis.Plugin.SandboxToPanel.RulesetComponents.Configuration;
using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components
{
    public partial class LayoutController : CompositeDrawable
    {
        private readonly Bindable<VisualizerLayout> layoutBinable = new Bindable<VisualizerLayout>();

        [BackgroundDependencyLoader]
        private void load(SandboxRulesetConfigManager config)
        {
            RelativeSizeAxes = Axes.Both;
            config?.BindWith(SandboxRulesetSetting.VisualizerLayout, layoutBinable);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            layoutBinable.BindValueChanged(_ => updateLayout(), true);
        }

        private void updateLayout()
        {
            DrawableVisualizerLayout l;

            switch(layoutBinable.Value)
            {
                default:
                case VisualizerLayout.TypeA:
                    l = new TypeALayout();
                    break;

                case VisualizerLayout.TypeB:
                    l = new TypeBLayout();
                    break;

                case VisualizerLayout.Empty:
                    l = new EmptyLayout();
                    break;
            }

            loadLayout(l);
        }

        private void loadLayout(DrawableVisualizerLayout layout)
        {
            InternalChild = layout;
        }
    }
}
