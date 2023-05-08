// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Online.API;

namespace osu.Game.Scoring
{
    public class ScoreManager : ModelManager<ScoreInfo>, IModelImporter<ScoreInfo>
    {
        private readonly OsuConfigManager configManager;
        private readonly ScoreImporter scoreImporter;
        private readonly LegacyScoreExporter scoreExporter;

        public override bool PauseImports
        {
            get => base.PauseImports;
            set
            {
                base.PauseImports = value;
                scoreImporter.PauseImports = value;
            }
        }

        public ScoreManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, RealmAccess realm, IAPIProvider api,
                            OsuConfigManager configManager = null)
            : base(storage, realm)
        {
            this.configManager = configManager;

            scoreImporter = new ScoreImporter(rulesets, beatmaps, storage, realm, api)
            {
                PostNotification = obj => PostNotification?.Invoke(obj)
            };

            scoreExporter = new LegacyScoreExporter(storage)
            {
                PostNotification = obj => PostNotification?.Invoke(obj)
            };
        }

        public Score GetScore(ScoreInfo score) => scoreImporter.GetScore(score);

        /// <summary>
        /// Perform a lookup query on available <see cref="ScoreInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public ScoreInfo Query(Expression<Func<ScoreInfo, bool>> query)
        {
            return Realm.Run(r => r.All<ScoreInfo>().FirstOrDefault(query)?.Detach());
        }

        /// <summary>
        /// Orders an array of <see cref="ScoreInfo"/>s by total score.
        /// </summary>
        /// <param name="scores">The array of <see cref="ScoreInfo"/>s to reorder.</param>
        /// <returns>The given <paramref name="scores"/> ordered by decreasing total score.</returns>
        public IEnumerable<ScoreInfo> OrderByTotalScore(IEnumerable<ScoreInfo> scores)
            => scores.OrderByDescending(s => GetTotalScore(s))
                     .ThenBy(s => s.OnlineID)
                     // Local scores may not have an online ID. Fall back to date in these cases.
                     .ThenBy(s => s.Date);

        /// <summary>
        /// Retrieves a bindable that represents the total score of a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="ScoreInfo"/> to retrieve the bindable for.</param>
        /// <returns>The bindable containing the total score.</returns>
        public Bindable<long> GetBindableTotalScore([NotNull] ScoreInfo score) => new TotalScoreBindable(score, this, configManager);

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
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to calculate the total score of.</param>
        /// <param name="mode">The <see cref="ScoringMode"/> to return the total score as.</param>
        /// <returns>The total score.</returns>
        public long GetTotalScore([NotNull] ScoreInfo score, ScoringMode mode = ScoringMode.Standardised)
        {
            // TODO: This is required for playlist aggregate scores. They should likely not be getting here in the first place.
            if (string.IsNullOrEmpty(score.BeatmapInfo.MD5Hash))
                return score.TotalScore;

            var ruleset = score.Ruleset.CreateInstance();
            var scoreProcessor = ruleset.CreateScoreProcessor();
            scoreProcessor.Mods.Value = score.Mods;

            return scoreProcessor.ComputeScore(mode, score);
        }

        /// <summary>
        /// Retrieves the maximum achievable combo for the provided score.
        /// </summary>
        /// <param name="score">The <see cref="ScoreInfo"/> to compute the maximum achievable combo for.</param>
        /// <returns>The maximum achievable combo.</returns>
        public int GetMaximumAchievableCombo([NotNull] ScoreInfo score) => score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Sum(kvp => kvp.Value);

        /// <summary>
        /// Provides the total score of a <see cref="ScoreInfo"/>. Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </summary>
        private class TotalScoreBindable : Bindable<long>
        {
            private readonly Bindable<ScoringMode> scoringMode = new Bindable<ScoringMode>();

            /// <summary>
            /// Creates a new <see cref="TotalScoreBindable"/>.
            /// </summary>
            /// <param name="score">The <see cref="ScoreInfo"/> to provide the total score of.</param>
            /// <param name="scoreManager">The <see cref="ScoreManager"/>.</param>
            /// <param name="configManager">The config.</param>
            public TotalScoreBindable(ScoreInfo score, ScoreManager scoreManager, OsuConfigManager configManager)
            {
                configManager?.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
                scoringMode.BindValueChanged(mode => Value = scoreManager.GetTotalScore(score, mode.NewValue), true);
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

        public void Delete([CanBeNull] Expression<Func<ScoreInfo, bool>> filter = null, bool silent = false)
        {
            Realm.Run(r =>
            {
                var items = r.All<ScoreInfo>()
                             .Where(s => !s.DeletePending);

                if (filter != null)
                    items = items.Where(filter);

                Delete(items.ToList(), silent);
            });
        }

        public void Delete(BeatmapInfo beatmap, bool silent = false)
        {
            Realm.Run(r =>
            {
                var beatmapScores = r.Find<BeatmapInfo>(beatmap.ID).Scores.ToList();
                Delete(beatmapScores, silent);
            });
        }

        public Task Import(params string[] paths) => scoreImporter.Import(paths);

        public Task Import(ImportTask[] imports, ImportParameters parameters = default) => scoreImporter.Import(imports, parameters);

        public override bool IsAvailableLocally(ScoreInfo model) => Realm.Run(realm => realm.All<ScoreInfo>().Any(s => s.OnlineID == model.OnlineID));

        public IEnumerable<string> HandledExtensions => scoreImporter.HandledExtensions;

        public Task<IEnumerable<Live<ScoreInfo>>> Import(ProgressNotification notification, ImportTask[] tasks, ImportParameters parameters = default) => scoreImporter.Import(notification, tasks);

        public Task Export(ScoreInfo score) => scoreExporter.ExportAsync(score.ToLive(Realm));

        public Task<Live<ScoreInfo>> ImportAsUpdate(ProgressNotification notification, ImportTask task, ScoreInfo original) => scoreImporter.ImportAsUpdate(notification, task, original);

        public Live<ScoreInfo> Import(ScoreInfo item, ArchiveReader archive = null, ImportParameters parameters = default, CancellationToken cancellationToken = default) =>
            scoreImporter.ImportModel(item, archive, parameters, cancellationToken);

        /// <summary>
        /// Populates the <see cref="ScoreInfo.MaximumStatistics"/> for a given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to populate the statistics of.</param>
        public void PopulateMaximumStatistics(ScoreInfo score) => scoreImporter.PopulateMaximumStatistics(score);

        #region Implementation of IPresentImports<ScoreInfo>

        public Action<IEnumerable<Live<ScoreInfo>>> PresentImport
        {
            set => scoreImporter.PresentImport = value;
        }

        #endregion
    }
}
