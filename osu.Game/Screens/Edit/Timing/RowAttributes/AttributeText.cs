// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Timing.RowAttributes
{
    public partial class AttributeText : OsuSpriteText
    {
        private readonly ControlPoint controlPoint;

        public AttributeText(ControlPoint controlPoint)
        {
            this.controlPoint = controlPoint;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            Padding = new MarginPadding(6);
            Font = OsuFont.Default.With(weight: FontWeight.Bold, size: 12);
            Colour = controlPoint.GetRepresentingColour(colours);
        }
    }
}
