// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Judgements
{
    public class TestSceneDrumRollJudgements : JudgementTest
    {
        [Test]
        public void TestHitAllDrumRoll()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1001),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000
            }));

            AssertJudgementCount(3);
            AssertResult<DrumRollTick>(0, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeDrumRoll()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000
            }));

            AssertJudgementCount(3);
            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitNoneDrumRoll()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000
            }));

            AssertJudgementCount(3);
            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(1, HitResult.IgnoreMiss);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitAllStrongDrumRollWithOneKey()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1001),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000,
                IsStrong = true
            }));

            AssertJudgementCount(6);

            AssertResult<DrumRollTick>(0, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(0, HitResult.LargeBonus);

            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(1, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(2, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeStrongDrumRollWithOneKey()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000,
                IsStrong = true
            }));

            AssertJudgementCount(6);

            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);

            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(1, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(2, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitAllStrongDrumRollWithBothKeys()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(1001),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000,
                IsStrong = true
            }));

            AssertJudgementCount(6);

            AssertResult<DrumRollTick>(0, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(0, HitResult.LargeBonus);

            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(1, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(2, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeStrongDrumRollWithBothKeys()
        {
            const double hit_time = 1000;

            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(new DrumRoll
            {
                StartTime = hit_time,
                Duration = 1000,
                IsStrong = true
            }));

            AssertJudgementCount(6);

            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);

            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(1, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(2, HitResult.IgnoreHit);
        }
    }
}
