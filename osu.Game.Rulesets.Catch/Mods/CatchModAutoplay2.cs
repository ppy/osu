// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Catch.Mods
{
    public class CatchModAutoplay2 : ModAutoplay<CatchHitObject>
    {
        public override string Name => "Autoplay2";
        public override string Acronym => "A2";
        public override Type[] IncompatibleMods => new[] { typeof(CatchModRelax), typeof(CatchModSuddenDeath), typeof(CatchModNoFail), typeof(CatchModAutoplay) };

        public override Score CreateReplayScore(IBeatmap beatmap, IReadOnlyList<Mod> mods)
        {
            return new Score
            {
                Replay = new CatchAutoGenerator2((Beatmap<CatchHitObject>)beatmap).Generate(),
            };
        }
    }
}
