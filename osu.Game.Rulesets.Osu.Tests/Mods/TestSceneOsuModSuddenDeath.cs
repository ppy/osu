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
    public partial class TestSceneOsuModSuddenDeath : ModFailConditionTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new OsuRuleset();

        public TestSceneOsuModSuddenDeath()
            : base(new OsuModSuddenDeath())
        {
        }

        [Test]
        public void TestMissTail() => CreateModTest(new ModTestData
        {
            Mod = new OsuModSuddenDeath(),
            PassCondition = () => ((ModFailConditionTestPlayer)Player).CheckFailed(false),
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

        [Test]
        public void TestMissTick() => CreateModTest(new ModTestData
        {
            Mod = new OsuModSuddenDeath(),
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
                        Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(200, 0), })
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
