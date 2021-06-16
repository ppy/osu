// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModCinema : ModCinema<CatchHitObject>
    {
        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods) => new Score
        {
            ScoreInfo = new ScoreInfo { User = new User { Username = "osu!salad!" } },
            Replay = new CatchAutoGenerator(beatmap).Generate(),
        };
    }
}
