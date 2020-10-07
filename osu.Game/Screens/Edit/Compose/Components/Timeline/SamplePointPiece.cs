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
    public class SamplePointPiece : CompositeDrawable
    {
        private readonly SampleControlPoint samplePoint;

        private readonly Bindable<string> bank;
        private readonly BindableNumber<int> volume;

        private OsuSpriteText text;
        private Box volumeBox;

        public SamplePointPiece(SampleControlPoint samplePoint)
        {
            this.samplePoint = samplePoint;
            volume = samplePoint.SampleVolumeBindable.GetBoundCopy();
            bank = samplePoint.SampleBankBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Origin = Anchor.TopLeft;
            Anchor = Anchor.TopLeft;

            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Color4 colour = samplePoint.GetRepresentingColour(colours);

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 20,
                    Children = new Drawable[]
                    {
                        volumeBox = new Box
                        {
                            X = 2,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Colour = ColourInfo.GradientVertical(colour, Color4.Black),
                            RelativeSizeAxes = Axes.Both,
                        },
                        new Box
                        {
                            Colour = colour.Lighten(0.2f),
                            Width = 2,
                            RelativeSizeAxes = Axes.Y,
                        },
                    }
                },
                text = new OsuSpriteText
                {
                    X = 2,
                    Y = -5,
                    Anchor = Anchor.BottomLeft,
                    Alpha = 0.9f,
                    Rotation = -90,
                    Font = OsuFont.Default.With(weight: FontWeight.SemiBold)
                }
            };

            volume.BindValueChanged(volume => volumeBox.Height = volume.NewValue / 100f, true);
            bank.BindValueChanged(bank => text.Text = bank.NewValue, true);
        }
    }
}
