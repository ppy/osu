// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            float fruit1OldX = default;
            float fruit2OldX = default;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = fruit1OldX = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = fruit2OldX = 20,
                }
            ]);

            selectEverything();
            reverseSelection();

            AddAssert("fruit1 is at fruit2's X",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(0).EffectiveX,
                () => Is.EqualTo(fruit2OldX)
            );

            AddAssert("fruit2 is at fruit1's X",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(1).EffectiveX,
                () => Is.EqualTo(fruit1OldX)
            );

            AddAssert("fruit2 is not a new combo",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(1).NewCombo,
                () => Is.EqualTo(false)
            );
        }

        [Test]
        public void TestReverseSelectionThreeFruits()
        {
            float fruit1OldX = default;
            float fruit2OldX = default;
            float fruit3OldX = default;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = fruit1OldX = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = fruit2OldX = 20,
                },
                new Fruit
                {
                    StartTime = 600,
                    X = fruit3OldX = 40,
                }
            ]);

            selectEverything();
            reverseSelection();

            AddAssert("fruit1 is at fruit3's X",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(0).EffectiveX,
                () => Is.EqualTo(fruit3OldX)
            );

            AddAssert("fruit2's X is unchanged",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(1).EffectiveX,
                () => Is.EqualTo(fruit2OldX)
            );

            AddAssert("fruit3's is at fruit1's X",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(2).EffectiveX,
                () => Is.EqualTo(fruit1OldX)
            );

            AddAssert("fruit3 is not a new combo",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(2).NewCombo,
                () => Is.EqualTo(false)
            );
        }

        [Test]
        public void TestReverseSelectionFruitAndJuiceStream()
        {
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

            selectEverything();
            reverseSelection();

            AddAssert("First element is juice stream",
                () => EditorBeatmap.HitObjects.First().GetType(),
                () => Is.EqualTo(typeof(JuiceStream))
            );

            AddAssert("Last element is fruit",
                () => EditorBeatmap.HitObjects.Last().GetType(),
                () => Is.EqualTo(typeof(Fruit))
            );

            AddAssert("Fruit is not new combo",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(0).NewCombo,
                () => Is.EqualTo(false)
            );
        }

        [Test]
        public void TestReverseSelectionTwoFruitsAndJuiceStream()
        {
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

            selectEverything();
            reverseSelection();

            AddAssert("First element is juice stream",
                () => EditorBeatmap.HitObjects.First().GetType(),
                () => Is.EqualTo(typeof(JuiceStream))
            );

            AddAssert("Middle element is Fruit",
                () => EditorBeatmap.HitObjects.ElementAt(1).GetType(),
                () => Is.EqualTo(typeof(Fruit))
            );

            AddAssert("Last element is Fruit",
                () => EditorBeatmap.HitObjects.Last().GetType(),
                () => Is.EqualTo(typeof(Fruit))
            );

            AddAssert("Last fruit is not new combo",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().Last().NewCombo,
                () => Is.EqualTo(false)
            );
        }

        [Test]
        public void TestReverseSelectionTwoCombos()
        {
            float fruit1OldX = default;
            float fruit2OldX = default;
            float fruit3OldX = default;

            float fruit4OldX = default;
            float fruit5OldX = default;
            float fruit6OldX = default;

            addObjects([
                new Fruit
                {
                    StartTime = 200,
                    X = fruit1OldX = 0,
                },
                new Fruit
                {
                    StartTime = 400,
                    X = fruit2OldX = 20,
                },
                new Fruit
                {
                    StartTime = 600,
                    X = fruit3OldX = 40,
                },

                new Fruit
                {
                    StartTime = 800,
                    NewCombo = true,
                    X = fruit4OldX = 60,
                },
                new Fruit
                {
                    StartTime = 1000,
                    X = fruit5OldX = 80,
                },
                new Fruit
                {
                    StartTime = 1200,
                    X = fruit6OldX = 100,
                }
            ]);

            selectEverything();
            reverseSelection();

            AddAssert("fruit1 is at fruit6 position",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(0).EffectiveX,
                () => Is.EqualTo(fruit6OldX)
            );

            AddAssert("fruit2 is at fruit5 position",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(1).EffectiveX,
                () => Is.EqualTo(fruit5OldX)
            );

            AddAssert("fruit3 is at fruit4 position",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(2).EffectiveX,
                () => Is.EqualTo(fruit4OldX)
            );

            AddAssert("fruit4 is at fruit3 position",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(3).EffectiveX,
                () => Is.EqualTo(fruit3OldX)
            );

            AddAssert("fruit5 is at fruit2 position",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(4).EffectiveX,
                () => Is.EqualTo(fruit2OldX)
            );

            AddAssert("fruit6 is at fruit1 position",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(5).EffectiveX,
                () => Is.EqualTo(fruit1OldX)
            );

            AddAssert("fruit1 is new combo",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(0).NewCombo,
                () => Is.EqualTo(true)
            );

            AddAssert("fruit4 is new combo",
                () => EditorBeatmap.HitObjects.OfType<Fruit>().ElementAt(3).NewCombo,
                () => Is.EqualTo(true)
            );
        }

        private void addObjects(CatchHitObject[] hitObjects) => AddStep("Add objects", () => EditorBeatmap.AddRange(hitObjects));

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
