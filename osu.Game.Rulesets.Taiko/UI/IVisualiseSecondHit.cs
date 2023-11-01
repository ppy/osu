// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal interface IVisualiseSecondHit
    {
        void VisualiseSecondHit(JudgementResult? judgementResult);
    }
}
