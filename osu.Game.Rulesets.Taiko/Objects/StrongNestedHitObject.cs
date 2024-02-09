// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    /// <summary>
    /// Base type for nested strong hits.
    /// Used by <see cref="TaikoStrongableHitObject"/>s to represent their strong bonus scoring portions.
    /// </summary>
    public abstract class StrongNestedHitObject : TaikoHitObject
    {
        public readonly TaikoHitObject Parent;

        protected StrongNestedHitObject(TaikoHitObject parent)
        {
            Parent = parent;
        }

        protected override Judgement CreateJudgement() => new TaikoStrongJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
