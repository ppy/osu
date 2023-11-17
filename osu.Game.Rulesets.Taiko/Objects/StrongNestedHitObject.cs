// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
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
        public TaikoHitObject Parent { get; private set; }

        protected StrongNestedHitObject(TaikoHitObject parent)
        {
            Parent = parent;
        }

        public override Judgement CreateJudgement() => new TaikoStrongJudgement();

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not StrongNestedHitObject strongNestedHitObject)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(StrongNestedHitObject)}");

            Parent = (TaikoHitObject)strongNestedHitObject.Parent.DeepClone(referenceLookup);
        }

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
