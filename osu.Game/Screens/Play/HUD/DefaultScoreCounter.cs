// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultScoreCounter : GameplayScoreCounter, ISkinnableDrawable
    {
        public DefaultScoreCounter()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
        }

        [Resolved(canBeNull: true)]
        private HUDOverlay hud { get; set; }

        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }
    }
}
