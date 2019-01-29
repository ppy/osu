// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.MathUtils;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestCaseShaking : TestCaseHitCircle
    {
        public override void Add(Drawable drawable)
        {
            base.Add(drawable);

            if (drawable is TestDrawableHitCircle hitObject)
            {
                Scheduler.AddDelayed(() => hitObject.TriggerJudgement(),
                    hitObject.HitObject.StartTime - (hitObject.HitObject.HitWindows.HalfWindowFor(HitResult.Miss) + RNG.Next(0, 300)) - Time.Current);
            }
        }
    }
}
