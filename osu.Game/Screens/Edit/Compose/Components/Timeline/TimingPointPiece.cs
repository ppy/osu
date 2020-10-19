// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK.Graphics;

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
            Origin = Anchor.CentreLeft;
            Anchor = Anchor.CentreLeft;

            AutoSizeAxes = Axes.Both;

            Color4 colour = point.GetRepresentingColour(colours);

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Alpha = 0.9f,
                    Colour = ColourInfo.GradientHorizontal(colour, colour.Opacity(0.5f)),
                    RelativeSizeAxes = Axes.Both,
                },
                bpmText = new OsuSpriteText
                {
                    Alpha = 0.9f,
                    Padding = new MarginPadding(3),
                    Font = OsuFont.Default.With(size: 40)
                }
            };

            beatLength.BindValueChanged(beatLength =>
            {
                bpmText.Text = $"{60000 / beatLength.NewValue:n1} BPM";
            }, true);
        }
    }
}
