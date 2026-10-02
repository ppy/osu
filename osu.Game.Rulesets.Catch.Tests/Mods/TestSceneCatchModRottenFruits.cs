// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Catch.Beatmaps;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.Objects.Drawables;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Tests.Visual;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests.Mods
{
    public partial class TestSceneCatchModRottenFruits : ModTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new CatchRuleset();

        [Test]
        public void TestMissingFruitsIsGood() => CreateModTest(new ModTestData
        {
            Mod = new CatchModRottenFruits(),
            CreateBeatmap = () => new CatchBeatmap
            {
                HitObjects =
                [
                    new Fruit { StartTime = 0, X = 0 },
                    new Fruit { StartTime = 500, X = 512 },
                    new JuiceStream
                    {
                        StartTime = 1000,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        },
                    },
                    new JuiceStream
                    {
                        StartTime = 2000,
                        X = 512,
                        Path = new SliderPath
                        {
                            ControlPoints =
                            {
                                new PathControlPoint(),
                                new PathControlPoint(new Vector2(0, 100))
                            }
                        },
                    }
                ]
            },
            Autoplay = false,
            ReplayFrames = [],
            PassCondition = () => Player.ScoreProcessor.Combo.Value == 6,
        });

        [Test]
        public void TestCatchingFruitIsBad() => CreateModTest(new ModTestData
        {
            Mod = new CatchModRottenFruits(),
            CreateBeatmap = () => new CatchBeatmap
            {
                HitObjects =
                [
                    new Fruit { StartTime = 0, X = 0 },
                    new Fruit { StartTime = 500, X = 512 },
                    new Fruit { StartTime = 1000, X = 256 },
                ]
            },
            Autoplay = false,
            ReplayFrames = [],
            PassCondition = () => Player.ScoreProcessor.Statistics.GetValueOrDefault(HitResult.Miss) == 1,
        });

        [Test]
        public void TestHyperDashesDisabled()
        {
            var nonHyperDashObjects = new HashSet<HitObject>();

            CreateModTest(new ModTestData
            {
                Mod = new CatchModRottenFruits(),
                CreateBeatmap = () => new CatchBeatmap
                {
                    HitObjects =
                    [
                        new Fruit { StartTime = 0, X = 0 },
                        new Fruit { StartTime = 100, X = 512 },
                        new JuiceStream
                        {
                            StartTime = 200,
                            Path = new SliderPath
                            {
                                ControlPoints =
                                {
                                    new PathControlPoint(),
                                    new PathControlPoint(new Vector2(512, 50)),
                                    new PathControlPoint(new Vector2(0, 100))
                                }
                            },
                            SliderVelocityMultiplier = 10
                        },
                        new JuiceStream
                        {
                            StartTime = 1000,
                            Path = new SliderPath
                            {
                                ControlPoints =
                                {
                                    new PathControlPoint(new Vector2(512, 0)),
                                    new PathControlPoint(new Vector2(0, 50)),
                                    new PathControlPoint(new Vector2(512, 100))
                                }
                            },
                            SliderVelocityMultiplier = 10
                        },
                    ]
                },
                Autoplay = false,
                ReplayFrames = [],
                PassCondition = () =>
                {
                    nonHyperDashObjects.UnionWith(Player.DrawableRuleset
                                                        .ChildrenOfType<DrawablePalpableCatchHitObject>()
                                                        .Where(dho => !dho.HyperDash.Value)
                                                        .Select(dho => dho.HitObject));
                    return nonHyperDashObjects.Count == 20;
                },
            });
        }
    }
}
