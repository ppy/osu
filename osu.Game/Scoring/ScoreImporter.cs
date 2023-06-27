// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Rulesets;
using osu.Game.Scoring.Legacy;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using Realms;

namespace osu.Game.Scoring
{
    public class ScoreImporter : RealmArchiveModelImporter<ScoreInfo>
    {
        public override IEnumerable<string> HandledExtensions => new[] { ".osr" };

        protected override string[] HashableFileTypes => new[] { ".osr" };

        private readonly RulesetStore rulesets;
        private readonly Func<BeatmapManager> beatmaps;

        private readonly IAPIProvider api;

        public ScoreImporter(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, RealmAccess realm, IAPIProvider api)
            : base(storage, realm)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
            this.api = api;
        }

        protected override ScoreInfo? CreateModel(ArchiveReader archive)
        {
            string name = archive.Filenames.First(f => f.EndsWith(".osr", StringComparison.OrdinalIgnoreCase));

            using (var stream = archive.GetStream(name))
            {
                try
                {
                    return new DatabasedLegacyScoreDecoder(rulesets, beatmaps()).Parse(stream).ScoreInfo;
                }
                catch (LegacyScoreDecoder.BeatmapNotFoundException e)
                {
                    Logger.Log($@"Score '{name}' failed to import: no corresponding beatmap with the hash '{e.Hash}' could be found.", LoggingTarget.Database);
                    return null;
                }
            }
        }

        public Score GetScore(ScoreInfo score) => new LegacyDatabasedScore(score, rulesets, beatmaps(), Files.Store);

