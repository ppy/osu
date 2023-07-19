// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Judgements
{
    public partial class TestSceneSwellJudgements : JudgementTest
    {
        [Test]
        public void TestHitAllSwell()
        {
            const double hit_time = 1000;

            Swell swell = new Swell
            {
                StartTime = hit_time,
                Duration = 1000,
                RequiredHits = 10
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2001),
            };

            for (int i = 0; i < swell.RequiredHits; i++)
            {
                double frameTime = 1000 + i * 50;
                frames.Add(new TaikoReplayFrame(frameTime, i % 2 == 0 ? TaikoAction.LeftCentre : TaikoAction.LeftRim));
                frames.Add(new TaikoReplayFrame(frameTime + 10));
            }

            PerformTest(frames, CreateBeatmap(swell));

            AssertJudgementCount(11);

            for (int i = 0; i < swell.RequiredHits; i++)
                AssertResult<SwellTick>(i, HitResult.IgnoreHit);

            AssertResult<Swell>(0, HitResult.LargeBonus);
        }

        [Test]
        public void TestHitSomeSwell()
        {
            const double hit_time = 1000;

            Swell swell = new Swell
            {
                StartTime = hit_time,
                Duration = 1000,
                RequiredHits = 10
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2001),
            };

            for (int i = 0; i < swell.RequiredHits / 2; i++)
            {
                double frameTime = 1000 + i * 50;
                frames.Add(new TaikoReplayFrame(frameTime, i % 2 == 0 ? TaikoAction.LeftCentre : TaikoAction.LeftRim));
                frames.Add(new TaikoReplayFrame(frameTime + 10));
            }

            PerformTest(frames, CreateBeatmap(swell));

            AssertJudgementCount(11);

            for (int i = 0; i < swell.RequiredHits / 2; i++)
                AssertResult<SwellTick>(i, HitResult.IgnoreHit);
            for (int i = swell.RequiredHits / 2; i < swell.RequiredHits; i++)
                AssertResult<SwellTick>(i, HitResult.IgnoreMiss);

            AssertResult<Swell>(0, HitResult.IgnoreMiss);
        }

        [Test]
        public void TestHitNoneSwell()
        {
            const double hit_time = 1000;

            Swell swell = new Swell
            {
                StartTime = hit_time,
                Duration = 1000,
                RequiredHits = 10
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(2001),
            };

            PerformTest(frames, CreateBeatmap(swell));

            AssertJudgementCount(11);

            for (int i = 0; i < swell.RequiredHits; i++)
                AssertResult<SwellTick>(i, HitResult.IgnoreMiss);

            AssertResult<Swell>(0, HitResult.IgnoreMiss);

            AddAssert("all tick offsets are 0", () => JudgementResults.Where(r => r.HitObject is SwellTick).All(r => r.TimeOffset == 0));
        }

        /// <summary>
        /// Ensure input is correctly sent to subsequent hits if a swell is fully completed.
        /// </summary>
        [Test]
        public void TestHitSwellThenHitHit()
        {
            const double swell_time = 1000;
            const double hit_time = 1150;

            Swell swell = new Swell
            {
                StartTime = swell_time,
                Duration = 100,
                RequiredHits = 1
            };

            Hit hit = new Hit
            {
                StartTime = hit_time
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(swell_time, TaikoAction.LeftRim),
                new TaikoReplayFrame(hit_time, TaikoAction.RightCentre),
            };

            PerformTest(frames, CreateBeatmap(swell, hit));

            AssertJudgementCount(3);

            AssertResult<SwellTick>(0, HitResult.IgnoreHit);
            AssertResult<Swell>(0, HitResult.LargeBonus);
            AssertResult<Hit>(0, HitResult.Great);
        }

        [Test]
        public void TestMissSwellThenHitHit()
        {
            const double swell_time = 1000;
            const double hit_time = 1150;

            Swell swell = new Swell
            {
                StartTime = swell_time,
                Duration = 100,
                RequiredHits = 1
            };

            Hit hit = new Hit
            {
                StartTime = hit_time
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0),
                new TaikoReplayFrame(hit_time, TaikoAction.RightCentre),
            };

            PerformTest(frames, CreateBeatmap(swell, hit));

            AssertJudgementCount(3);

            AssertResult<SwellTick>(0, HitResult.IgnoreMiss);
            AssertResult<Swell>(0, HitResult.IgnoreMiss);
            AssertResult<Hit>(0, HitResult.Great);
        }
    }
}
