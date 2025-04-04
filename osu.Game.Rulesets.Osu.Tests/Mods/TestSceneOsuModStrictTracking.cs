// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModStrictTracking : OsuModTestScene
    {
        [Test]
        public void TestSliderInput() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        }
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2(), OsuAction.LeftButton),
                new OsuReplayFrame(500, new Vector2(200, 0), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(200, 0)),
                new OsuReplayFrame(1000, new Vector2(), OsuAction.LeftButton),
                new OsuReplayFrame(1750, new Vector2(0, 100), OsuAction.LeftButton),
                new OsuReplayFrame(1751, new Vector2(0, 100)),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2
        });

        [Test]
        public void TestReleaseOverSliderBeforeHeadHit() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        }
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(501, new Vector2(200, 0), OsuAction.LeftButton),
                new OsuReplayFrame(1000, new Vector2()),
                new OsuReplayFrame(1020, new Vector2(), OsuAction.LeftButton),
                new OsuReplayFrame(1750, new Vector2(0, 100), OsuAction.LeftButton),
                new OsuReplayFrame(1751, new Vector2(0, 100)),
            },
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 2
        });

        [Test]
        public void TestDoNothing() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        }
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2()),
            },
            PassCondition = () => Player.ScoreProcessor.JudgedHits == 3 && Player.ScoreProcessor.Statistics.All(s => s.Key.IsMiss())
        });

        [Test]
        public void TestMissHeadButTrackRestOfSlider() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        }
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2()),
                new OsuReplayFrame(1375, new Vector2(0, 50), OsuAction.LeftButton),
                new OsuReplayFrame(1751, new Vector2(0, 100)),
            },
            PassCondition = () => Player.ScoreProcessor.JudgedHits == 3 && Player.ScoreProcessor.Statistics[HitResult.Miss] == 1 && Player.ScoreProcessor.Statistics[HitResult.Great] == 1
        });

        [Test]
        public void TestMissHeadThenDropTracking() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        }
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2()),
                new OsuReplayFrame(1375, new Vector2(0, 50), OsuAction.LeftButton),
                new OsuReplayFrame(1475, new Vector2(), OsuAction.LeftButton),
            },
            PassCondition = () => Player.ScoreProcessor.JudgedHits == 3 && Player.ScoreProcessor.Statistics.All(s => s.Key.IsMiss())
        });

        [Test]
        public void TestLosingTrackingMissesTail() => CreateModTest(new ModTestData
        {
            Mod = new OsuModStrictTracking(),
            Autoplay = false,
            CreateBeatmap = () => new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        }
                    }
                }
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(0, new Vector2(), OsuAction.LeftButton),
                new OsuReplayFrame(500, new Vector2(200, 0), OsuAction.LeftButton),
                new OsuReplayFrame(501, new Vector2(200, 0)),
                new OsuReplayFrame(1000, new Vector2(), OsuAction.LeftButton),
            },
            PassCondition = () => Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss) == 1
        });
    }
}
