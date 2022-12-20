using Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts.TypeB;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.Visualizer.Components.Layouts
{
    public partial class TypeBLayout : DrawableVisualizerLayout
    {
        public TypeBLayout()
        {
            AddInternal(new TypeBVisualizerController());
        }
    }
}
