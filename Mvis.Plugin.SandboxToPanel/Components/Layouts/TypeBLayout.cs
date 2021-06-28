using Mvis.Plugin.Sandbox.Components.Layouts.TypeB;

namespace Mvis.Plugin.Sandbox.Components.Layouts
{
    public class TypeBLayout : DrawableVisualizerLayout
    {
        public TypeBLayout()
        {
            AddInternal(new TypeBVisualizerController());
        }
    }
}
