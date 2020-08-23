// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public class TestSceneCatchModRelax : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestModRelax() => CreateModTest(new ModTestData
        {
            Mod = new CatchModRelax(),
            Autoplay = false,
            PassCondition = () =>
            {
                var playfield = this.ChildrenOfType<CatchPlayfield>().Single();
                InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre);

                return Player.ScoreProcessor.Combo.Value > 0;
            },
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Fruit
                    {
                        X = CatchPlayfield.CENTER_X
                    }
                }
            }
        });
    }
}