        protected override void Populate(ScoreInfo model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
        {
            // Ensure the beatmap is not detached.
            if (!model.BeatmapInfo.IsManaged)
                model.BeatmapInfo = realm.Find<BeatmapInfo>(model.BeatmapInfo.ID);

            if (!model.Ruleset.IsManaged)
                model.Ruleset = realm.Find<RulesetInfo>(model.Ruleset.ShortName);

            // These properties are known to be non-null, but these final checks ensure a null hasn't come from somewhere (or the refetch has failed).
            // Under no circumstance do we want these to be written to realm as null.
            ArgumentNullException.ThrowIfNull(model.BeatmapInfo);
            ArgumentNullException.ThrowIfNull(model.Ruleset);

            PopulateMaximumStatistics(model);

            if (string.IsNullOrEmpty(model.StatisticsJson))
                model.StatisticsJson = JsonConvert.SerializeObject(model.Statistics);

            if (string.IsNullOrEmpty(model.MaximumStatisticsJson))
                model.MaximumStatisticsJson = JsonConvert.SerializeObject(model.MaximumStatistics);

            // for pre-ScoreV2 lazer scores, apply a best-effort conversion of total score to ScoreV2.
            // this requires: max combo, statistics, max statistics (where available), and mods to already be populated on the score.
            if (StandardisedScoreMigrationTools.ShouldMigrateToNewStandardised(model))
                model.TotalScore = StandardisedScoreMigrationTools.GetNewStandardised(model);
            else if (model.IsLegacyScore)
                model.TotalScore = ConvertFromLegacyTotalScore(model);
        }

        /// <summary>
        /// Populates the <see cref="ScoreInfo.MaximumStatistics"/> for a given <see cref="ScoreInfo"/>.
        /// </summary>
        /// <param name="score">The score to populate the statistics of.</param>
        public void PopulateMaximumStatistics(ScoreInfo score)
        {
            if (score.MaximumStatistics.Select(kvp => kvp.Value).Sum() > 0)
                return;

            var beatmap = score.BeatmapInfo.Detach();
            var ruleset = score.Ruleset.Detach();
            var rulesetInstance = ruleset.CreateInstance();

            Debug.Assert(rulesetInstance != null);

            // Populate the maximum statistics.
            HitResult maxBasicResult = rulesetInstance.GetHitResults()
                                                      .Select(h => h.result)
                                                      .Where(h => h.IsBasic()).MaxBy(Judgement.ToNumericResult);

            foreach ((HitResult result, int count) in score.Statistics)
            {
                switch (result)
                {
                    case HitResult.LargeTickHit:
                    case HitResult.LargeTickMiss:
                        score.MaximumStatistics[HitResult.LargeTickHit] = score.MaximumStatistics.GetValueOrDefault(HitResult.LargeTickHit) + count;
                        break;

                    case HitResult.SmallTickHit:
                    case HitResult.SmallTickMiss:
                        score.MaximumStatistics[HitResult.SmallTickHit] = score.MaximumStatistics.GetValueOrDefault(HitResult.SmallTickHit) + count;
                        break;

                    case HitResult.IgnoreHit:
                    case HitResult.IgnoreMiss:
                    case HitResult.SmallBonus:
                    case HitResult.LargeBonus:
                        break;

                    default:
                        score.MaximumStatistics[maxBasicResult] = score.MaximumStatistics.GetValueOrDefault(maxBasicResult) + count;
                        break;
                }
            }

            if (!score.IsLegacyScore)
                return;

#pragma warning disable CS0618
            // In osu! and osu!mania, some judgements affect combo but aren't stored to scores.
            // A special hit result is used to pad out the combo value to match, based on the max combo from the difficulty attributes.
            var calculator = rulesetInstance.CreateDifficultyCalculator(beatmaps().GetWorkingBeatmap(beatmap));
            var attributes = calculator.Calculate(score.Mods);

            int maxComboFromStatistics = score.MaximumStatistics.Where(kvp => kvp.Key.AffectsCombo()).Select(kvp => kvp.Value).DefaultIfEmpty(0).Sum();
            if (attributes.MaxCombo > maxComboFromStatistics)
                score.MaximumStatistics[HitResult.LegacyComboIncrease] = attributes.MaxCombo - maxComboFromStatistics;
#pragma warning restore CS0618
        }

        public long ConvertFromLegacyTotalScore(ScoreInfo score)
        {
            if (!score.IsLegacyScore)
                return score.TotalScore;

            var beatmap = beatmaps().GetWorkingBeatmap(score.BeatmapInfo);
            var ruleset = score.Ruleset.CreateInstance();

            var sv1Processor = ruleset.CreateLegacyScoreProcessor();
            if (sv1Processor == null)
                return score.TotalScore;

            sv1Processor.Simulate(beatmap, beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, score.Mods), score.Mods);

            int maximumLegacyAccuracyScore = sv1Processor.AccuracyScore;
            int maximumLegacyComboScore = sv1Processor.ComboScore;
            double maximumLegacyBonusRatio = sv1Processor.BonusScoreRatio;
            double modMultiplier = score.Mods.Select(m => m.ScoreMultiplier).Aggregate(1.0, (c, n) => c * n);

            // The part of total score that doesn't include bonus.
            int maximumLegacyBaseScore = maximumLegacyAccuracyScore + maximumLegacyComboScore;

            // The combo proportion is calculated as a proportion of maximumLegacyBaseScore.
            double comboProportion = Math.Min(1, (double)score.LegacyTotalScore / maximumLegacyBaseScore);

            // The bonus proportion makes up the rest of the score that exceeds maximumLegacyBaseScore.
            double bonusProportion = Math.Max(0, (score.LegacyTotalScore - maximumLegacyBaseScore) * maximumLegacyBonusRatio);

            switch (ruleset.RulesetInfo.OnlineID)
            {
                case 0:
                    return (long)Math.Round((
                        700000 * comboProportion
                        + 300000 * Math.Pow(score.Accuracy, 10)
                        + bonusProportion) * modMultiplier);

                case 1:
                    return (long)Math.Round((
                        250000 * comboProportion
                        + 750000 * Math.Pow(score.Accuracy, 3.6)
                        + bonusProportion) * modMultiplier);

                case 2:
                    return (long)Math.Round((
                        600000 * comboProportion
                        + 400000 * score.Accuracy
                        + bonusProportion) * modMultiplier);

                case 3:
                    return (long)Math.Round((
                        990000 * comboProportion
                        + 10000 * Math.Pow(score.Accuracy, 2 + 2 * score.Accuracy)
                        + bonusProportion) * modMultiplier);

                default:
                    return score.TotalScore;
            }
        }

        // Very naive local caching to improve performance of large score imports (where the username is usually the same for most or all scores).
        private readonly Dictionary<string, APIUser> usernameLookupCache = new Dictionary<string, APIUser>();

        protected override void PostImport(ScoreInfo model, Realm realm, ImportParameters parameters)
        {
            base.PostImport(model, realm, parameters);

            populateUserDetails(model);
        }

        /// <summary>
        /// Legacy replays only store a username.
        /// This will populate a user ID during import.
        /// </summary>
        private void populateUserDetails(ScoreInfo model)
        {
            string username = model.RealmUser.Username;

            if (usernameLookupCache.TryGetValue(username, out var existing))
            {
                model.User = existing;
                return;
            }

            var userRequest = new GetUserRequest(username);

            api.Perform(userRequest);

            if (userRequest.Response is APIUser user)
            {
                usernameLookupCache.TryAdd(username, new APIUser
                {
                    // Because this is a permanent cache, let's only store the pieces we're interested in,
                    // rather than the full API response. If we start to store more than these three fields
                    // in realm, this should be undone.
                    Id = user.Id,
                    Username = user.Username,
                    CountryCode = user.CountryCode,
                });

                model.User = user;
            }
        }
    }
}
