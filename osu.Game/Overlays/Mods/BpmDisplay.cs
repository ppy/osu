// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public sealed class BpmDisplay : CompositeDrawable
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> working { get; set; } = null!;

        private readonly BindableNumber<int> current = new BindableNumber<int>();

        private ModSettingChangeTracker? settingChangeTracker;
        private const float bpm_value_area_width = 56;
        private const float transition_duration = 200;
        private readonly Box underlayBackground;
        private readonly Box contentBackground;
        private readonly BpmCounter counter;
        private readonly OsuSpriteText dashText;

        public BpmDisplay()
        {
            Height = DifficultyMultiplierDisplay.HEIGHT;
            AutoSizeAxes = Axes.X;

            InternalChild = new InputBlockingContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = ModSelectPanel.CORNER_RADIUS,
                Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0),
                Children = new Drawable[]
                {
                    underlayBackground = new Box
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = bpm_value_area_width + ModSelectPanel.CORNER_RADIUS
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, bpm_value_area_width)
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
                                    CornerRadius = ModSelectPanel.CORNER_RADIUS,
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
                                            Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                            Text = "Average BPM",
                                            Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    AutoSizeAxes = Axes.Both,
                                    Children = new Drawable[]
                                    {
                                        counter = new BpmCounter
                                        {
                                            Current = { BindTarget = current }
                                        },
                                        dashText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold),
                                            Text = "-"
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
            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                refresh();

                settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                settingChangeTracker.SettingChanged += _ => refresh();
            }, true);
            working.ValueChanged += _ => refresh();
        }

        /// <summary>
        /// Refreshes counter and background color.
        /// </summary>
        private void refresh()
        {
            var beatmap = working.Value.Beatmap;

            if (beatmap == null)
                return;

            double rate = 1;
            foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                rate = mod.ApplyToRate(0, rate);

            double mostCommonBPM = Math.Round(Math.Round(60000 / beatmap.GetMostCommonBeatLength()) * rate);

            if (double.IsNormal(mostCommonBPM))
            {
                counter.FadeIn(transition_duration, Easing.OutQuint);
                dashText.FadeOut(transition_duration, Easing.OutQuint);
            }
            else
            {
                // when no map is selected, common length will be zero, producing infinity.
                mostCommonBPM = 0d;
                counter.FadeOut(transition_duration, Easing.OutQuint);
                dashText.FadeIn(transition_duration, Easing.OutQuint);
            }

            current.Value = (int)mostCommonBPM;

            if (mostCommonBPM == 0d || Precision.AlmostEquals(rate, 1d))
            {
                underlayBackground.FadeColour(colourProvider.Background3, transition_duration, Easing.OutQuint);
                counter.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
            }
            else
            {
                var backgroundColour = rate < 1
                    ? colours.ForModType(ModType.DifficultyReduction)
                    : colours.ForModType(ModType.DifficultyIncrease);

                underlayBackground.FadeColour(backgroundColour, transition_duration, Easing.OutQuint);
                counter.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
            }
        }

        private class BpmCounter : RollingCounter<int>
        {
            protected override double RollingDuration => 500;

            protected override OsuSpriteText CreateSpriteText() => new OsuSpriteText
            {
                Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
            };
        }
    }
}
