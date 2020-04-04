﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    internal class OsuScoreProcessor : ScoreProcessor
    {
        protected override JudgementResult CreateResult(HitObject hitObject, Judgement judgement) => new OsuJudgementResult(hitObject, judgement);

        public override HitWindows CreateHitWindows() => new OsuHitWindows();
    }
}
