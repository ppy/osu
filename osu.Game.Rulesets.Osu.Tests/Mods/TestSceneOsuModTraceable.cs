// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public class TestSceneOsuModTraceable : OsuModTestScene
    {
        [Test]
        public void TestDefaultBeatmapTest() => CreateModTest(new ModTestData
        {
            Mod = new OsuModTraceable(),
            Autoplay = true,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(206, 142)
                    },
                    new HitCircle
                    {
                        StartTime = 2000,
                        Position = new Vector2(306, 142)
                    },
                    new Slider
                    {
                        StartTime = 3000,
                        Position = new Vector2(156, 242),
                        Path = new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(200, 0), })
                    },
                    new Spinner
                    {
                        StartTime = 7000,
                        Position = new Vector2(256, 192),
                        EndTime = 8000,
                    }
                }
            },
            PassCondition = checkSomeHit
        });

        [Test]
        public void TestFadeOutEffect() => CreateModTest(new ModTestData
        {
            Mod = new OsuModTraceable { FadeOutEffect = { Value = true } },
            Autoplay = true,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HitCircle
                    {
                        StartTime = 1000,
                        Position = new Vector2(206, 142)
                    },
                    new HitCircle
                    {
                        StartTime = 2000,
                        Position = new Vector2(306, 142)
                    },
                    new Slider
                    {
                        StartTime = 3000,
                        Position = new Vector2(156, 242),
                        Path = new SliderPath(PathType.Linear, new[] { Vector2.Zero, new Vector2(200, 0), })
                    },
                }
            },
            PassCondition = checkSomeHit
        });

        private bool checkSomeHit() => Player.ScoreProcessor.JudgedHits >= 3;
    }
}
