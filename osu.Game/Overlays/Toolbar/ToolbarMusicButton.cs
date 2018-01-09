// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarMusicButton : ToolbarOverlayToggleButton
    {
        public ToolbarMusicButton()
        {
            Icon = FontAwesome.fa_music;
        }

        [BackgroundDependencyLoader(true)]
        private void load(MusicController music)
        {
            StateContainer = music;
        }
    }
}
