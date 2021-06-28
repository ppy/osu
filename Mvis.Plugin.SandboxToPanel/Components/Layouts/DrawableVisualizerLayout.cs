using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace Mvis.Plugin.RulesetPanel.Components.Layouts
{
    public abstract class DrawableVisualizerLayout : CompositeDrawable
    {
        protected DrawableVisualizerLayout()
        {
            RelativeSizeAxes = Axes.Both;
        }
    }
}
