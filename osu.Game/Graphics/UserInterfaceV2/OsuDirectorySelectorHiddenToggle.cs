// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterfaceV2
{
    internal class OsuDirectorySelectorHiddenToggle : OsuCheckbox, IHasTooltip
    {
        public LocalisableString TooltipText => @"Show hidden items";

        public OsuDirectorySelectorHiddenToggle()
        {
            RelativeSizeAxes = Axes.None;
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Nub.AccentColour = colours.GreySeaFoamLighter;
            Nub.GlowingAccentColour = Color4.White;
            Nub.GlowColour = Color4.White;
        }
    }
}
