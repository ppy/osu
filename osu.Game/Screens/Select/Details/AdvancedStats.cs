// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Rulesets;

namespace osu.Game.Screens.Select.Details
{
    public class AdvancedStats : Container
    {
        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        protected readonly StatisticRow FirstValue, HpDrain, Accuracy, ApproachRate;
        private readonly StatisticRow starDifficulty;

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

        public AdvancedStats()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Children = new[]
                {
                    FirstValue = new StatisticRow(), // circle size/key amount
                    HpDrain = new StatisticRow { Title = "HP Drain" },
                    Accuracy = new StatisticRow { Title = "Accuracy" },
                    ApproachRate = new StatisticRow { Title = "Approach Rate" },
                    starDifficulty = new StatisticRow(10, true) { Title = "Star Difficulty" },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starDifficulty.AccentColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ => updateStatistics());
            mods.BindValueChanged(modsChanged, true);
        }

        private ModSettingChangeTracker modSettingChangeTracker;
        private ScheduledDelegate debouncedStatisticsUpdate;

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingChangeTracker?.Dispose();

            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += m =>
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

            if (baseDifficulty != null && mods.Value.Any(m => m is IApplicableToDifficulty))
            {
                adjustedDifficulty = new BeatmapDifficulty(baseDifficulty);

                foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(adjustedDifficulty);
            }

            switch (BeatmapInfo?.Ruleset.OnlineID)
            {
                case 3:
                    // Account for mania differences locally for now
                    // Eventually this should be handled in a more modular way, allowing rulesets to return arbitrary difficulty attributes
                    FirstValue.Title = "Key Count";
                    FirstValue.Value = (baseDifficulty?.CircleSize ?? 0, null);
                    break;

                default:
                    FirstValue.Title = "Circle Size";
                    FirstValue.Value = (baseDifficulty?.CircleSize ?? 0, adjustedDifficulty?.CircleSize);
                    break;
            }

            HpDrain.Value = (baseDifficulty?.DrainRate ?? 0, adjustedDifficulty?.DrainRate);
            Accuracy.Value = (baseDifficulty?.OverallDifficulty ?? 0, adjustedDifficulty?.OverallDifficulty);
            ApproachRate.Value = (baseDifficulty?.ApproachRate ?? 0, adjustedDifficulty?.ApproachRate);

            updateStarDifficulty();
        }

        private CancellationTokenSource starDifficultyCancellationSource;

        private void updateStarDifficulty()
        {
            starDifficultyCancellationSource?.Cancel();

            if (BeatmapInfo == null)
                return;

            starDifficultyCancellationSource = new CancellationTokenSource();

            var normalStarDifficultyTask = difficultyCache.GetDifficultyAsync(BeatmapInfo, ruleset.Value, null, starDifficultyCancellationSource.Token);
            var moddedStarDifficultyTask = difficultyCache.GetDifficultyAsync(BeatmapInfo, ruleset.Value, mods.Value, starDifficultyCancellationSource.Token);

            Task.WhenAll(normalStarDifficultyTask, moddedStarDifficultyTask).ContinueWith(_ => Schedule(() =>
            {
                var normalDifficulty = normalStarDifficultyTask.GetResultSafely();
                var moddedDifficulty = moddedStarDifficultyTask.GetResultSafely();

                if (normalDifficulty == null || moddedDifficulty == null)
                    return;

                starDifficulty.Value = ((float)normalDifficulty.Value.Stars, (float)moddedDifficulty.Value.Stars);
            }), starDifficultyCancellationSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingChangeTracker?.Dispose();
            starDifficultyCancellationSource?.Cancel();
        }

        public class StatisticRow : Container, IHasAccentColour
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
                    bar = new Bar
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        BackgroundColour = Color4.White.Opacity(0.5f),
                        Padding = new MarginPadding { Left = name_width + 10, Right = value_width + 10 },
                    },
                    ModBar = new Bar
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Alpha = 0.5f,
                        Height = 5,
                        Padding = new MarginPadding { Left = name_width + 10, Right = value_width + 10 },
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
