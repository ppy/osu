// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Pippidon.Replays;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Pippidon.Mods
{
    public class PippidonModAutoplay : ModAutoplay
    {
        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score
        {
            ScoreInfo = new ScoreInfo
            {
                User = new APIUser { Username = "sample" },
            },
            Replay = new PippidonAutoGenerator(beatmap).Generate(),
        };
    }
}
