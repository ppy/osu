using Mvis.Plugin.RulesetPanel.Components.Layouts.TypeB;

namespace Mvis.Plugin.RulesetPanel.Components.Layouts
{
    public class TypeBLayout : DrawableVisualizerLayout
    {
        public TypeBLayout()
        {
            AddInternal(new TypeBVisualizerController());
        }
    }
}
