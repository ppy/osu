using Mvis.Plugin.RulesetPanel.Components.Layouts;
using Mvis.Plugin.Sandbox.Config;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.RulesetPanel.Components
{
    public class LayoutController : CompositeDrawable
    {
        private readonly Bindable<VisualizerLayout> layoutBinable = new Bindable<VisualizerLayout>();

        [BackgroundDependencyLoader]
        private void load(SandboxConfigManager config)
        {
            RelativeSizeAxes = Axes.Both;
            config?.BindWith(SandboxSetting.VisualizerLayout, layoutBinable);
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
