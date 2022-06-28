// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using M.Resources.Localisation.LLin.Plugins;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class LyricFunctionProvider : FakeButton, IPluginFunctionProvider
    {
        public PluginSidebarPage SourcePage { get; set; }

        public LyricFunctionProvider(PluginSidebarPage page)
        {
            SourcePage = page;

            Icon = FontAwesome.Solid.Music;
            Description = CloudMusicStrings.EntryTooltip;
            Type = FunctionType.Plugin;
        }
    }
}
