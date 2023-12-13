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

        [Test]
        public void TestAtMostOneSwellTickJudgedPerFrame()
        {
            const double swell_time = 1000;

            Swell swell = new Swell
            {
                StartTime = swell_time,
                Duration = 1000,
                RequiredHits = 10
            };

            List<ReplayFrame> frames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(1000),
                new TaikoReplayFrame(1250, TaikoAction.LeftCentre, TaikoAction.LeftRim),
                new TaikoReplayFrame(1251),
                new TaikoReplayFrame(1500, TaikoAction.LeftCentre, TaikoAction.LeftRim, TaikoAction.RightCentre, TaikoAction.RightRim),
                new TaikoReplayFrame(1501),
                new TaikoReplayFrame(2000),
            };

            PerformTest(frames, CreateBeatmap(swell));

            AssertJudgementCount(11);

            // this is a charitable interpretation of the inputs.
            //
            // for the frame at time 1250, we only count either one of the input actions - simple.
            //
            // for the frame at time 1500, we give the user the benefit of the doubt,
            // and we ignore actions that wouldn't otherwise cause a hit due to not alternating,
            // but we still count one (just one) of the actions that _would_ normally cause a hit.
            // this is done as a courtesy to avoid stuff like key chattering after press blocking legitimate inputs.
            for (int i = 0; i < 2; i++)
                AssertResult<SwellTick>(i, HitResult.IgnoreHit);
            for (int i = 2; i < swell.RequiredHits; i++)
                AssertResult<SwellTick>(i, HitResult.IgnoreMiss);

            AssertResult<Swell>(0, HitResult.IgnoreMiss);
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
