// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Screens.RankingV2.Argon;
using osuTK;

namespace osu.Game.Screens.RankingV2.Extended
{
    public partial class StatisticValue : CompositeDrawable
    {
        public LocalisableString Caption { get; init; }

        private LocalisableString value;

        public LocalisableString Value
        {
            get => value;
            set
            {
                this.value = value;

                if (IsLoaded)
                    updateState();
            }
        }

        private LocalisableString maxValue;

        public LocalisableString MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;

                if (IsLoaded)
                    updateState();
            }
        }

        private LocalisableString difference;

        public LocalisableString Difference
        {
            get => difference;
            set
            {
                difference = value;

                if (IsLoaded)
                    updateState();
            }
        }

        private Kind differenceKind;

        public Kind DifferenceKind
        {
            get => differenceKind;
            set
            {
                differenceKind = value;

                if (IsLoaded)
                    updateState();
            }
        }

        public new Axes AutoSizeAxes
        {
            get => base.AutoSizeAxes;
            set => base.AutoSizeAxes = value;
        }

        public Colour4? AccentColour { get; init; }

        private OsuSpriteText valueSpriteText = null!;
        private OsuSpriteText maxValueSpriteText = null!;
        private OsuSpriteText differenceSpriteText = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            InternalChildren =
            [
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(-2),
                    Children =
                    [
                        new OsuSpriteText
                        {
                            Colour = AccentColour ?? colourProvider.Content2,
                            Text = Caption.ToUpper(),
                            Font = OsuFont.Style.Body.With(weight: FontWeight.Bold),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Children =
                            [
                                valueSpriteText = new OsuSpriteText
                                {
                                    Colour = ArgonTotalScorePanel.TEXT_GRADIENT,
                                    Font = OsuFont.Default.With(size: 24),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                },
                                maxValueSpriteText = new OsuSpriteText
                                {
                                    Colour = ArgonTotalScorePanel.TEXT_GRADIENT,
                                    Font = OsuFont.Default.With(size: 18),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Margin = new MarginPadding { Bottom = 1, },
                                },
                            ]
                        },
                        differenceSpriteText = new OsuSpriteText
                        {
                            Font = OsuFont.Style.Caption1,
                        },
                    ],
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            updateState();
        }

        private void updateState()
        {
            valueSpriteText.Text = Value;

            maxValueSpriteText.Alpha = MaxValue != default ? 1 : 0;
            maxValueSpriteText.Text = LocalisableString.Interpolate($@" / {MaxValue}");

            differenceSpriteText.Text = Difference;

            switch (differenceKind)
            {
                case Kind.Gain:
                    differenceSpriteText.Colour = colours.Green1;
                    break;

                case Kind.Loss:
                    differenceSpriteText.Colour = colours.Red1;
                    break;

                default:
                    differenceSpriteText.Colour = colourProvider.Light2;
                    break;
            }
        }

        public enum Kind
        {
            NoChange,
            Gain,
            Loss,
        }
    }
}
