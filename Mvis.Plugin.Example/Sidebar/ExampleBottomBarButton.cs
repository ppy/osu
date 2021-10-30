using System;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.Example.Sidebar
{
    public class ExampleBottomBarButton : IPluginFunctionProvider
    {
        public Vector2 Size { get; set; }
        public Action Action { get; set; }
        public IconUsage Icon { get; set; } = FontAwesome.Solid.Egg;
        public LocalisableString Title { get; set; }
        public LocalisableString Description { get; set; } = "hi";
        public FunctionType Type { get; set; }

        public void Active()
        {
            Action?.Invoke();
        }

        public PluginSidebarPage SourcePage { get; set; }
    }
}
