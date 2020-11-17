// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A skinnable judgement element which supports playing an animation from the current point in time.
    /// </summary>
    public interface IAnimatableJudgement
    {
        void PlayAnimation(HitResult result);
    }
}
