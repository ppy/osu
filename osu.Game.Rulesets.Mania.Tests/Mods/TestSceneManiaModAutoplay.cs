// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Mods
{
    public partial class TestSceneManiaModAutoplay : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Test]
        public void TestPerfectScoreOnShortHoldNote()
        {
            CreateModTest(new ModTestData
            {
                Autoplay = true,
                Beatmap = new ManiaBeatmap(new StageDefinition(1))
                {
                    HitObjects = new List<ManiaHitObject>
                    {
                        new HoldNote
                        {
                            StartTime = 100,
                            EndTime = 100,
                        },
                        new HoldNote
                        {
                            StartTime = 100.1,
                            EndTime = 150,
                        },
                    }
                },
                PassCondition = () => Player.ScoreProcessor.Combo.Value == 4
            });
        }
    }
}
