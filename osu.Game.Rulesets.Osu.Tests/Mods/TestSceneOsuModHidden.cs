// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModHidden : ModTestScene
    {
        public TestSceneOsuModHidden()
            : base(new OsuRuleset())
        {
        }

        [Test]
        public void TestDefaultBeatmapTest() => CreateModTest(new ModTestData
        {
            Mod = new OsuModHidden(),
            Autoplay = true,
            PassCondition = checkSomeHit
        });

        [Test]
        public void FirstCircleAfterTwoSpinners() => CreateModTest(new ModTestData
        {
            Mod = new OsuModHidden(),
            Autoplay = true,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        EndTime = 1000,
                    },
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        StartTime = 1200,
                        EndTime = 2200,
                    },
                    new HitCircle
                    {
                        Position = new Vector2(300, 192),
                        StartTime = 3200,
                    },
                    new HitCircle
                    {
                        Position = new Vector2(384, 192),
                        StartTime = 4200,
                    }
                }
            },
            PassCondition = checkSomeHit
        });

        [Test]
        public void FirstSliderAfterTwoSpinners() => CreateModTest(new ModTestData
        {
            Mod = new OsuModHidden(),
            Autoplay = true,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        EndTime = 1000,
                    },
                    new Spinner
                    {
                        Position = new Vector2(256, 192),
                        StartTime = 1200,
                        EndTime = 2200,
                    },
                    new Slider
                    {
                        StartTime = 3200,
                        Path = new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(100, 0), })
                    },
                    new Slider
                    {
                        StartTime = 5200,
                        Path = new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(100, 0), })
                    }
                }
            },
            PassCondition = checkSomeHit
        });

        private bool checkSomeHit()
        {
            return Player.ScoreProcessor.JudgedHits >= 4;
        }
    }
}
