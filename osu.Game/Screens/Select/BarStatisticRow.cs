// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mods;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class BarStatisticRow : GridContainer, IHasAccentColour
    {
        private const float name_width = 70;
        private const float value_width = 30;

        private readonly float maxValue;
        private readonly bool forceDecimalPlaces;
        private readonly OsuTextFlowContainer name;
        private readonly OsuSpriteText valueText;
        private readonly Bar bar;
        private readonly Bar modBar;

        public const float HORIZONTAL_PADDING = 5;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private LocalisableString title;

        public LocalisableString Title
        {
            get => title;
            set
            {
                title = value;
                name.Text = value;
            }
        }

        private (float baseValue, float? adjustedValue)? value;
        private readonly Bar barBackground;

        public (float baseValue, float? adjustedValue) Value
        {
            get => value ?? (0, null);
            set
            {
                if (value == this.value)
                    return;

                this.value = value;

                bar.Length = value.baseValue / maxValue;

                valueText.Text = (value.adjustedValue ?? value.baseValue).ToString(forceDecimalPlaces ? "0.00" : "0.##");
                modBar.Length = (value.adjustedValue ?? 0) / maxValue;

                if (Precision.AlmostEquals(value.baseValue, value.adjustedValue ?? value.baseValue, 0.05f))
                {
                    bar.Alpha = 1;
                    modBar.Alpha = 0;
                    valueText.Colour = Color4.White;
                }
                else if (value.adjustedValue > value.baseValue)
                {
                    bar.Alpha = 0.75f;
                    modBar.Alpha = 0.5f;
                    modBar.AccentColour = valueText.Colour = colours.ForModType(ModType.DifficultyIncrease);
                }
                else if (value.adjustedValue < value.baseValue)
                {
                    bar.Alpha = 0.25f;
                    modBar.Alpha = 1;

                    // can't be `colours.ForModType(ModType.DifficultyReduction)` as it's green and the unchanged bar colour also is another green by default (i.e. with `OverlayColourScheme.Aquamarine`)...
                    modBar.AccentColour = valueText.Colour = colours.BlueDark;
                }
            }
        }

        public Color4 AccentColour
        {
            get => bar.AccentColour;
            set => Schedule(() => bar.AccentColour = value);
        }

        public BarStatisticRow(float maxValue = 10, bool forceDecimalPlaces = false)
        {
            this.maxValue = maxValue;
            this.forceDecimalPlaces = forceDecimalPlaces;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Width = 1 / 3f;
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;
            Padding = new MarginPadding { Horizontal = HORIZONTAL_PADDING };

            ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Absolute, name_width),
                new Dimension(GridSizeMode.Absolute, value_width),
            };
            RowDimensions = new[]
            {
                new Dimension(GridSizeMode.AutoSize),
            };

            Content = new[]
            {
                new Drawable[]
                {
                    name = new OsuTextFlowContainer(t => t.Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 12))
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Text = title,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Child = valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 12),
                        },
                    },
                    new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new Drawable[]
                        {
                            barBackground = new Bar
                            {
                                CornerRadius = 2,
                                RelativeSizeAxes = Axes.X,
                                Height = 4,
                            },
                            bar = new Bar
                            {
                                CornerRadius = 2,
                                RelativeSizeAxes = Axes.X,
                                Height = 4,
                            },
                            modBar = new Bar
                            {
                                CornerRadius = 2,
                                Alpha = 0.5f,
                                RelativeSizeAxes = Axes.X,
                                Height = 4,
                            },
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            barBackground.BackgroundColour = colourProvider.Background6;
            bar.AccentColour = colourProvider.Highlight1;
        }
    }
}
