// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Tests
{
    public partial class TestSceneShaking : TestSceneHitCircle
    {
        private readonly List<ScheduledDelegate> scheduledTasks = new List<ScheduledDelegate>();

        protected override IBeatmap CreateBeatmapForSkinProvider()
        {
            // best way to run cleanup before a new step is run
            foreach (var task in scheduledTasks)
                task.Cancel();

            scheduledTasks.Clear();

            return base.CreateBeatmapForSkinProvider();
        }

        protected override TestDrawableHitCircle CreateDrawableHitCircle(HitCircle circle, bool auto, double hitOffset = 0)
        {
            var drawableHitObject = base.CreateDrawableHitCircle(circle, auto, hitOffset);

            Debug.Assert(drawableHitObject.HitObject.HitWindows != null);

            double delay = drawableHitObject.HitObject.StartTime - (drawableHitObject.HitObject.HitWindows.WindowFor(HitResult.Miss) + RNG.Next(0, 300)) - Time.Current;
            scheduledTasks.Add(Scheduler.AddDelayed(() => drawableHitObject.TriggerJudgement(), delay));

            return drawableHitObject;
        }
    }
}
