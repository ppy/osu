// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Replays;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModSuddenDeath : ModFailConditionTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        public TestSceneManiaModSuddenDeath()
            : base(new ManiaModSuddenDeath())
        {
        }

        [Test]
        public void TestGreatHit() => CreateModTest(new ModTestData
        {
            Mod = new ManiaModSuddenDeath(),
            PassCondition = () => ((ModFailConditionTestPlayer)Player).CheckFailed(false),
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Note
                    {
                        StartTime = 1000,
                    }
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new ManiaReplayFrame(1020, ManiaAction.Key1),
                new ManiaReplayFrame(2000)
            }
        });

        [Test]
        public void TestBreakOnHoldNote() => CreateModTest(new ModTestData
        {
            Mod = new ManiaModSuddenDeath(),
            PassCondition = () => ((ModFailConditionTestPlayer)Player).CheckFailed(true) && Player.Results.Count == 2,
            Autoplay = false,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new HoldNote
                    {
                        StartTime = 1000,
                        EndTime = 3000,
                    },
                },
            },
            ReplayFrames = new List<ReplayFrame>
            {
                new ManiaReplayFrame(1000, ManiaAction.Key1),
                new ManiaReplayFrame(2000)
            }
        });
    }
}
