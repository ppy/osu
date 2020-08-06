// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        public ToolbarMusicButton()
        {
            Icon = FontAwesome.Solid.Music;
            TooltipMain = "Now playing";
            TooltipSub = "Manage the currently playing track";

            Hotkey = GlobalAction.ToggleNowPlaying;
        }

        [BackgroundDependencyLoader(true)]
        private void load(NowPlayingOverlay music)
        {
            StateContainer = music;
        }
    }
}
