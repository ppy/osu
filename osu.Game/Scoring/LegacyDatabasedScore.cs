// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.IO.Stores;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Scoring.Legacy;

namespace osu.Game.Scoring
{
    public class LegacyDatabasedScore : Score
    {
        public LegacyDatabasedScore(ScoreInfo score, RulesetStore rulesets, BeatmapManager beatmaps, IResourceStore<byte[]> store)
        {
            ScoreInfo = score;

            var replayFilename = score.Files.First(f => f.Filename.EndsWith(".osr", StringComparison.InvariantCultureIgnoreCase)).FileInfo.StoragePath;

            using (var stream = store.GetStream(replayFilename))
                Replay = new DatabasedLegacyScoreParser(rulesets, beatmaps).Parse(stream).Replay;
        }
    }
}
