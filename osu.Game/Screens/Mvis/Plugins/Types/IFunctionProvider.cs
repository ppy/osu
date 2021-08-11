using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osuTK;

namespace osu.Game.Screens.Mvis.Plugins.Types
{
    public interface IFunctionProvider
    {
        public Vector2 Size { get; set; }
        public Action Action { get; set; }

        public IconUsage Icon { get; set; }

        public LocalisableString Title { get; set; }

        public LocalisableString Description { get; set; }

        public FunctionType Type { get; set; }

        public void Active();

        public string ToString() => $"{Title} - {Description}";
    }

    public interface IToggleableFunctionProvider : IFunctionProvider
    {
        public BindableBool Bindable { get; set; }
    }

    public interface IPluginFunctionProvider : IFunctionProvider
    {
        public PluginSidebarPage SourcePage { get; set; }
    }

    public enum FunctionType
    {
        Base,
        Audio,
        Plugin,
        Misc,
        ProgressDisplay
    }
}
