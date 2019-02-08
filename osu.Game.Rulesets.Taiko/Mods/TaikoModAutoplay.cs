﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Scoring;
using osu.Game.Users;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModAutoplay : ModAutoplay<TaikoHitObject>
    {
        protected override Score CreateReplayScore(Beatmap<TaikoHitObject> beatmap) => new Score
        {
            ScoreInfo = new ScoreInfo { User = new User { Username = "mekkadosu!" } },
            Replay = new TaikoAutoGenerator(beatmap).Generate(),
        };
    }
}
