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
    public partial class TestSceneTaikoModAlternate : TaikoModTestScene
    {
        [Test]
        public void TestInputAlternateHands() => CreateModTest(new ModTestData
        {
            Mod = new TaikoModAlternate { UserPlaystyle = { Value = TaikoModAlternate.Playstyle.KDDK } },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
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
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
        });

        [Test]
        public void TestInputSameKey([Values] TaikoModAlternate.Playstyle playstyle) => CreateModTest(new ModTestData
        {
            Mod = new TaikoModAlternate { UserPlaystyle = { Value = playstyle } },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
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
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1
        });

        [Test]
        public void TestInputIntro([Values] TaikoModAlternate.Playstyle playstyle) => CreateModTest(new ModTestData
        {
            Mod = new TaikoModAlternate { UserPlaystyle = { Value = playstyle } },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
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
                new TaikoReplayFrame(100, TaikoAction.RightRim),
                new TaikoReplayFrame(120),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1
        });

        [Test]
        public void TestInputStrong([Values] TaikoModAlternate.Playstyle playstyle) => CreateModTest(new ModTestData
        {
            Mod = new TaikoModAlternate { UserPlaystyle = { Value = playstyle } },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Hit
                    {
                        StartTime = 100,
                        Type = HitType.Centre
                    },
                    new Hit
                    {
                        StartTime = 300,
                        Type = HitType.Rim,
                        IsStrong = true
                    }
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(100, TaikoAction.RightCentre),
                new TaikoReplayFrame(120),
                // Ensure you can hit strong hits starting with the same hand.
                new TaikoReplayFrame(300, TaikoAction.RightRim),
                new TaikoReplayFrame(310, TaikoAction.LeftRim),
                new TaikoReplayFrame(320),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2
        });

        [Test]
        public void TestInputSpecialObjects([Values] TaikoModAlternate.Playstyle playstyle) => CreateModTest(new ModTestData
        {
            Mod = new TaikoModAlternate { UserPlaystyle = { Value = playstyle } },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    // Strong
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

                    // Swell
                    new Swell
                    {
                        StartTime = 700,
                        EndTime = 900,
                        RequiredHits = 2
                    },
                    new Hit
                    {
                        StartTime = 1000,
                        Type = HitType.Rim,
                    },

                    // Drumroll
                    new DrumRoll
                    {
                        StartTime = 1100,
                        EndTime = 1400,
                    },
                    new Hit
                    {
                        StartTime = 1500,
                        Type = HitType.Rim,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                // Ensure strongs reset handedness.
                new TaikoReplayFrame(300, TaikoAction.LeftRim),
                new TaikoReplayFrame(310, TaikoAction.RightRim),
                new TaikoReplayFrame(320),
                new TaikoReplayFrame(500, TaikoAction.RightRim),
                new TaikoReplayFrame(520),

                // Ensure swells reset handedness.
                new TaikoReplayFrame(700, TaikoAction.RightCentre),
                new TaikoReplayFrame(710),
                new TaikoReplayFrame(800, TaikoAction.RightRim),
                new TaikoReplayFrame(810),
                new TaikoReplayFrame(1000, TaikoAction.RightRim),
                new TaikoReplayFrame(1010),

                // Ensure drumrolls reset handedness.
                new TaikoReplayFrame(1100, TaikoAction.RightCentre),
                new TaikoReplayFrame(1110),
                new TaikoReplayFrame(1300, TaikoAction.RightRim),
                new TaikoReplayFrame(1310),
                new TaikoReplayFrame(1500, TaikoAction.RightRim),
                new TaikoReplayFrame(1510),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
        });

        [Test]
        public void TestInputBreaks([Values] TaikoModAlternate.Playstyle playstyle) => CreateModTest(new ModTestData
        {
            Mod = new TaikoModAlternate { UserPlaystyle = { Value = playstyle } },
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                Breaks =
                {
                    new BreakPeriod(100, 1300),
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
                        StartTime = 1500,
                        Type = HitType.Rim,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new TaikoReplayFrame(100, TaikoAction.RightRim),
                new TaikoReplayFrame(120),
                // Press same key after break but before hit object.
                new TaikoReplayFrame(1400, TaikoAction.RightRim),
                new TaikoReplayFrame(1420),
                // Press same key again at second hitobject and ensure the break has reset handedness.
                new TaikoReplayFrame(1500, TaikoAction.RightRim),
                new TaikoReplayFrame(1520),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2
        });

        [Test]
        public void TestInputDDKK()
        {
            CreateModTest(new ModTestData
            {
                Mod = new TaikoModAlternate { UserPlaystyle = { Value = TaikoModAlternate.Playstyle.DDKK } },
                Autoplay = false,
                CreateBeatmap = () => new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new Hit
                        {
                            StartTime = 100,
                            Type = HitType.Centre
                        },
                        new Hit
                        {
                            StartTime = 300,
                            Type = HitType.Rim
                        },
                        new Hit
                        {
                            StartTime = 500,
                            Type = HitType.Centre
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
                    new TaikoReplayFrame(100, TaikoAction.LeftCentre),
                    new TaikoReplayFrame(120),
                    new TaikoReplayFrame(300, TaikoAction.LeftRim),
                    new TaikoReplayFrame(320),
                    new TaikoReplayFrame(500, TaikoAction.RightCentre),
                    new TaikoReplayFrame(520),
                    new TaikoReplayFrame(700, TaikoAction.RightRim),
                    new TaikoReplayFrame(720),
                },
                PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
            });
        }
    }
}
