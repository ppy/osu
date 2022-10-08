// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TopPointPiece : CompositeDrawable
    {
        private readonly ControlPoint point;

        protected OsuSpriteText Label { get; private set; }

        public TopPointPiece(ControlPoint point)
        {
            this.point = point;
            AutoSizeAxes = Axes.X;
            Height = 16;
            Margin = new MarginPadding(4);

            Masking = true;
            CornerRadius = Height / 2;

            Origin = Anchor.TopCentre;
            Anchor = Anchor.TopCentre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = point.GetRepresentingColour(colours),
                    RelativeSizeAxes = Axes.Both,
                },
                Label = new OsuSpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Padding = new MarginPadding(3),
                    Font = OsuFont.Default.With(size: 14, weight: FontWeight.SemiBold),
                    Colour = colours.B5,
                }
            };
        }
    }
}
