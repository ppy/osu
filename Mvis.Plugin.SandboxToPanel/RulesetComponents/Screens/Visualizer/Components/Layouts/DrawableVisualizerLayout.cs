using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts
{
    public abstract partial class DrawableVisualizerLayout : CompositeDrawable
    {
        public DrawableVisualizerLayout()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
