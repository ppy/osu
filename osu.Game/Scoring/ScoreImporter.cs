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
            if (model.BeatmapInfo == null) throw new ArgumentNullException(nameof(model.BeatmapInfo));
            if (model.Ruleset == null) throw new ArgumentNullException(nameof(model.Ruleset));

            PopulateMaximumStatistics(model);

            if (string.IsNullOrEmpty(model.StatisticsJson))
                model.StatisticsJson = JsonConvert.SerializeObject(model.Statistics);

            if (string.IsNullOrEmpty(model.MaximumStatisticsJson))
                model.MaximumStatisticsJson = JsonConvert.SerializeObject(model.MaximumStatistics);
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
                                                      .Where(h => h.IsBasic())
                                                      .OrderByDescending(Judgement.ToNumericResult).First();

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

        protected override void PostImport(ScoreInfo model, Realm realm, bool batchImport)
        {
            base.PostImport(model, realm, batchImport);

            var userRequest = new GetUserRequest(model.RealmUser.Username);

            api.Perform(userRequest);

            if (userRequest.Response is APIUser user)
                model.User = user;
        }
    }
}
