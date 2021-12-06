// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Models;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using Realms;

#nullable enable

namespace osu.Game.Stores
{
    /// <summary>
    /// Handles the storage and retrieval of Scores/WorkingScores.
    /// </summary>
    [ExcludeFromDynamicCompile]
    public class ScoreImporter : RealmArchiveModelImporter<ScoreInfo>
    {
        private readonly RealmRulesetStore rulesets;
        private readonly BeatmapManager beatmaps;

        public override IEnumerable<string> HandledExtensions => new[] { ".osr" };

        protected override string[] HashableFileTypes => new[] { ".osr" };

        public ScoreImporter(RealmRulesetStore rulesets, RealmContextFactory contextFactory, Storage storage, BeatmapManager beatmaps)
            : base(storage, contextFactory)
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
                    // TODO: make work.
                    // return new DatabasedLegacyScoreDecoder(rulesets, beatmaps).Parse(stream).ScoreInfo;
                    return new ScoreInfo();
                }
                catch (LegacyScoreDecoder.BeatmapNotFoundException e)
                {
                    Logger.Log(e.Message, LoggingTarget.Information, LogLevel.Error);
                    return null;
                }
            }
        }

        protected override Task Populate(ScoreInfo model, ArchiveReader? archive, Realm realm, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
