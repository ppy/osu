// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Osu.Tests.Mods
{
    public partial class TestSceneOsuModPerfect : ModFailConditionTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        public TestSceneOsuModPerfect()
            : base(new OsuModPerfect())
        {
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestHitCircle(bool shouldMiss) => CreateHitObjectTest(new HitObjectTestData(new HitCircle { StartTime = 1000 }), shouldMiss);

        [TestCase(false)]
        [TestCase(true)]
        public void TestSlider(bool shouldMiss)
        {
            var slider = new Slider
            {
                StartTime = 1000,
                Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(100, 0), })
            };

            CreateHitObjectTest(new HitObjectTestData(slider), shouldMiss);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestSpinner(bool shouldMiss)
        {
            var spinner = new Spinner
            {
                StartTime = 1000,
                EndTime = 3000,
                Position = new Vector2(256, 192)
            };

            CreateHitObjectTest(new HitObjectTestData(spinner), shouldMiss);
        }

        [Test]
        public void TestMissSliderTail() => CreateModTest(new ModTestData
        {
            Mod = new OsuModPerfect(),
            PassCondition = () => ((ModFailConditionTestPlayer)Player).CheckFailed(true),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Slider
                    {
                        Position = new Vector2(256, 192),
                        StartTime = 1000,
                        Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(100, 0), })
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new OsuReplayFrame(1000, new Vector2(256, 192), OsuAction.LeftButton),
                new OsuReplayFrame(1001, new Vector2(256, 192)),
            }
        });
    }
}
