// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuSliderJudgementResult : OsuJudgementResult
    {
        public readonly Stack<(double time, bool tracking)> TrackingHistory = new Stack<(double, bool)>();

        public OsuSliderJudgementResult(HitObject hitObject, Judgement judgement)
            : base(hitObject, judgement)
        {
            TrackingHistory.Push((double.NegativeInfinity, false));
        }
    }
}
