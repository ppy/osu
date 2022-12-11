// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Rulesets.Taiko.Tests.Judgements;

namespace osu.Game.Rulesets.Taiko.Tests.Skinning
{
    public partial class TestSceneDrawableSwellExpireDelay : JudgementTest
    {
        [Test]
        public void TestExpireDelay()
        {
            const double swell_start = 1000;
            const double swell_duration = 1000;

            Swell swell = new Swell
            {
                StartTime = swell_start,
                Duration = swell_duration,
            };

            Hit hit = new Hit { StartTime = swell_start + swell_duration + 50 };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2100, TaikoAction.LeftCentre),
            };

            PerformTest(frames, CreateBeatmap(swell, hit));

            AssertResult<Hit>(0, HitResult.Ok);
        }
    }
}
