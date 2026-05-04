// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModPreciseTapping : OsuModTestScene
    {
        [Test]
        public void TestPressWithoutHitMissesNextObject() => CreateModTest(new ModTestData
        {
            Mod = new OsuModPreciseTapping(),
            PassCondition = () => Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss) == 1,
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
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
                new OsuReplayFrame(250, new Vector2(300, 100), OsuAction.LeftButton),
                new OsuReplayFrame(251, new Vector2(300, 100)),
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
            }
        });

        [Test]
        public void TestExtraPressDoesNotChainMissNextObject() => CreateModTest(new ModTestData
        {
            Mod = new OsuModPreciseTapping(),
            PassCondition = () => Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss) == 1
                                  && Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Great) == 1,
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
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
                        StartTime = 700,
                        Position = new Vector2(200, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(400, new Vector2(300, 300), OsuAction.LeftButton),
                new OsuReplayFrame(401, new Vector2(300, 300)),
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(100)),
                new OsuReplayFrame(700, new Vector2(200, 100), OsuAction.LeftButton),
                new OsuReplayFrame(701, new Vector2(200, 100)),
            }
        });

        [Test]
        public void TestExtraPressDuringSliderMissesNextObject() => CreateModTest(new ModTestData
        {
            Mod = new OsuModPreciseTapping(),
            PassCondition = () => Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss) == 1,
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 500,
                        Position = new Vector2(100),
                        Path = new SliderPath(PathType.LINEAR, new[]
                        {
                            Vector2.Zero,
                            new Vector2(100, 0),
                        }),
                    },
                    new HitCircle
                    {
                        StartTime = 1500,
                        Position = new Vector2(300, 100),
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(500, new Vector2(100), OsuAction.LeftButton),
                new OsuReplayFrame(700, new Vector2(150, 100), OsuAction.LeftButton, OsuAction.RightButton),
                new OsuReplayFrame(701, new Vector2(150, 100), OsuAction.LeftButton),
                new OsuReplayFrame(900, new Vector2(200, 100)),
                new OsuReplayFrame(1500, new Vector2(300, 100), OsuAction.LeftButton),
                new OsuReplayFrame(1501, new Vector2(300, 100)),
            }
        });

        [Test]
        public void TestPressBlockedByAlternateIsNotCountedAsExtra() => CreateModTest(new ModTestData
        {
            Mods = new Mod[] { new OsuModAlternate(), new OsuModPreciseTapping() },
            PassCondition = () => Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Great) == 2
                                  && Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss) == 0,
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
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
                new OsuReplayFrame(700, new Vector2(150, 100), OsuAction.LeftButton),
                new OsuReplayFrame(701, new Vector2(150, 100)),
                new OsuReplayFrame(1000, new Vector2(200, 100), OsuAction.RightButton),
                new OsuReplayFrame(1001, new Vector2(200, 100)),
            }
        });
    }
}
