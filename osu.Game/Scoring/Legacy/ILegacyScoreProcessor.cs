// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Scoring.Legacy
{
    public interface ILegacyScoreProcessor
    {
        void ApplyBeatmap(IBeatmap beatmap);
        void ApplyMods(IReadOnlyList<Mod> mods);
        long GetScoreForResult(JudgementResult result);
    }
}
