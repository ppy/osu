// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    /// <summary>
    /// Used as a nested hitobject to provide <see cref="TaikoStrongJudgement"/>s for <see cref="DrawableTaikoHitObject"/>s.
    /// </summary>
    public abstract class DrawableStrongNestedHit : DrawableTaikoHitObject
    {
        public readonly DrawableHitObject MainObject;

        protected DrawableStrongNestedHit(StrongHitObject strong, DrawableHitObject mainObject)
            : base(strong)
        {
            MainObject = mainObject;
        }
    }
}
