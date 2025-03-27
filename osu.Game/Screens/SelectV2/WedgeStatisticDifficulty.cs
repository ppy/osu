// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class WedgeStatisticDifficulty : CompositeDrawable, IHasAccentColour
    {
        public LocalisableString Label
        {
            get => labelText.Text;
            set => labelText.Text = value;
        }

        public float LabelWidth => labelText.DrawWidth;

        private (float value, float maximum) value;

        public (float value, float maximum) Value
        {
            get => value;
            set
            {
                this.value = value;

                bar.ResizeWidthTo(value.maximum == 0 ? 0 : value.value / value.maximum, 300, Easing.OutQuint);
                valueText.Text = value.value.ToLocalisableString("0.##");
            }
        }

        private readonly Circle bar;
        private readonly OsuSpriteText labelText;
        private readonly OsuSpriteText valueText;

        public Color4 AccentColour
        {
            get => bar.Colour;
            set => bar.Colour = value;
        }

        public WedgeStatisticDifficulty(LocalisableString label)
        {
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            new Circle
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 2.5f,
                                Colour = Color4.Black,
                                Masking = true,
                                CornerRadius = 1f,
                            },
                            bar = new Circle
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0f,
                                Height = 2.5f,
                                Masking = true,
                                CornerRadius = 1f,
                            },
                        },
                    },
                    labelText = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 2f },
                        Text = label,
                        Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold),
                    },
                    valueText = new OsuSpriteText
                    {
                        Font = OsuFont.Torus.With(size: 20f, weight: FontWeight.Regular),
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            labelText.Colour = colourProvider.Content2;
            valueText.Colour = colourProvider.Content1;
        }
    }
}
