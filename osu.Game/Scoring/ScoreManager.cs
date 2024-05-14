// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;
using osu.Game.Online.API;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Scoring
{
    public class ScoreManager : ModelManager<ScoreInfo>, IModelImporter<ScoreInfo>
    {
        private readonly Func<BeatmapManager> beatmaps;
        private readonly OsuConfigManager? configManager;
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
                            OsuConfigManager? configManager = null)
            : base(storage, realm)
        {
            this.beatmaps = beatmaps;
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

        /// <summary>
        /// Retrieve a <see cref="Score"/> from a given <see cref="IScoreInfo"/>.
        /// </summary>
        /// <param name="scoreInfo">The <see cref="IScoreInfo"/> to convert.</param>
        /// <returns>The <see cref="Score"/>. Null if the score cannot be found in the database.</returns>
        /// <remarks>
        /// The <see cref="IScoreInfo"/> is re-retrieved from the database to ensure all the required data
        /// for retrieving a replay are present (may have missing properties if it was retrieved from online data).
        /// </remarks>
        public Score? GetScore(IScoreInfo scoreInfo)
        {
            ScoreInfo? databasedScoreInfo = getDatabasedScoreInfo(scoreInfo);

            return databasedScoreInfo == null ? null : scoreImporter.GetScore(databasedScoreInfo);
        }

        /// <summary>
        /// Perform a lookup query on available <see cref="ScoreInfo"/>s.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>The first result for the provided query, or null if no results were found.</returns>
        public ScoreInfo? Query(Expression<Func<ScoreInfo, bool>> query)
        {
            return Realm.Run(r => r.All<ScoreInfo>().FirstOrDefault(query)?.Detach());
        }

        private ScoreInfo? getDatabasedScoreInfo(IScoreInfo originalScoreInfo)
        {
            ScoreInfo? databasedScoreInfo = null;

            if (originalScoreInfo is ScoreInfo scoreInfo)
                databasedScoreInfo = Query(s => s.Hash == scoreInfo.Hash);

            if (originalScoreInfo.OnlineID > 0)
                databasedScoreInfo ??= Query(s => s.OnlineID == originalScoreInfo.OnlineID);

            if (originalScoreInfo.LegacyOnlineID > 0)
                databasedScoreInfo ??= Query(s => s.LegacyOnlineID == originalScoreInfo.LegacyOnlineID);

            if (databasedScoreInfo == null)
            {
                Logger.Log("The requested score could not be found locally.", LoggingTarget.Information);
                return null;
            }

            return databasedScoreInfo;
        }

        /// <summary>
        /// Retrieves a bindable that represents the total score of a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="ScoreInfo"/> to retrieve the bindable for.</param>
        /// <returns>The bindable containing the total score.</returns>
        public Bindable<long> GetBindableTotalScore(ScoreInfo score) => new TotalScoreBindable(score, configManager);

        /// <summary>
        /// Retrieves a bindable that represents the formatted total score string of a <see cref="ScoreInfo"/>.
        /// </summary>
        /// <remarks>
        /// Responds to changes in the currently-selected <see cref="ScoringMode"/>.
        /// </remarks>
        /// <param name="score">The <see cref="ScoreInfo"/> to retrieve the bindable for.</param>
        /// <returns>The bindable containing the formatted total score string.</returns>
        public Bindable<string> GetBindableTotalScoreString(ScoreInfo score) => new TotalScoreStringBindable(GetBindableTotalScore(score));

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
            /// <param name="configManager">The config.</param>
            public TotalScoreBindable(ScoreInfo score, OsuConfigManager? configManager)
            {
                configManager?.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
                scoringMode.BindValueChanged(mode => Value = score.GetDisplayScore(mode.NewValue), true);
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

        public void Delete(Expression<Func<ScoreInfo, bool>>? filter = null, bool silent = false)
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
                var beatmapScores = r.Find<BeatmapInfo>(beatmap.ID)!.Scores.ToList();
                Delete(beatmapScores, silent);
            });
        }

        public Task Import(params string[] paths) => scoreImporter.Import(paths);

        public Task Import(ImportTask[] imports, ImportParameters parameters = default) => scoreImporter.Import(imports, parameters);

        public override bool IsAvailableLocally(ScoreInfo model)
            => Realm.Run(realm => realm.All<ScoreInfo>()
                                       // this basically inlines `ModelExtension.MatchesOnlineID(IScoreInfo, IScoreInfo)`,
                                       // because that method can't be used here, as realm can't translate it to its query language.
                                       .Any(s => s.OnlineID == model.OnlineID || s.LegacyOnlineID == model.LegacyOnlineID));

        public IEnumerable<string> HandledExtensions => scoreImporter.HandledExtensions;

        public Task<IEnumerable<Live<ScoreInfo>>> Import(ProgressNotification notification, ImportTask[] tasks, ImportParameters parameters = default) => scoreImporter.Import(notification, tasks);

        /// <summary>
        /// Export a replay from a given <see cref="IScoreInfo"/>.
        /// </summary>
        /// <param name="scoreInfo">The <see cref="IScoreInfo"/> to export.</param>
        /// <returns>The <see cref="Task"/>. Return <see cref="Task.CompletedTask"/> if the score cannot be found in the database.</returns>
        /// <remarks>
        /// The <see cref="IScoreInfo"/> is re-retrieved from the database to ensure all the required data
        /// for exporting a replay are present (may have missing properties if it was retrieved from online data).
        /// </remarks>
        public Task Export(ScoreInfo scoreInfo)
        {
            ScoreInfo? databasedScoreInfo = getDatabasedScoreInfo(scoreInfo);

            return databasedScoreInfo == null ? Task.CompletedTask : scoreExporter.ExportAsync(databasedScoreInfo.ToLive(Realm));
        }

        public Task<Live<ScoreInfo>?> ImportAsUpdate(ProgressNotification notification, ImportTask task, ScoreInfo original) => scoreImporter.ImportAsUpdate(notification, task, original);

        public Live<ScoreInfo>? Import(ScoreInfo item, ArchiveReader? archive = null, ImportParameters parameters = default, CancellationToken cancellationToken = default) =>
            scoreImporter.ImportModel(item, archive, parameters, cancellationToken);

        /// <summary>
        /// Populates the <see cref="ScoreInfo.MaximumStatistics"/> for a given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to populate the statistics of.</param>
        public void PopulateMaximumStatistics(ScoreInfo score)
        {
            Debug.Assert(score.BeatmapInfo != null);
            LegacyScoreDecoder.PopulateMaximumStatistics(score, beatmaps().GetWorkingBeatmap(score.BeatmapInfo.Detach()));
        }

        #region Implementation of IPresentImports<ScoreInfo>

        public Action<IEnumerable<Live<ScoreInfo>>>? PresentImport
        {
            set => scoreImporter.PresentImport = value;
        }

        #endregion
    }
}
