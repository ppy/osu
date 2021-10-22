// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using M.Resources.Localisation.LLin.Plugins;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class LyricFunctionProvider : IPluginFunctionProvider
    {
        public Vector2 Size { get; set; } = new Vector2(30);
        public Action Action { get; set; }
        public IconUsage Icon { get; set; } = FontAwesome.Solid.Music;
        public LocalisableString Title { get; set; }
        public LocalisableString Description { get; set; } = CloudMusicStrings.EntryTooltip;
        public void Active() => Action?.Invoke();
        public FunctionType Type { get; set; } = FunctionType.Plugin;

        public PluginSidebarPage SourcePage { get; set; }

        public LyricFunctionProvider(PluginSidebarPage page)
        {
            SourcePage = page;
        }
    }
}
