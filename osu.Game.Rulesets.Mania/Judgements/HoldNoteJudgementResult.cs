// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Objects;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class HoldNoteJudgementResult : JudgementResult
    {
        private Stack<(double time, bool holding)> holdingState { get; } = new Stack<(double, bool)>();

        public HoldNoteJudgementResult(HoldNote hitObject, Judgement judgement)
            : base(hitObject, judgement)
        {
            holdingState.Push((double.NegativeInfinity, false));
        }

        private (double time, bool holding) getLastReport(double currentTime)
        {
            while (holdingState.Peek().time > currentTime)
                holdingState.Pop();

            return holdingState.Peek();
        }

        public bool IsHolding(double currentTime) => getLastReport(currentTime).holding;

        public bool DroppedHoldAfter(double time)
        {
            foreach (var state in holdingState)
            {
                if (state.time >= time && !state.holding)
                    return true;
            }

            return false;
        }

        public void ReportHoldState(double currentTime, bool holding)
        {
            var lastReport = getLastReport(currentTime);
            if (holding != lastReport.holding)
                holdingState.Push((currentTime, holding));
        }
    }
}
