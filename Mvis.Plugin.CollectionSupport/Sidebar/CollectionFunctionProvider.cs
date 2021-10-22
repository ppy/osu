using System;
using M.Resources.Localisation.LLin.Plugins;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class CollectionFunctionProvider : IPluginFunctionProvider
    {
        public Vector2 Size { get; set; } = new Vector2(30);
        public Action Action { get; set; }
        public IconUsage Icon { get; set; } = FontAwesome.Solid.Check;
        public LocalisableString Title { get; set; }
        public LocalisableString Description { get; set; } = CollectionStrings.EntryTooltip;
        public FunctionType Type { get; set; } = FunctionType.Plugin;

        public void Active() => Action?.Invoke();

        public PluginSidebarPage SourcePage { get; set; }

        public CollectionFunctionProvider(PluginSidebarPage page)
        {
            SourcePage = page;
        }
    }
}
