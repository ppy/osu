// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModQuarterize : TaikoModTestScene
    {
        [Test]
        public void TestOneThirdConversion()
        {
            CreateModTest(new ModTestData
            {
                Mod = new TaikoModQuarterize
                {
                    OneThirdConversion = { Value = true },
                },
                Autoplay = false,
                CreateBeatmap = () => new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new Hit { StartTime = 1000, Type = HitType.Centre },
                        new Hit { StartTime = 1500, Type = HitType.Centre },
                        new Hit { StartTime = 2000, Type = HitType.Centre },
                        new Hit { StartTime = 2333, Type = HitType.Rim }, // mod removes this
                        new Hit { StartTime = 2666, Type = HitType.Centre }, // mod moves this to 2500
                        new Hit { StartTime = 3000, Type = HitType.Centre },
                        new Hit { StartTime = 3500, Type = HitType.Centre },
                    },
                },
                ReplayFrames = new List<ReplayFrame>
                {
                    new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(1200),
                    new TaikoReplayFrame(1500, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(1700),
                    new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(2200),
                    new TaikoReplayFrame(2500, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(2700),
                    new TaikoReplayFrame(3000, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(3200),
                    new TaikoReplayFrame(3500, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(3700),
                },
                PassCondition = () => Player.ScoreProcessor.Combo.Value == 6 && Player.ScoreProcessor.Accuracy.Value == 1
            });
        }

        [Test]
        public void TestOneSixthConversion() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModQuarterize
            {
                OneSixthConversion = { Value = true }
            },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit { StartTime = 1000, Type = HitType.Centre },
                    new Hit { StartTime = 1250, Type = HitType.Centre },
                    new Hit { StartTime = 1500, Type = HitType.Centre },
                    new Hit { StartTime = 1666, Type = HitType.Rim }, // mod removes this
                    new Hit { StartTime = 1833, Type = HitType.Centre }, // mod moves this to 1750
                    new Hit { StartTime = 2000, Type = HitType.Centre },
                    new Hit { StartTime = 2250, Type = HitType.Centre },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1200),
                new TaikoReplayFrame(1250, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1450),
                new TaikoReplayFrame(1500, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1600),
                new TaikoReplayFrame(1750, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1800),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2200),
                new TaikoReplayFrame(2250, TaikoAction.LeftCentre),
                new TaikoReplayFrame(2450),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 6 && Player.ScoreProcessor.Accuracy.Value == 1
        });

        [Test]
        public void TestOneEighthConversion() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModQuarterize
            {
                OneEighthConversion = { Value = true }
            },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit { StartTime = 1000, Type = HitType.Centre },
                    new Hit { StartTime = 1250, Type = HitType.Centre },
                    new Hit { StartTime = 1500, Type = HitType.Centre },
                    new Hit { StartTime = 1625, Type = HitType.Rim }, // mod removes this
                    new Hit { StartTime = 1750, Type = HitType.Centre },
                    new Hit { StartTime = 2000, Type = HitType.Centre },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(1000, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1200),
                new TaikoReplayFrame(1250, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1450),
                new TaikoReplayFrame(1500, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1700),
                new TaikoReplayFrame(1750, TaikoAction.LeftCentre),
                new TaikoReplayFrame(1900),
                new TaikoReplayFrame(2000, TaikoAction.LeftCentre),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 5 && Player.ScoreProcessor.Accuracy.Value == 1
        });
    }
}
