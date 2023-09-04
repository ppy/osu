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
    public partial class TestSceneDrumRollJudgements : JudgementTest
    {
        [Test]
        public void TestHitAllDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1001),
                new TaikoReplayFrame(1250, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1251),
                new TaikoReplayFrame(1500, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1501),
                new TaikoReplayFrame(1750, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1751),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(false)));

            AssertJudgementCount(6);
            AssertResult<DrumRollTick>(0, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(1, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(2, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(3, HitResult.SmallBonus);
            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(false)));

            AssertJudgementCount(6);
            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(1, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(2, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(3, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitNoneDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
            }, CreateBeatmap(createDrumRoll(false)));

            AssertJudgementCount(6);
            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(1, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(2, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(3, HitResult.IgnoreMiss);
            AssertResult<DrumRollTick>(4, HitResult.IgnoreMiss);
            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitNoneStrongDrumRoll()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            for (int i = 0; i < 5; ++i)
            {
                AssertResult<DrumRollTick>(i, HitResult.IgnoreMiss);
                AssertResult<DrumRollTick.StrongNestedHit>(i, HitResult.IgnoreMiss);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitAllStrongDrumRollWithOneKey()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1001),
                new TaikoReplayFrame(1250, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1251),
                new TaikoReplayFrame(1500, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1501),
                new TaikoReplayFrame(1750, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1751),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            for (int i = 0; i < 5; i++)
            {
                AssertResult<DrumRollTick>(i, HitResult.SmallBonus);
                AssertResult<StrongNestedHitObject>(i, HitResult.LargeBonus);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeStrongDrumRollWithOneKey()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);

            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(4, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitAllStrongDrumRollWithBothKeys()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(1001),
                new TaikoReplayFrame(1250, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(1251),
                new TaikoReplayFrame(1500, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(1501),
                new TaikoReplayFrame(1750, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(1751),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            for (int i = 0; i < 5; i++)
            {
                AssertResult<DrumRollTick>(i, HitResult.SmallBonus);
                AssertResult<StrongNestedHitObject>(i, HitResult.LargeBonus);
            }

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        [Test]
        public void TestHitSomeStrongDrumRollWithBothKeys()
        {
            PerformTest(new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre, TaikoAction.RightCentre),
                new TaikoReplayFrame(2001),
            }, CreateBeatmap(createDrumRoll(true)));

            AssertJudgementCount(12);

            AssertResult<DrumRollTick>(0, HitResult.IgnoreMiss);
            AssertResult<StrongNestedHitObject>(0, HitResult.IgnoreMiss);

            AssertResult<DrumRollTick>(4, HitResult.SmallBonus);
            AssertResult<StrongNestedHitObject>(4, HitResult.LargeBonus);

            AssertResult<DrumRoll>(0, HitResult.IgnoreHit);
            AssertResult<StrongNestedHitObject>(5, HitResult.IgnoreHit);
        }

        private DrumRoll createDrumRoll(bool strong) => new DrumRoll
        {
            StartTime = 1000,
            Duration = 1000,
            IsStrong = strong
        };
    }
}
