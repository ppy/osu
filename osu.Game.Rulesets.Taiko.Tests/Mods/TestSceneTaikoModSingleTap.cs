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
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
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
    }
}
