// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Tests
{
    [TestFixture]
    public partial class TestSceneHyperDash : TestSceneCatchPlayer
    {
        protected override bool Autoplay => true;

        private int hyperDashCount;
        private bool inHyperDash;

        [Test]
        public void TestHyperDash()
        {
            AddStep("reset count", () =>
            {
                inHyperDash = false;
                hyperDashCount = 0;

                // this needs to be done within the frame stable context due to how quickly hyperdash state changes occur.
                Player.DrawableRuleset.FrameStableComponents.OnUpdate += _ =>
                {
                    var catcher = Player.ChildrenOfType<Catcher>().FirstOrDefault();

                    if (catcher == null)
                        return;

                    if (catcher.HyperDashing != inHyperDash)
                    {
                        inHyperDash = catcher.HyperDashing;
                        if (catcher.HyperDashing)
                            hyperDashCount++;
                    }
                };
            });

            AddAssert("First note is hyperdash", () => Beatmap.Value.Beatmap.HitObjects[0] is Fruit f && f.HyperDash);

            for (int i = 0; i < 9; i++)
            {
                int count = i + 1;
                AddUntilStep($"wait for hyperdash #{count}", () => hyperDashCount >= count);
            }
        }

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = new Beatmap
            {
                BeatmapInfo =
                {
                    Ruleset = ruleset,
                    Difficulty = new BeatmapDifficulty
                    {
                        CircleSize = 3.6f,
                        SliderMultiplier = 1,
                    },
                }
            };

            beatmap.ControlPointInfo.Add(0, new TimingControlPoint());

            // Should produce a hyper-dash (edge case test)
            beatmap.HitObjects.Add(new Fruit { StartTime = 1816, X = 56, NewCombo = true });
            beatmap.HitObjects.Add(new Fruit { StartTime = 2008, X = 308, NewCombo = true });

            double startTime = 3000;

            const float left_x = 0.02f * CatchPlayfield.WIDTH;
            const float right_x = 0.98f * CatchPlayfield.WIDTH;

            createObjects(() => new Fruit { X = left_x });
            createObjects(() => new TestJuiceStream(right_x), 1);
            createObjects(() => new TestJuiceStream(left_x), 1);
            createObjects(() => new Fruit { X = right_x });
            createObjects(() => new Fruit { X = left_x });
            createObjects(() => new Fruit { X = right_x });
            createObjects(() => new TestJuiceStream(left_x), 1);

            beatmap.ControlPointInfo.Add(startTime, new TimingControlPoint
            {
                BeatLength = 50
            });

            createObjects(() => new TestJuiceStream(left_x)
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero),
                    new PathControlPoint(new Vector2(512, 0))
                })
            }, 1);

            return beatmap;

            void createObjects(Func<CatchHitObject> createObject, int count = 3)
            {
                const float spacing = 140;

                for (int i = 0; i < count; i++)
                {
                    var hitObject = createObject();
                    hitObject.StartTime = startTime + i * spacing;
                    beatmap.HitObjects.Add(hitObject);
                }

                startTime += 700;
            }
        }

        private class TestJuiceStream : JuiceStream
        {
            public TestJuiceStream(float x)
            {
                X = x;

                Path = new SliderPath(new[]
                {
                    new PathControlPoint(Vector2.Zero),
                    new PathControlPoint(new Vector2(30, 0)),
                });
            }
        }
    }
}
