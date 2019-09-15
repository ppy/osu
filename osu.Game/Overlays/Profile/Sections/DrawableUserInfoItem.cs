// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using System.Collections.Generic;

namespace osu.Game.Overlays.Profile.Sections
{
    public class DrawableUserInfoItem : OsuHoverContainer
    {
        private const int corner_radius = 6;

        private readonly Box background;

        protected override IEnumerable<Drawable> EffectTargets => new[] { background };

        public DrawableUserInfoItem()
        {
            Enabled.Value = true; //manually enabled, because we have no action

            RelativeSizeAxes = Axes.X;
            Masking = true;
            CornerRadius = corner_radius;

            Add(background = new Box
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            IdleColour = colours.GreySeafoam;
            HoverColour = colours.GreySeafoamLight;
        }
    }
}
