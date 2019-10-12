// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneShaking : TestSceneHitCircle
    {
        protected override TestDrawableHitCircle CreateDrawableHitCircle(HitCircle circle, bool auto)
        {
            var drawableHitObject = base.CreateDrawableHitCircle(circle, auto);

            Debug.Assert(drawableHitObject.HitObject.HitWindows != null);

            double delay = drawableHitObject.HitObject.StartTime - (drawableHitObject.HitObject.HitWindows.WindowFor(HitResult.Miss) + RNG.Next(0, 300)) - Time.Current;
            Scheduler.AddDelayed(() => drawableHitObject.TriggerJudgement(), delay);

            return drawableHitObject;
        }
    }
}
