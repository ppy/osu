// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Input.Bindings;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarMusicButton : ToolbarOverlayToggleButtonRightSide
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        public ToolbarMusicButton()
        {
            Icon = FontAwesome.Solid.Music;
            TooltipMain = "音乐";
            TooltipSub = "在这里播放音乐";

            Hotkey = GlobalAction.ToggleNowPlaying;
        }

        [BackgroundDependencyLoader(true)]
        private void load(NowPlayingOverlay music)
        {
            StateContainer = music;
        }
    }
}
