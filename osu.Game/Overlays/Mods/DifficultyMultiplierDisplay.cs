// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public class DifficultyMultiplierDisplay : CompositeDrawable, IHasCurrentValue<double>
    {
        public const float HEIGHT = 42;

        public Bindable<double> Current
        {
            get => current.Current;
            set => current.Current = value;
        }

        private readonly BindableNumberWithCurrent<double> current = new BindableNumberWithCurrent<double>(1)
        {
            Precision = 0.01
        };

        private readonly Box underlayBackground;
        private readonly Box contentBackground;
        private readonly FillFlowContainer multiplierFlow;
        private readonly MultiplierCounter multiplierCounter;

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; }

        private const float multiplier_value_area_width = 56;
        private const float transition_duration = 200;

        public DifficultyMultiplierDisplay()
        {
            Height = HEIGHT;
            AutoSizeAxes = Axes.X;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = ModPanel.CORNER_RADIUS,
                Shear = new Vector2(ModPanel.SHEAR_X, 0),
                Children = new Drawable[]
                {
                    underlayBackground = new Box
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = multiplier_value_area_width + ModPanel.CORNER_RADIUS
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, multiplier_value_area_width)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Masking = true,
                                    CornerRadius = ModPanel.CORNER_RADIUS,
                                    Children = new Drawable[]
                                    {
                                        contentBackground = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Margin = new MarginPadding { Horizontal = 18 },
                                            Shear = new Vector2(-ModPanel.SHEAR_X, 0),
                                            Text = "Difficulty Multiplier",
                                            Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
                                        }
                                    }
                                },
                                multiplierFlow = new FillFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shear = new Vector2(-ModPanel.SHEAR_X, 0),
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(2, 0),
                                    Children = new Drawable[]
                                    {
                                        multiplierCounter = new MultiplierCounter
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Current = { BindTarget = Current }
                                        },
                                        new SpriteIcon
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Icon = FontAwesome.Solid.Times,
                                            Size = new Vector2(7),
                                            Margin = new MarginPadding { Top = 1 }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            contentBackground.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            current.BindValueChanged(_ => updateState(), true);

            // required to prevent the counter initially rolling up from 0 to 1
            // due to `Current.Value` having a nonstandard default value of 1.
            multiplierCounter.SetCountWithoutRolling(Current.Value);
        }

        private void updateState()
        {
            if (Current.IsDefault)
            {
                underlayBackground.FadeColour(colourProvider.Background3, transition_duration, Easing.OutQuint);
                multiplierFlow.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
            }
            else
            {
                var backgroundColour = Current.Value < 1
                    ? colours.ForModType(ModType.DifficultyReduction)
                    : colours.ForModType(ModType.DifficultyIncrease);

                underlayBackground.FadeColour(backgroundColour, transition_duration, Easing.OutQuint);
                multiplierFlow.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
            }
        }

        private class MultiplierCounter : RollingCounter<double>
        {
            protected override double RollingDuration => 500;

            protected override LocalisableString FormatCount(double count) => count.ToLocalisableString(@"N2");

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
            };
        }
    }
}
