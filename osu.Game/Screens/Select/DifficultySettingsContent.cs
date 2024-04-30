// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Mods;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Screens.Select
{
    public partial class DifficultySettingsContent : FillFlowContainer, IHasCustomTooltip<AdjustedAttributesTooltip.Data>
    {
        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private IBindable<IBeatmapInfo?> beatmapInfo { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private IBindable<RulesetInfo> gameRuleset = null!;

        private BarStatisticRow firstValue = null!;
        private BarStatisticRow hpDrain = null!;
        private BarStatisticRow accuracy = null!;
        private BarStatisticRow approachRate = null!;
        private BarStatisticRow starDifficulty = null!;

        public DifficultySettingsContent()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(0, 5);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Children = new[]
            {
                firstValue = new BarStatisticRow(), // circle size/key amount
                hpDrain = new BarStatisticRow { Title = BeatmapsetsStrings.ShowStatsDrain },
                accuracy = new BarStatisticRow { Title = BeatmapsetsStrings.ShowStatsAccuracy },
                approachRate = new BarStatisticRow { Title = BeatmapsetsStrings.ShowStatsAr },
                starDifficulty = new BarStatisticRow(10, true)
                {
                    Title = BeatmapsetsStrings.ShowStatsStars,
                    AccentColour = colours.Yellow,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // the cached ruleset bindable might be a decoupled bindable provided by SongSelect,
            // which we can't rely on in combination with the game-wide selected mods list,
            // since mods could be updated to the new ruleset instances while the decoupled bindable is held behind,
            // therefore resulting in performing difficulty calculation with invalid states.
            gameRuleset = game.Ruleset.GetBoundCopy();
            gameRuleset.BindValueChanged(_ => updateStatistics());

            beatmapInfo.BindValueChanged(_ => updateStatistics(), true);

            mods.BindValueChanged(modsChanged);
        }

        private ModSettingChangeTracker? modSettingChangeTracker;
        private ScheduledDelegate? debouncedStatisticsUpdate;

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
            switch (beatmapInfo.Value)
            {
                case BeatmapInfo:
                    IBeatmapDifficultyInfo? baseDifficulty = beatmapInfo.Value?.Difficulty;
                    BeatmapDifficulty? adjustedDifficulty = null;

                    IRulesetInfo ruleset = gameRuleset.Value ?? beatmapInfo.Value!.Ruleset;

                    if (baseDifficulty != null)
                    {
                        BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(baseDifficulty);

                        foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                            mod.ApplyToDifficulty(originalDifficulty);

                        double rate = 1;
                        foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                            rate = mod.ApplyToRate(0, rate);

                        adjustedDifficulty = ruleset.CreateInstance().GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

                        TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, adjustedDifficulty);
                    }

                    switch (ruleset.OnlineID)
                    {
                        case 3:
                            // Account for mania differences locally for now.
                            // Eventually this should be handled in a more modular way, allowing rulesets to return arbitrary difficulty attributes.
                            ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.CreateInstance();

                            // For the time being, the key count is static no matter what, because:
                            // a) The method doesn't have knowledge of the active keymods. Doing so may require considerations for filtering.
                            // b) Using the difficulty adjustment mod to adjust OD doesn't have an effect on conversion.
                            int keyCount = baseDifficulty == null ? 0 : legacyRuleset.GetKeyCount(beatmapInfo.Value!, mods.Value);

                            firstValue.Title = BeatmapsetsStrings.ShowStatsCsMania;
                            firstValue.Value = (keyCount, keyCount);
                            break;

                        default:
                            firstValue.Title = BeatmapsetsStrings.ShowStatsCs;
                            firstValue.Value = (baseDifficulty?.CircleSize ?? 0, adjustedDifficulty?.CircleSize);
                            break;
                    }

                    hpDrain.Value = (baseDifficulty?.DrainRate ?? 0, adjustedDifficulty?.DrainRate);
                    accuracy.Value = (baseDifficulty?.OverallDifficulty ?? 0, adjustedDifficulty?.OverallDifficulty);
                    approachRate.Value = (baseDifficulty?.ApproachRate ?? 0, adjustedDifficulty?.ApproachRate);
                    break;

                case APIBeatmap apiBeatmap:
                    switch (apiBeatmap.Ruleset.OnlineID)
                    {
                        case 3:
                            firstValue.Title = BeatmapsetsStrings.ShowStatsCsMania;
                            break;

                        default:
                            firstValue.Title = BeatmapsetsStrings.ShowStatsCs;
                            break;
                    }

                    firstValue.Value = (apiBeatmap.CircleSize, null);
                    hpDrain.Value = (apiBeatmap.DrainRate, null);
                    accuracy.Value = (apiBeatmap.OverallDifficulty, null);
                    approachRate.Value = (apiBeatmap.ApproachRate, null);
                    break;
            }

            updateStarDifficulty();
        }

        private CancellationTokenSource? starDifficultyCancellationSource;

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

            if (beatmapInfo.Value == null)
                return;

            starDifficultyCancellationSource = new CancellationTokenSource();

            var normalStarDifficultyTask = difficultyCache.GetDifficultyAsync(beatmapInfo.Value, gameRuleset.Value, null, starDifficultyCancellationSource.Token);
            var moddedStarDifficultyTask = difficultyCache.GetDifficultyAsync(beatmapInfo.Value, gameRuleset.Value, mods.Value, starDifficultyCancellationSource.Token);

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
        }

        public ITooltip<AdjustedAttributesTooltip.Data> GetCustomTooltip() => new AdjustedAttributesTooltip();

        public AdjustedAttributesTooltip.Data TooltipContent { get; private set; } = null!;
    }
}
