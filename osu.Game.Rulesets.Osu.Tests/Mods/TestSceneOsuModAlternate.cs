// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModAlternate : OsuModTestScene
    {
        [Test]
        public void TestInputAtIntro() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 1,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(200), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(200)),
                new OsuReplayFrame(1000, new Vector2(100), OsuAction.LeftButton),
            }
        });

        [Test]
        public void TestInputAlternating() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 4,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(200, 100),
                    },
                    new HitCircle
                    {
                        StartTime = 1500,
                        Position = new Vector2(300, 100),
                    },
                    new HitCircle
                    {
                        StartTime = 2000,
                        Position = new Vector2(400, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.RightButton),
                new OsuReplayFrame(1001, new Vector2(200, 100)),
                new OsuReplayFrame(1500, new Vector2(300, 100), OsuAction.LeftButton),
                new OsuReplayFrame(1501, new Vector2(300, 100)),
                new OsuReplayFrame(2000, new Vector2(400, 100), OsuAction.RightButton),
                new OsuReplayFrame(2001, new Vector2(400, 100)),
            }
        });

        [Test]
        public void TestInputSingular() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 0 && Player.ScoreProcessor.HighestCombo.Value == 1,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(200, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.LeftButton),
            }
        });

        [Test]
        public void TestInputSingularWithBreak() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAlternate(),
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                Breaks = new List<BreakPeriod>
                {
                    new BreakPeriod(500, 2250),
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                    },
                    new HitCircle
                    {
                        StartTime = 2500,
                        Position = new Vector2(100),
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(2500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(2501, new Vector2(100)),
            }
        });
    }
}
