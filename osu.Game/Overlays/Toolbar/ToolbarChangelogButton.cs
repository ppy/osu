// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public partial class ToolbarChangelogButton : ToolbarOverlayToggleButton
    {
        protected override Anchor TooltipAnchor => Anchor.TopRight;

        [BackgroundDependencyLoader(true)]
        private void load(ChangelogOverlay changelog)
        {
            StateContainer = changelog;
        }
    }
}
