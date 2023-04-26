// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Replays;

namespace osu.Game.Rulesets.Taiko.Tests.Mods
{
    public partial class TestSceneTaikoModSingleTap : TaikoModTestScene
    {
        [Test]
        public void TestInputAlternate() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModSingleTap(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 500,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 700,
                        Type = HitType.Rim
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(100, TaikoAction.RightRim),
                new TaikoReplayFrame(120),
                new TaikoReplayFrame(300, TaikoAction.LeftRim),
                new TaikoReplayFrame(320),
                new TaikoReplayFrame(500, TaikoAction.RightRim),
                new TaikoReplayFrame(520),
                new TaikoReplayFrame(700, TaikoAction.LeftRim),
                new TaikoReplayFrame(720),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1
        });

        [Test]
        public void TestInputSameKey() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModSingleTap(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 500,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 700,
                        Type = HitType.Rim
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(100, TaikoAction.RightRim),
                new TaikoReplayFrame(120),
                new TaikoReplayFrame(300, TaikoAction.RightRim),
                new TaikoReplayFrame(320),
                new TaikoReplayFrame(500, TaikoAction.RightRim),
                new TaikoReplayFrame(520),
                new TaikoReplayFrame(700, TaikoAction.RightRim),
                new TaikoReplayFrame(720),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
        });

        [Test]
        public void TestInputIntro() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModSingleTap(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(0, TaikoAction.RightRim),
                new TaikoReplayFrame(20),
                new TaikoReplayFrame(100, TaikoAction.LeftRim),
                new TaikoReplayFrame(120),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1
        });

        [Test]
        public void TestInputStrong() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModSingleTap(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim,
                        IsStrong = true
                    },
                    new Hit
                    {
                        StartTime = 500,
                        Type = HitType.Rim,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(100, TaikoAction.RightRim),
                new TaikoReplayFrame(120),
                new TaikoReplayFrame(300, TaikoAction.LeftRim),
                new TaikoReplayFrame(320),
                new TaikoReplayFrame(500, TaikoAction.LeftRim),
                new TaikoReplayFrame(520),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 2
        });

        [Test]
        public void TestInputBreaks() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModSingleTap(),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(100, 1600),
                },
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Rim
                    },
                    new Hit
                    {
                        StartTime = 2000,
                        Type = HitType.Rim,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(100, TaikoAction.RightRim),
                new TaikoReplayFrame(120),
                // Press different key after break but before hit object.
                new TaikoReplayFrame(1900, TaikoAction.LeftRim),
                new TaikoReplayFrame(1820),
                // Press original key at second hitobject and ensure it has been hit.
                new TaikoReplayFrame(2000, TaikoAction.RightRim),
                new TaikoReplayFrame(2020),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2
        });
    }
}
