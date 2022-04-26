// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
using osu.Game.Stores;
using Realms;

#nullable enable

namespace osu.Game.Scoring
{
    public class ScoreModelManager : RealmArchiveModelManager<ScoreInfo>
    {
        public override IEnumerable<string> HandledExtensions => new[] { ".osr" };

        protected override string[] HashableFileTypes => new[] { ".osr" };

        private readonly RulesetStore rulesets;
        private readonly Func<BeatmapManager> beatmaps;

        public ScoreModelManager(RulesetStore rulesets, Func<BeatmapManager> beatmaps, Storage storage, RealmAccess realm)
            : base(storage, realm)
        {
            this.rulesets = rulesets;
            this.beatmaps = beatmaps;
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

            if (string.IsNullOrEmpty(model.StatisticsJson))
                model.StatisticsJson = JsonConvert.SerializeObject(model.Statistics);
        }

        public override bool IsAvailableLocally(ScoreInfo model)
        {
            return Realm.Run(realm => realm.All<ScoreInfo>().Any(s => s.OnlineID == model.OnlineID));
        }
    }
}
