// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    public partial class TestSceneCatchModHidden : ModTestScene
    {
        [Test]
        public void TestJuiceStream()
        {
            CreateModTest(new ModTestData
            {
                Beatmap = new Beatmap
                {
                    HitObjects = new List<HitObject>
                    {
                        new JuiceStream
                        {
                            StartTime = 1000,
                            Path = new SliderPath(PathType.LINEAR, new[] { Vector2.Zero, new Vector2(0, -192) }),
                            X = CatchPlayfield.WIDTH / 2
                        }
                    }
                },
                Mod = new CatchModHidden(),
                PassCondition = () => Player.Results.Count > 0
                                      && Player.ChildrenOfType<DrawableJuiceStream>().Single().Alpha > 0
                                      && Player.ChildrenOfType<DrawableFruit>().Last().Alpha > 0
            });
        }

        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();
    }
}
