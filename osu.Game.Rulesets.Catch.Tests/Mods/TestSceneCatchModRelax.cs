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
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;

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
            PassCondition = passCondition,
            Beatmap = new Beatmap
            {
                HitObjects = new List<HitObject>
                {
                    new Fruit
                    {
                        X = CatchPlayfield.CENTER_X,
                        StartTime = 0
                    },
                    new Fruit
                    {
                        X = 0,
                        StartTime = 1000
                    },
                    new Fruit
                    {
                        X = CatchPlayfield.WIDTH,
                        StartTime = 2000
                    },
                    new JuiceStream
                    {
                        X = CatchPlayfield.CENTER_X,
                        StartTime = 3000,
                        Path = new SliderPath(PathType.Linear, new[] { Vector2.Zero, Vector2.UnitY * 200 })
                    }
                }
            }
        });

        private bool passCondition()
        {
            var playfield = this.ChildrenOfType<CatchPlayfield>().Single();

            switch (Player.ScoreProcessor.Combo.Value)
            {
                case 0:
                    InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre);
                    break;

                case 1:
                    InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.BottomLeft);
                    break;

                case 2:
                    InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.BottomRight);
                    break;

                case 3:
                    InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre);
                    break;
            }

            return Player.ScoreProcessor.Combo.Value >= 6;
        }
    }
}
