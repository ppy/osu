// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Overlays.Mods;

namespace osu.Game.Screens.Select.Details
{
    public partial class AdvancedStats : Container, IHasCustomTooltip<AdjustedAttributesTooltip.Data>
    {
        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        protected readonly StatisticRow FirstValue, HpDrain, Accuracy, ApproachRate;
        private readonly StatisticRow starDifficulty;

        public ITooltip<AdjustedAttributesTooltip.Data> GetCustomTooltip() => new AdjustedAttributesTooltip();
        public AdjustedAttributesTooltip.Data TooltipContent { get; private set; }

        private IBeatmapInfo beatmapInfo;

        public IBeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (value == beatmapInfo) return;

                beatmapInfo = value;

                updateStatistics();
            }
        }

        /// <summary>
        /// Ruleset to be used for certain elements of display.
        /// When set, this will override the set <see cref="Beatmap"/>'s own ruleset.
        /// </summary>
        /// <remarks>
        /// No checks are done as to whether the ruleset specified is valid for the currently <see cref="BeatmapInfo"/>.
        /// </remarks>
        public Bindable<RulesetInfo> Ruleset { get; } = new Bindable<RulesetInfo>();

        public AdvancedStats(int columns = 1)
        {
            switch (columns)
            {
                case 1:
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            FirstValue = new StatisticRow(), // circle size/key amount
                            HpDrain = new StatisticRow { Title = BeatmapsetsStrings.ShowStatsDrain },
                            Accuracy = new StatisticRow { Title = BeatmapsetsStrings.ShowStatsAccuracy },
                            ApproachRate = new StatisticRow { Title = BeatmapsetsStrings.ShowStatsAr },
                            starDifficulty = new StatisticRow(10, true) { Title = BeatmapsetsStrings.ShowStatsStars },
                        },
                    };
                    break;

                case 2:
                    Child = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Children = new[]
                        {
                            FirstValue = new StatisticRow
                            {
                                Width = 0.5f,
                                Padding = new MarginPadding { Right = 5, Vertical = 2.5f },
                            }, // circle size/key amount
                            HpDrain = new StatisticRow
                            {
                                Title = BeatmapsetsStrings.ShowStatsDrain,
                                Width = 0.5f,
                                Padding = new MarginPadding { Left = 5, Vertical = 2.5f },
                            },
                            Accuracy = new StatisticRow
                            {
                                Title = BeatmapsetsStrings.ShowStatsAccuracy,
                                Width = 0.5f,
                                Padding = new MarginPadding { Right = 5, Vertical = 2.5f },
                            },
                            ApproachRate = new StatisticRow
                            {
                                Title = BeatmapsetsStrings.ShowStatsAr,
                                Width = 0.5f,
                                Padding = new MarginPadding { Left = 5, Vertical = 2.5f },
                            },
                            starDifficulty = new StatisticRow(10, true)
                            {
                                Title = BeatmapsetsStrings.ShowStatsStars,
                                Width = 0.5f,
                                Padding = new MarginPadding { Right = 5, Vertical = 2.5f },
                            },
                        },
                    };
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starDifficulty.AccentColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Ruleset.BindValueChanged(_ => updateStatistics());

            mods.BindValueChanged(modsChanged, true);
        }

        private ModSettingChangeTracker modSettingChangeTracker;
        private ScheduledDelegate debouncedStatisticsUpdate;

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingChangeTracker?.Dispose();

            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += _ =>
            {
                debouncedStatisticsUpdate?.Cancel();
                debouncedStatisticsUpdate = Scheduler.AddDelayed(updateStatistics, 100);
            };

            updateStatistics();
        }

        private void updateStatistics()
        {
            IBeatmapDifficultyInfo baseDifficulty = BeatmapInfo?.Difficulty;
            BeatmapDifficulty adjustedDifficulty = null;

            if (baseDifficulty != null)
            {
                BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(baseDifficulty);

                foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(originalDifficulty);

                adjustedDifficulty = originalDifficulty;

                if (Ruleset.Value != null)
                {
                    double rate = 1;
                    foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                        rate = mod.ApplyToRate(0, rate);

                    adjustedDifficulty = Ruleset.Value.CreateInstance().GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

                    TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, adjustedDifficulty);
                }
            }

            switch (Ruleset.Value?.OnlineID)
            {
                case 3:
                    // Account for mania differences locally for now.
                    // Eventually this should be handled in a more modular way, allowing rulesets to return arbitrary difficulty attributes.
                    ILegacyRuleset legacyRuleset = (ILegacyRuleset)Ruleset.Value.CreateInstance();

                    // For the time being, the key count is static no matter what, because:
                    // a) The method doesn't have knowledge of the active keymods. Doing so may require considerations for filtering.
                    // b) Using the difficulty adjustment mod to adjust OD doesn't have an effect on conversion.
                    int keyCount = baseDifficulty == null ? 0 : legacyRuleset.GetKeyCount(BeatmapInfo, mods.Value);

                    FirstValue.Title = BeatmapsetsStrings.ShowStatsCsMania;
                    FirstValue.Value = (keyCount, keyCount);
                    break;

                default:
                    FirstValue.Title = BeatmapsetsStrings.ShowStatsCs;
                    FirstValue.Value = (baseDifficulty?.CircleSize ?? 0, adjustedDifficulty?.CircleSize);
                    break;
            }

            HpDrain.Value = (baseDifficulty?.DrainRate ?? 0, adjustedDifficulty?.DrainRate);
            Accuracy.Value = (baseDifficulty?.OverallDifficulty ?? 0, adjustedDifficulty?.OverallDifficulty);
            ApproachRate.Value = (baseDifficulty?.ApproachRate ?? 0, adjustedDifficulty?.ApproachRate);

            updateStarDifficulty();
        }

        private CancellationTokenSource starDifficultyCancellationSource;

        /// <summary>
        /// Updates the displayed star difficulty statistics with the values provided by the currently-selected beatmap, ruleset, and selected mods.
        /// </summary>
        /// <remarks>
        /// This is scheduled to avoid scenarios wherein a ruleset changes first before selected mods do,
        /// potentially resulting in failure during difficulty calculation due to incomplete bindable state updates.
        /// </remarks>
        private void updateStarDifficulty() => Scheduler.AddOnce(() =>
        {
            starDifficultyCancellationSource?.Cancel();

            if (BeatmapInfo == null)
                return;

            starDifficultyCancellationSource = new CancellationTokenSource();

            var normalStarDifficultyTask = difficultyCache.GetDifficultyAsync(BeatmapInfo, Ruleset.Value, null, starDifficultyCancellationSource.Token);
            var moddedStarDifficultyTask = difficultyCache.GetDifficultyAsync(BeatmapInfo, Ruleset.Value, mods.Value, starDifficultyCancellationSource.Token);

            Task.WhenAll(normalStarDifficultyTask, moddedStarDifficultyTask).ContinueWith(_ => Schedule(() =>
            {
                var normalDifficulty = normalStarDifficultyTask.GetResultSafely();
                var moddedDifficulty = moddedStarDifficultyTask.GetResultSafely();

                if (normalDifficulty == null || moddedDifficulty == null)
                    return;

                starDifficulty.Value = ((float)normalDifficulty.Value.Stars, (float)moddedDifficulty.Value.Stars);
            }), starDifficultyCancellationSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingChangeTracker?.Dispose();
            starDifficultyCancellationSource?.Cancel();
        }

        public partial class StatisticRow : Container, IHasAccentColour
        {
            private const float value_width = 25;
            private const float name_width = 70;

            private readonly float maxValue;
            private readonly bool forceDecimalPlaces;
            private readonly OsuSpriteText name, valueText;
            private readonly Bar bar;
            public readonly Bar ModBar;

            [Resolved]
            private OsuColour colours { get; set; }

            public LocalisableString Title
            {
                get => name.Text;
                set => name.Text = value;
            }

            private (float baseValue, float? adjustedValue)? value;

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
                    ModBar.Length = (value.adjustedValue ?? 0) / maxValue;

                    if (Precision.AlmostEquals(value.baseValue, value.adjustedValue ?? value.baseValue, 0.05f))
                        ModBar.AccentColour = valueText.Colour = Color4.White;
                    else if (value.adjustedValue > value.baseValue)
                        ModBar.AccentColour = valueText.Colour = colours.Red;
                    else if (value.adjustedValue < value.baseValue)
                        ModBar.AccentColour = valueText.Colour = colours.BlueDark;
                }
            }

            public Color4 AccentColour
            {
                get => bar.AccentColour;
                set => bar.AccentColour = value;
            }

            public StatisticRow(float maxValue = 10, bool forceDecimalPlaces = false)
            {
                this.maxValue = maxValue;
                this.forceDecimalPlaces = forceDecimalPlaces;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Vertical = 2.5f };

                Children = new Drawable[]
                {
                    new Container
                    {
                        Width = name_width,
                        AutoSizeAxes = Axes.Y,
                        // osu-web uses 1.25 line-height, which at 12px font size makes the element 14px tall - this compentates that difference
                        Padding = new MarginPadding { Vertical = 1 },
                        Child = name = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12)
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = name_width + 10, Right = value_width + 10 },
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = 5,

                                CornerRadius = 2,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    bar = new Bar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        BackgroundColour = Color4.White.Opacity(0.5f),
                                    },
                                    ModBar = new Bar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.5f,
                                    },
                                }
                            },
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Width = value_width,
                        RelativeSizeAxes = Axes.Y,
                        Child = valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12)
                        },
                    },
                };
            }
        }
    }
}
