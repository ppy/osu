// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarChangelogButton : ToolbarOverlayToggleButton
    {
        public ToolbarChangelogButton()
        {
            SetIcon(FontAwesome.fa_list);
        }

        [BackgroundDependencyLoader(true)]
        private void load(ChangelogOverlay changelog)
        {
            StateContainer = changelog;
        }
    }
}
