// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Difficulty;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapTitleWedge
    {
        public partial class StatisticDifficulty : CompositeDrawable, IHasAccentColour, IHasCustomTooltip<RulesetBeatmapAttribute?>
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
            private readonly Container bars;

            public Color4 AccentColour
            {
                get => bar.Colour;
                set => bar.Colour = value;
            }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public StatisticDifficulty()
            {
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        bars = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new[]
                            {
                                new Circle
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = 2f,
                                    Colour = Color4.Black,
                                    Masking = true,
                                    CornerRadius = 1f,
                                    Depth = float.MaxValue,
                                },
                                bar = new Circle
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0f,
                                    Height = 2f,
                                    Masking = true,
                                    CornerRadius = 1f,
                                },
                                adjustedBar = new Circle
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0f,
                                    Height = 2f,
                                    Masking = true,
                                    CornerRadius = 1f,
                                },
                            },
                        },
                        labelText = new TruncatingSpriteText
                        {
                            Margin = new MarginPadding { Top = 2f },
                            Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                            MaxWidth = 85,
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
                                    Font = OsuFont.Style.Body,
                                },
                                valueIcon = new SpriteIcon
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Margin = new MarginPadding
                                    {
                                        Top = -4f,
                                        Left = 2,
                                    },
                                    Size = new Vector2(8),
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
                valueText.Text = value.Content ?? value.AdjustedValue.ToLocalisableString("0.##");

                if (value.Value == value.AdjustedValue)
                {
                    adjustedBar.FadeColour(Color4.Transparent, 300, Easing.OutQuint);
                    bar.FadeIn(300, Easing.OutQuint);

                    valueText.FadeColour(Color4.White, 300, Easing.OutQuint);
                    valueIcon.Hide();
                }
                else
                {
                    bool difficultyIncrease = value.Value < value.AdjustedValue;

                    if (difficultyIncrease)
                    {
                        bars.ChangeChildDepth(adjustedBar, 1);
                        bar.FadeIn(300, Easing.OutQuint);
                        adjustedBar.FadeColour(ColourInfo.GradientHorizontal(Color4.Black, colours.Red1), 300, Easing.OutQuint);

                        valueText.FadeColour(colours.Red1, 300, Easing.OutQuint);
                        valueIcon.Show();
                        valueIcon.Colour = colours.Red1;
                        valueIcon.Icon = FontAwesome.Solid.SortUp;
                    }
                    else
                    {
                        bar.FadeTo(0.5f, 300, Easing.OutQuint);
                        bars.ChangeChildDepth(adjustedBar, -1);
                        adjustedBar.FadeColour(colours.Lime1, 300, Easing.OutQuint);

                        valueText.FadeColour(colours.Lime1, 300, Easing.OutQuint);
                        valueIcon.Show();
                        valueIcon.Colour = colours.Lime1;
                        valueIcon.Icon = FontAwesome.Solid.SortDown;
                    }
                }
            }

            public record Data(LocalisableString Label, float Value, float AdjustedValue, float Maximum, string? Content = null, RulesetBeatmapAttribute? BeatmapAttribute = null)
            {
                public Data(RulesetBeatmapAttribute attribute)
                    : this(attribute.Label, attribute.OriginalValue, attribute.AdjustedValue, attribute.MaxValue, BeatmapAttribute: attribute)
                {
                }
            }

            public ITooltip<RulesetBeatmapAttribute?> GetCustomTooltip() => new BeatmapAttributeTooltip();
            public RulesetBeatmapAttribute? TooltipContent => value.BeatmapAttribute;
        }
    }
}
