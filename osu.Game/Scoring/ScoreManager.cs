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
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Scoring
{
    public class ScoreManager : IModelManager<ScoreInfo>, IModelImporter<ScoreInfo>, IModelFileManager<ScoreInfo, ScoreFileInfo>, IModelDownloader<IScoreInfo>
    {
        private readonly Scheduler scheduler;
        private readonly Func<BeatmapDifficultyCache> difficulties;
        private readonly OsuConfigManager configManager;
        private readonly ScoreModelManager scoreModelManager;
        private readonly ScoreModelDownloader scoreModelDownloader;

        public ScoreManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, IAPIProvider api, IDatabaseContextFactory contextFactory, Scheduler scheduler,
                            IIpcHost importHost = null, Func<BeatmapDifficultyCache> difficulties = null, OsuConfigManager configManager = null)
        {
            this.scheduler = scheduler;
            this.difficulties = difficulties;
            this.configManager = configManager;

            scoreModelManager = new ScoreModelManager(rulesets, beatmaps, storage, contextFactory, importHost);
            scoreModelDownloader = new ScoreModelDownloader(scoreModelManager, api, importHost);
        }

        public Score GetScore(ScoreInfo score) => scoreModelManager.GetScore(score);

        public List<ScoreInfo> GetAllUsableScores() => scoreModelManager.GetAllUsableScores();

        public IEnumerable<ScoreInfo> QueryScores(Expression<Func<ScoreInfo, bool>> query) => scoreModelManager.QueryScores(query);

        public ScoreInfo Query(Expression<Func<ScoreInfo, bool>> query) => scoreModelManager.Query(query);

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
                    await difficultyCache.GetDifficultyAsync(s.BeatmapInfo, s.Ruleset, s.Mods, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                }
            }

            long[] totalScores = await Task.WhenAll(scores.Select(s => GetTotalScoreAsync(s, cancellationToken: cancellationToken))).ConfigureAwait(false);

            return scores.Select((score, index) => (score, totalScore: totalScores[index]))
                         .OrderByDescending(g => g.totalScore)
                         .ThenBy(g => g.score.OnlineScoreID)
                         .Select(g => g.score)
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
            if (score.BeatmapInfo == null)
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
                if (score.BeatmapInfo.MaxCombo != null)
                    beatmapMaxCombo = score.BeatmapInfo.MaxCombo.Value;
                else
                {
                    if (score.BeatmapInfo.ID == 0 || difficulties == null)
                    {
                        // We don't have enough information (max combo) to compute the score, so use the provided score.
                        return score.TotalScore;
                    }

                    // We can compute the max combo locally after the async beatmap difficulty computation.
                    var difficulty = await difficulties().GetDifficultyAsync(score.BeatmapInfo, score.Ruleset, score.Mods, cancellationToken).ConfigureAwait(false);
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

        #region Implementation of IPostNotifications

        public Action<Notification> PostNotification
        {
            set
            {
                scoreModelManager.PostNotification = value;
                scoreModelDownloader.PostNotification = value;
            }
        }

        #endregion

        #region Implementation of IModelManager<ScoreInfo>

        public event Action<ScoreInfo> ItemUpdated
        {
            add => scoreModelManager.ItemUpdated += value;
            remove => scoreModelManager.ItemUpdated -= value;
        }

        public event Action<ScoreInfo> ItemRemoved
        {
            add => scoreModelManager.ItemRemoved += value;
            remove => scoreModelManager.ItemRemoved -= value;
        }

        public Task ImportFromStableAsync(StableStorage stableStorage)
        {
            return scoreModelManager.ImportFromStableAsync(stableStorage);
        }

        public void Export(ScoreInfo item)
        {
            scoreModelManager.Export(item);
        }

        public void ExportModelTo(ScoreInfo model, Stream outputStream)
        {
            scoreModelManager.ExportModelTo(model, outputStream);
        }

        public void Update(ScoreInfo item)
        {
            scoreModelManager.Update(item);
        }

        public bool Delete(ScoreInfo item)
        {
            return scoreModelManager.Delete(item);
        }

        public void Delete(List<ScoreInfo> items, bool silent = false)
        {
            scoreModelManager.Delete(items, silent);
        }

        public void Undelete(List<ScoreInfo> items, bool silent = false)
        {
            scoreModelManager.Undelete(items, silent);
        }

        public void Undelete(ScoreInfo item)
        {
            scoreModelManager.Undelete(item);
        }

        public Task Import(params string[] paths)
        {
            return scoreModelManager.Import(paths);
        }

        public Task Import(params ImportTask[] tasks)
        {
            return scoreModelManager.Import(tasks);
        }

        public IEnumerable<string> HandledExtensions => scoreModelManager.HandledExtensions;

        public Task<IEnumerable<ILive<ScoreInfo>>> Import(ProgressNotification notification, params ImportTask[] tasks)
        {
            return scoreModelManager.Import(notification, tasks);
        }

        public Task<ILive<ScoreInfo>> Import(ImportTask task, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return scoreModelManager.Import(task, lowPriority, cancellationToken);
        }

        public Task<ILive<ScoreInfo>> Import(ArchiveReader archive, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return scoreModelManager.Import(archive, lowPriority, cancellationToken);
        }

        public Task<ILive<ScoreInfo>> Import(ScoreInfo item, ArchiveReader archive = null, bool lowPriority = false, CancellationToken cancellationToken = default)
        {
            return scoreModelManager.Import(item, archive, lowPriority, cancellationToken);
        }

        public bool IsAvailableLocally(ScoreInfo model)
        {
            return scoreModelManager.IsAvailableLocally(model);
        }

        #endregion

        #region Implementation of IModelFileManager<in ScoreInfo,in ScoreFileInfo>

        public void ReplaceFile(ScoreInfo model, ScoreFileInfo file, Stream contents, string filename = null)
        {
            scoreModelManager.ReplaceFile(model, file, contents, filename);
        }

        public void DeleteFile(ScoreInfo model, ScoreFileInfo file)
        {
            scoreModelManager.DeleteFile(model, file);
        }

        public void AddFile(ScoreInfo model, Stream contents, string filename)
        {
            scoreModelManager.AddFile(model, contents, filename);
        }

        #endregion

        #region Implementation of IModelDownloader<IScoreInfo>

        public event Action<ArchiveDownloadRequest<IScoreInfo>> DownloadBegan
        {
            add => scoreModelDownloader.DownloadBegan += value;
            remove => scoreModelDownloader.DownloadBegan -= value;
        }

        public event Action<ArchiveDownloadRequest<IScoreInfo>> DownloadFailed
        {
            add => scoreModelDownloader.DownloadFailed += value;
            remove => scoreModelDownloader.DownloadFailed -= value;
        }

        public bool Download(IScoreInfo model, bool minimiseDownloadSize) =>
            scoreModelDownloader.Download(model, minimiseDownloadSize);

        public ArchiveDownloadRequest<IScoreInfo> GetExistingDownload(IScoreInfo model)
        {
            return scoreModelDownloader.GetExistingDownload(model);
        }

        #endregion

        #region Implementation of IPresentImports<ScoreInfo>

        public Action<IEnumerable<ILive<ScoreInfo>>> PostImport
        {
            set => scoreModelManager.PostImport = value;
        }

        #endregion
    }
}
