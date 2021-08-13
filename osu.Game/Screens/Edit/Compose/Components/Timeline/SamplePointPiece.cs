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
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class SamplePointPiece : CompositeDrawable
    {
        private readonly SampleControlPoint samplePoint;

        private readonly Bindable<string> bank;
        private readonly BindableNumber<int> volume;

        private OsuSpriteText text;
        private Container volumeBox;

        private const int max_volume_height = 22;

        public SamplePointPiece(SampleControlPoint samplePoint)
        {
            this.samplePoint = samplePoint;
            volume = samplePoint.SampleVolumeBindable.GetBoundCopy();
            bank = samplePoint.SampleBankBindable.GetBoundCopy();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Margin = new MarginPadding { Vertical = 5 };

            Origin = Anchor.BottomCentre;
            Anchor = Anchor.BottomCentre;

            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            Color4 colour = samplePoint.GetRepresentingColour(colours);

            InternalChildren = new Drawable[]
            {
                volumeBox = new Circle
                {
                    CornerRadius = 5,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Y = -20,
                    Width = 10,
                    Colour = colour,
                },
                new Container
                {
                    AutoSizeAxes = Axes.X,
                    Height = 16,
                    Masking = true,
                    CornerRadius = 8,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colour,
                            RelativeSizeAxes = Axes.Both,
                        },
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Padding = new MarginPadding(5),
                            Font = OsuFont.Default.With(size: 12, weight: FontWeight.SemiBold),
                            Colour = colours.B5,
                        }
                    }
                },
            };

            volume.BindValueChanged(volume => volumeBox.Height = max_volume_height * volume.NewValue / 100f, true);
            bank.BindValueChanged(bank => text.Text = bank.NewValue, true);
        }
    }
}
