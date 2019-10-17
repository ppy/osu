// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Profile.Sections
{
    public class ProfileShowMoreButton : ShowMoreButton
    {
        [BackgroundDependencyLoader]
        private void load(OsuColour colors)
        {
            IdleColour = colors.GreySeafoamDark;
            HoverColour = colors.GreySeafoam;
            ChevronIconColour = colors.Yellow;
        }
    }
}
