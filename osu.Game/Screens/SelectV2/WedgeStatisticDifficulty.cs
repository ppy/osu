// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class WedgeStatisticDifficulty : CompositeDrawable, IHasAccentColour
    {
        private Data value = new Data(string.Empty, 0, 0, 0);

        public Data Value
        {
            get => value;
            set
            {
                this.value = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        public float LabelWidth => labelText.DrawWidth;

        private readonly Circle bar;
        private readonly Circle adjustedBar;
        private readonly OsuSpriteText labelText;
        private readonly OsuSpriteText valueText;
        private readonly SpriteIcon valueIcon;

        public Color4 AccentColour
        {
            get => bar.Colour;
            set => bar.Colour = value;
        }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public WedgeStatisticDifficulty()
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
                            adjustedBar = new Circle
                            {
                                RelativeSizeAxes = Axes.X,
                                Width = 0f,
                                Height = 2.5f,
                                Masking = true,
                                CornerRadius = 1f,
                                Alpha = 0.5f,
                            },
                        },
                    },
                    labelText = new OsuSpriteText
                    {
                        Margin = new MarginPadding { Top = 2f },
                        Font = OsuFont.Torus.With(size: 12f, weight: FontWeight.SemiBold),
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Children = new Drawable[]
                        {
                            valueText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = OsuFont.Torus.With(size: 20f, weight: FontWeight.Regular),
                            },
                            valueIcon = new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Top = 2f },
                                Size = new Vector2(12),
                            }
                        },
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            bar.ResizeWidthTo(value.Maximum == 0 ? 0 : Math.Clamp(value.Value / value.Maximum, 0, 1), 300, Easing.OutQuint);
            adjustedBar.ResizeWidthTo(value.Maximum == 0 ? 0 : Math.Clamp(value.AdjustedValue / value.Maximum, 0, 1), 300, Easing.OutQuint);

            labelText.Text = value.Label;
            valueText.Text = value.AdjustedValue.ToLocalisableString("0.##");

            if (value.Value == value.AdjustedValue)
            {
                adjustedBar.FadeColour(Color4.Transparent, 300, Easing.OutQuint);

                valueText.FadeColour(Color4.White, 300, Easing.OutQuint);
                valueIcon.Hide();
            }
            else
            {
                if (value.Value > value.AdjustedValue)
                {
                    adjustedBar.FadeColour(colours.Blue1, 300, Easing.OutQuint);
                    valueText.FadeColour(colours.Blue1, 300, Easing.OutQuint);
                    valueIcon.Show();
                    valueIcon.Colour = colours.Blue1;
                    valueIcon.Icon = FontAwesome.Solid.ArrowDown;
                }
                else
                {
                    adjustedBar.FadeColour(colours.Red1, 300, Easing.OutQuint);
                    valueText.FadeColour(colours.Red1, 300, Easing.OutQuint);
                    valueIcon.Show();
                    valueIcon.Colour = colours.Red1;
                    valueIcon.Icon = FontAwesome.Solid.ArrowUp;
                }
            }
        }

        public record Data(LocalisableString Label, float Value, float AdjustedValue, float Maximum);
    }
}
