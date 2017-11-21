// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarDirectButton : ToolbarOverlayToggleButton
    {
        public ToolbarDirectButton()
        {
            SetIcon(FontAwesome.fa_osu_chevron_down_o);
        }

        [BackgroundDependencyLoader]
        private void load(DirectOverlay direct)
        {
            StateContainer = direct;
        }
    }
}
