// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModAllCircles : OsuModTestScene
    {
        [Test]
        public void TestConversion() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAllCircles(),
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 8,
                        ApproachRate = 5,
                        OverallDifficulty = 9
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 1000 },
                    new Slider
                    {
                        StartTime = 2000,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }),
                        RepeatCount = 0,
                        SliderVelocityMultiplier = 10
                    },
                    new HitCircle { StartTime = 2500 },
                    new HitCircle { StartTime = 3000, NewCombo = true },
                    new Slider
                    {
                        StartTime = 4000,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(100))
                        }),
                        RepeatCount = 1,
                        SliderVelocityMultiplier = 10
                    },
                    new HitCircle { StartTime = 4500 },
                    new Slider
                    {
                        StartTime = 5000,
                        NewCombo = true,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }),
                        RepeatCount = 0,
                        SliderVelocityMultiplier = 10
                    },
                    new Slider
                    {
                        StartTime = 5000,
                        NewCombo = true,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }),
                        RepeatCount = 2,
                        SliderVelocityMultiplier = 10
                    },
                    new HitCircle { StartTime = 5500 },
                }
            },
            Autoplay = true,
            PassCondition = () =>
            {
                return Player.ScoreProcessor.Combo.Value == 9 && Player.ScoreProcessor.MaximumCombo == 9 && !this.ChildrenOfType<Slider>().Any();
            }
        });

        [Test]
        public void TestConversionWithEnds() => CreateModTest(new ModTestData
        {
            Mod = new OsuModAllCircles { ConvertEnds = { Value = true } },
            CreateBeatmap = () => new Beatmap
            {
                BeatmapInfo = new BeatmapInfo
                {
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 8,
                        ApproachRate = 5,
                        OverallDifficulty = 9
                    }
                },
                HitObjects = new List<HitObject>
                {
                    new HitCircle { StartTime = 1000 },
                    new Slider
                    {
                        StartTime = 2000,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }),
                        RepeatCount = 0,
                        SliderVelocityMultiplier = 10
                    },
                    new HitCircle { StartTime = 2500 },
                    new HitCircle { StartTime = 3000, NewCombo = true },
                    new Slider
                    {
                        StartTime = 4000,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(100))
                        }),
                        RepeatCount = 1,
                        SliderVelocityMultiplier = 10
                    },
                    new HitCircle { StartTime = 4500 },
                    new Slider
                    {
                        StartTime = 5000,
                        NewCombo = true,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }),
                        RepeatCount = 0,
                        SliderVelocityMultiplier = 10
                    },
                    new Slider
                    {
                        StartTime = 6000,
                        NewCombo = true,
                        Position = new Vector2(0),
                        Path = new SliderPath(new[]
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(0, 100))
                        }),
                        RepeatCount = 2,
                        SliderVelocityMultiplier = 10
                    },
                    new HitCircle { StartTime = 5500 },
                }
            },
            Autoplay = true,
            PassCondition = () =>
            {
                return Player.ScoreProcessor.Combo.Value == 16 && Player.ScoreProcessor.MaximumCombo == 16 && !this.ChildrenOfType<Slider>().Any();
            }
        });
    }
}
