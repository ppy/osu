// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Scoring
{
    public class ScoreManager : DownloadableArchiveModelManager<ScoreInfo, ScoreFileInfo>
    {
        public override IEnumerable<string> HandledExtensions => new[] { ".osr" };

        protected override string[] HashableFileTypes => new[] { ".osr" };

        protected override string ImportFromStablePath => Path.Combine("Data", "r");

        private readonly RulesetStore rulesets;
        private readonly Func<BeatmapManager> beatmaps;
        private readonly Scheduler scheduler;

        [CanBeNull]
        private readonly Func<BeatmapDifficultyCache> difficulties;

        [CanBeNull]
        private readonly OsuConfigManager configManager;

        public ScoreManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, IAPIProvider api, IDatabaseContextFactory contextFactory, Scheduler scheduler,
                            IIpcHost importHost = null, Func<BeatmapDifficultyCache> difficulties = null, OsuConfigManager configManager = null)
            : base(storage, contextFactory, api, new ScoreStore(contextFactory, storage), importHost)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
            this.scheduler = scheduler;
            this.difficulties = difficulties;
            this.configManager = configManager;
        }

        protected override ScoreInfo CreateModel(ArchiveReader archive)
        {
            if (archive == null)
                return null;

            using (var stream = archive.GetStream(archive.Filenames.First(f => f.EndsWith(".osr", StringComparison.OrdinalIgnoreCase))))
            {
                try
                {
                    return new DatabasedLegacyScoreDecoder(rulesets, beatmaps()).Parse(stream).ScoreInfo;
                }
                catch (LegacyScoreDecoder.BeatmapNotFoundException e)
                {
                    Logger.Log(e.Message, LoggingTarget.Information, LogLevel.Error);
                    return null;
                }
            }
        }

        protected override Task Populate(ScoreInfo model, ArchiveReader archive, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        protected override void ExportModelTo(ScoreInfo model, Stream outputStream)
        {
            var file = model.Files.SingleOrDefault();
            if (file == null)
                return;

            using (var inputStream = Files.Storage.GetStream(file.FileInfo.StoragePath))
                inputStream.CopyTo(outputStream);
        }

        protected override IEnumerable<string> GetStableImportPaths(Storage storage)
            => storage.GetFiles(ImportFromStablePath).Where(p => HandledExtensions.Any(ext => Path.GetExtension(p)?.Equals(ext, StringComparison.OrdinalIgnoreCase) ?? false))
                      .Select(path => storage.GetFullPath(path));

        public Score GetScore(ScoreInfo score) => new LegacyDatabasedScore(score, rulesets, beatmaps(), Files.Store);

        public List<ScoreInfo> GetAllUsableScores() => ModelStore.ConsumableItems.Where(s => !s.DeletePending).ToList();

        public IEnumerable<ScoreInfo> QueryScores(Expression<Func<ScoreInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().Where(query);

        public ScoreInfo Query(Expression<Func<ScoreInfo, bool>> query) => ModelStore.ConsumableItems.AsNoTracking().FirstOrDefault(query);

        protected override ArchiveDownloadRequest<ScoreInfo> CreateDownloadRequest(ScoreInfo score, bool minimiseDownload) => new DownloadReplayRequest(score);

        protected override bool CheckLocalAvailability(ScoreInfo model, IQueryable<ScoreInfo> items)
            => base.CheckLocalAvailability(model, items)
               || (model.OnlineScoreID != null && items.Any(i => i.OnlineScoreID == model.OnlineScoreID));

        /// <summary>
        /// Orders an array of <see cref="ScoreInfo"/>s by total score.
        /// </summary>
        /// <param name="scores">The array of <see cref="ScoreInfo"/>s to reorder.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the process.</param>
        /// <returns>The given <paramref name="scores"/> ordered by decreasing total score.</returns>
        public async Task<ScoreInfo[]> OrderByTotalScoreAsync(ScoreInfo[] scores, CancellationToken cancellationToken = default)
        {
            var difficultyCache = difficulties?.Invoke();

            if (difficultyCache != null)
            {
                // Compute difficulties asynchronously first to prevent blocking via the GetTotalScore() call below.
                foreach (var s in scores)
                {
                    await difficultyCache.GetDifficultyAsync(s.Beatmap, s.Ruleset, s.Mods, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            // We're calling .Result, but this should not be a blocking call due to the above GetDifficultyAsync() calls.
            return scores.OrderByDescending(s => GetTotalScoreAsync(s, cancellationToken: cancellationToken).Result)
                         .ThenBy(s => s.OnlineScoreID)
                         .ToArray();
        }

        /// <summary>
        /// Retrieves a bindable that represents the total score of a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="ScoreInfo"/> to retrieve the bindable for.</param>
        /// <returns>The bindable containing the total score.</returns>
        public Bindable<long> GetBindableTotalScore([NotNull] ScoreInfo score)
        {
            var bindable = new TotalScoreBindable(score, this);
            configManager?.BindWith(OsuSetting.ScoreDisplayMode, bindable.ScoringMode);
            return bindable;
        }

        /// <summary>
        /// Retrieves a bindable that represents the formatted total score string of a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="ScoreInfo"/> to retrieve the bindable for.</param>
        /// <returns>The bindable containing the formatted total score string.</returns>
        public Bindable<string> GetBindableTotalScoreString([NotNull] ScoreInfo score) => new TotalScoreStringBindable(GetBindableTotalScore(score));

        /// <summary>
        /// Retrieves the total score of a <see cref="ScoreInfo"/> in the given <see cref="ScoringMode"/>.
        /// The score is returned in a callback that is run on the update thread.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to calculate the total score of.</param>
        /// <param name="callback">The callback to be invoked with the total score.</param>
        /// <param name="mode">The <see cref="ScoringMode"/> to return the total score as.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the process.</param>
        public void GetTotalScore([NotNull] ScoreInfo score, [NotNull] Action<long> callback, ScoringMode mode = ScoringMode.Standardised, CancellationToken cancellationToken = default)
        {
            GetTotalScoreAsync(score, mode, cancellationToken)
                .ContinueWith(s => scheduler.Add(() => callback(s.Result)), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        /// <summary>
        /// Retrieves the total score of a <see cref="ScoreInfo"/> in the given <see cref="ScoringMode"/>.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to calculate the total score of.</param>
        /// <param name="mode">The <see cref="ScoringMode"/> to return the total score as.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the process.</param>
        /// <returns>The total score.</returns>
        public async Task<long> GetTotalScoreAsync([NotNull] ScoreInfo score, ScoringMode mode = ScoringMode.Standardised, CancellationToken cancellationToken = default)
        {
            if (score.Beatmap == null)
                return score.TotalScore;

            int beatmapMaxCombo;
            double accuracy = score.Accuracy;

            if (score.IsLegacyScore)
            {
                if (score.RulesetID == 3)
                {
                    // In osu!stable, a full-GREAT score has 100% accuracy in mania. Along with a full combo, the score becomes indistinguishable from a full-PERFECT score.
                    // To get around this, recalculate accuracy based on the hit statistics.
                    // Note: This cannot be applied universally to all legacy scores, as some rulesets (e.g. catch) group multiple judgements together.
                    double maxBaseScore = score.Statistics.Select(kvp => kvp.Value).Sum() * Judgement.ToNumericResult(HitResult.Perfect);
                    double baseScore = score.Statistics.Select(kvp => Judgement.ToNumericResult(kvp.Key) * kvp.Value).Sum();
                    if (maxBaseScore > 0)
                        accuracy = baseScore / maxBaseScore;
                }

                // This score is guaranteed to be an osu!stable score.
                // The combo must be determined through either the beatmap's max combo value or the difficulty calculator, as lazer's scoring has changed and the score statistics cannot be used.
                if (score.Beatmap.MaxCombo != null)
                    beatmapMaxCombo = score.Beatmap.MaxCombo.Value;
                else
                {
                    if (score.Beatmap.ID == 0 || difficulties == null)
                    {
                        // We don't have enough information (max combo) to compute the score, so use the provided score.
                        return score.TotalScore;
                    }

                    // We can compute the max combo locally after the async beatmap difficulty computation.
                    var difficulty = await difficulties().GetDifficultyAsync(score.Beatmap, score.Ruleset, score.Mods, cancellationToken).ConfigureAwait(false);
                    beatmapMaxCombo = difficulty.MaxCombo;
                }
            }
            else
            {
                // This is guaranteed to be a non-legacy score.
                // The combo must be determined through the score's statistics, as both the beatmap's max combo and the difficulty calculator will provide osu!stable combo values.
                beatmapMaxCombo = Enum.GetValues(typeof(HitResult)).OfType<HitResult>().Where(r => r.AffectsCombo()).Select(r => score.Statistics.GetValueOrDefault(r)).Sum();
            }

            if (beatmapMaxCombo == 0)
                return 0;

            var ruleset = score.Ruleset.CreateInstance();
            var scoreProcessor = ruleset.CreateScoreProcessor();
            scoreProcessor.Mods.Value = score.Mods;

            return (long)Math.Round(scoreProcessor.GetScore(mode, beatmapMaxCombo, accuracy, (double)score.MaxCombo / beatmapMaxCombo, score.Statistics));
        }

        /// <summary>
        /// Provides the total score of a <see cref="ScoreInfo"/>. Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </summary>
        private class TotalScoreBindable : Bindable<long>
        {
            public readonly Bindable<ScoringMode> ScoringMode = new Bindable<ScoringMode>();

            private readonly ScoreInfo score;
            private readonly ScoreManager scoreManager;

            private CancellationTokenSource difficultyCalculationCancellationSource;

            /// <summary>
            /// Creates a new <see cref="TotalScoreBindable"/>.
            /// </summary>
            /// <param name="score">The <see cref="ScoreInfo"/> to provide the total score of.</param>
            /// <param name="scoreManager">The <see cref="ScoreManager"/>.</param>
            public TotalScoreBindable(ScoreInfo score, ScoreManager scoreManager)
            {
                this.score = score;
                this.scoreManager = scoreManager;

                ScoringMode.BindValueChanged(onScoringModeChanged, true);
            }

            private void onScoringModeChanged(ValueChangedEvent<ScoringMode> mode)
            {
                difficultyCalculationCancellationSource?.Cancel();
                difficultyCalculationCancellationSource = new CancellationTokenSource();

                scoreManager.GetTotalScore(score, s => Value = s, mode.NewValue, difficultyCalculationCancellationSource.Token);
            }
        }

        /// <summary>
        /// Provides the total score of a <see cref="ScoreInfo"/> as a formatted string. Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </summary>
        private class TotalScoreStringBindable : Bindable<string>
        {
            // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable (need to hold a reference)
            private readonly IBindable<long> totalScore;

            public TotalScoreStringBindable(IBindable<long> totalScore)
            {
                this.totalScore = totalScore;
                this.totalScore.BindValueChanged(v => Value = v.NewValue.ToString("N0"), true);
            }
        }
    }
}
