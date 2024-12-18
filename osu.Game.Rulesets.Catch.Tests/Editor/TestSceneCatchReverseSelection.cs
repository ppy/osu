// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneCatchReverseSelection : TestSceneEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestReverseSelectionTwoFruits()
        {
            CatchHitObject[] objects = null!;
            bool[] newCombos = null!;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = 20,
                }
            ]);

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            selectEverything();
            reverseSelection();

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        [Test]
        public void TestReverseSelectionThreeFruits()
        {
            CatchHitObject[] objects = null!;
            bool[] newCombos = null!;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = 20,
                },
                new Fruit
                {
                    StartTime = 600,
                    X = 40,
                }
            ]);

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            selectEverything();
            reverseSelection();

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        [Test]
        public void TestReverseSelectionFruitAndJuiceStream()
        {
            CatchHitObject[] objects = null!;
            bool[] newCombos = null!;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = 0,
                },
                new JuiceStream
                {
                    StartTime = 400,
                    X = 20,
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(50))
                        }
                    }
                }
            ]);

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            selectEverything();
            reverseSelection();

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        [Test]
        public void TestReverseSelectionTwoFruitsAndJuiceStream()
        {
            CatchHitObject[] objects = null!;
            bool[] newCombos = null!;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = 20,
                },
                new JuiceStream
                {
                    StartTime = 600,
                    X = 40,
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(),
                            new PathControlPoint(new Vector2(50))
                        }
                    }
                }
            ]);

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            selectEverything();
            reverseSelection();

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        [Test]
        public void TestReverseSelectionTwoCombos()
        {
            CatchHitObject[] objects = null!;
            bool[] newCombos = null!;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = 20,
                },
                new Fruit
                {
                    StartTime = 600,
                    X = 40,
                },

                new Fruit
                {
                    StartTime = 800,
                    NewCombo = true,
                    X = 60,
                },
                new Fruit
                {
                    StartTime = 1000,
                    X = 80,
                },
                new Fruit
                {
                    StartTime = 1200,
                    X = 100,
                }
            ]);

            AddStep("store objects & new combo data", () =>
            {
                objects = getObjects().ToArray();
                newCombos = getObjectNewCombos().ToArray();
            });

            selectEverything();
            reverseSelection();

            AddAssert("objects reversed", getObjects, () => Is.EqualTo(objects.Reverse()));
            AddAssert("new combo positions preserved", getObjectNewCombos, () => Is.EqualTo(newCombos));
        }

        private void addObjects(CatchHitObject[] hitObjects) => AddStep("Add objects", () => EditorBeatmap.AddRange(hitObjects));

        private IEnumerable<CatchHitObject> getObjects() => EditorBeatmap.HitObjects.OfType<CatchHitObject>();

        private IEnumerable<bool> getObjectNewCombos() => getObjects().Select(ho => ho.NewCombo);

        private void selectEverything()
        {
            AddStep("Select everything", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });
        }

        private void reverseSelection()
        {
            AddStep("Reverse selection", () =>
            {
                InputManager.PressKey(Key.LControl);
                InputManager.Key(Key.G);
                InputManager.ReleaseKey(Key.LControl);
            });
        }
    }
}
