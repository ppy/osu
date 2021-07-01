// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.EmptyScrolling.Replays;
using osu.Game.Scoring;
using osu.Game.Users;
using System.Collections.Generic;

namespace osu.Game.Rulesets.EmptyScrolling.Mods
{
    public class EmptyScrollingModAutoplay : ModAutoplay
    {
        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score
        {
            ScoreInfo = new ScoreInfo
            {
                User = new User { Username = "sample" },
            },
            Replay = new EmptyScrollingAutoGenerator(beatmap).Generate(),
        };
    }
}
