// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play.HUD
{
    public class DefaultAccuracyCounter : GameplayAccuracyCounter, ISkinnableDrawable
    {
        public bool UsesFixedAnchor { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.BlueLighter;
        }
    }
}
