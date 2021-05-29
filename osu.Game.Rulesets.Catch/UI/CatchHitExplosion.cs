// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Catch.UI
{
    /// <summary>
    /// The base for hit explosion types used with <see cref="PoolableHitExplosion"/>.
    /// </summary>
    public abstract class CatchHitExplosion : CompositeDrawable
    {
        public override bool RemoveWhenNotAlive => false;
        public override bool RemoveCompletedTransforms => false;

        [Resolved]
        private Bindable<JudgementResult> judgementResult { get; set; }

        public abstract void Animate();

        public void RunAnimation()
        {
            var resultTime = judgementResult.Value.TimeAbsolute;

            LifetimeStart = resultTime;

            ApplyTransformsAt(double.MinValue, true);
            ClearTransforms(true);

            using (BeginAbsoluteSequence(resultTime))
            {
                Animate();
            }

            LifetimeEnd = LatestTransformEndTime;
        }
    }
}
