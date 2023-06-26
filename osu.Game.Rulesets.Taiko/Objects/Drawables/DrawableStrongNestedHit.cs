// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// Used as a nested hitobject to provide <see cref="TaikoStrongJudgement"/>s for <see cref="DrawableTaikoStrongableHitObject{TObject,TStrongNestedObject}"/>s.
    /// </summary>
    public abstract partial class DrawableStrongNestedHit : DrawableTaikoHitObject
    {
        public new DrawableTaikoHitObject? ParentHitObject => base.ParentHitObject as DrawableTaikoHitObject;

        protected DrawableStrongNestedHit(StrongNestedHitObject? nestedHit)
            : base(nestedHit)
        {
        }
    }
}
