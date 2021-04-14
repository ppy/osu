// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class TimingPointPiece : CompositeDrawable
    {
        private readonly TimingControlPoint point;

        private readonly BindableNumber<double> beatLength;
        private OsuSpriteText bpmText;

        public TimingPointPiece(TimingControlPoint point)
        {
            this.point = point;
            beatLength = point.BeatLengthBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Margin = new MarginPadding { Vertical = 10 };

            const float corner_radius = 5;

            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = corner_radius;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = point.GetRepresentingColour(colours),
                    RelativeSizeAxes = Axes.Both,
                },
                bpmText = new OsuSpriteText
                {
                    Alpha = 0.9f,
                    Padding = new MarginPadding { Vertical = 3, Horizontal = 6 },
                    Font = OsuFont.Default.With(size: 20, weight: FontWeight.SemiBold),
                    Colour = colours.B5,
                }
            };

            beatLength.BindValueChanged(beatLength =>
            {
                bpmText.Text = $"{60000 / beatLength.NewValue:n1} BPM";
            }, true);
        }
    }
}
