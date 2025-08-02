// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneOsuComposerSelection : TestSceneOsuEditor
    {
        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        [Test]
        public void TestSelectAfterFadedOut()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));

            moveMouseToObject(() => slider);

            AddStep("seek after end", () => EditorClock.Seek(750));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("slider not selected", () => EditorBeatmap.SelectedHitObjects.Count == 0);

            AddStep("seek to visible", () => EditorClock.Seek(650));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("slider selected", () => EditorBeatmap.SelectedHitObjects.Single() == slider);
        }

        [Test]
        public void TestContextMenuShownCorrectlyForSelectedSlider()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));

            moveMouseToObject(() => slider);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("slider selected", () => EditorBeatmap.SelectedHitObjects.Single() == slider);

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(blueprintContainer.ChildrenOfType<SliderBodyPiece>().Single().ScreenSpaceDrawQuad.Centre));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("context menu is visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Open);
        }

        [Test]
        public void TestSelectionIncludingSliderPreservedOnClick()
        {
            var firstSlider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(0, 0),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            var secondSlider = new Slider
            {
                StartTime = 1000,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100, -100))
                    }
                }
            };
            var hitCircle = new HitCircle
            {
                StartTime = 200,
                Position = new Vector2(300, 0)
            };

            AddStep("add objects", () => EditorBeatmap.AddRange(new HitObject[] { firstSlider, secondSlider, hitCircle }));
            AddStep("select last 2 objects", () => EditorBeatmap.SelectedHitObjects.AddRange(new HitObject[] { secondSlider, hitCircle }));

            moveMouseToObject(() => secondSlider);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            AddAssert("selection preserved", () => EditorBeatmap.SelectedHitObjects.Count == 2);
        }

        [Test]
        public void TestControlClickAddsControlPointsIfSingleSliderSelected()
        {
            var firstSlider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(0, 0),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            var secondSlider = new Slider
            {
                StartTime = 1000,
                Position = new Vector2(200, 200),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100, -100))
                    }
                }
            };

            AddStep("add objects", () => EditorBeatmap.AddRange(new HitObject[] { firstSlider, secondSlider }));
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.AddRange(new HitObject[] { secondSlider }));

            AddStep("move mouse to middle of slider", () =>
            {
                var pos = blueprintContainer.SelectionBlueprints
                                            .First(s => s.Item == secondSlider)
                                            .ChildrenOfType<SliderBodyPiece>().First()
                                            .ScreenSpaceDrawQuad.Centre;

                InputManager.MoveMouseTo(pos);
            });
            AddStep("control-click left mouse", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("selection preserved", () => EditorBeatmap.SelectedHitObjects.Count, () => Is.EqualTo(1));
            AddAssert("slider has 3 anchors", () => secondSlider.Path.ControlPoints.Count, () => Is.EqualTo(3));
        }

        [Test]
        public void TestControlClickDoesNotAddSliderControlPointsIfMultipleObjectsSelected()
        {
            var firstSlider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(0, 0),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            var secondSlider = new Slider
            {
                StartTime = 1000,
                Position = new Vector2(200, 200),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100, -100))
                    }
                }
            };

            AddStep("add objects", () => EditorBeatmap.AddRange(new HitObject[] { firstSlider, secondSlider }));
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.AddRange(new HitObject[] { firstSlider, secondSlider }));

            AddStep("move mouse to middle of slider", () =>
            {
                var pos = blueprintContainer.SelectionBlueprints
                                            .First(s => s.Item == secondSlider)
                                            .ChildrenOfType<SliderBodyPiece>().First()
                                            .ScreenSpaceDrawQuad.Centre;

                InputManager.MoveMouseTo(pos);
            });
            AddStep("control-click left mouse", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("selection not preserved", () => EditorBeatmap.SelectedHitObjects.Count, () => Is.EqualTo(1));
            AddAssert("second slider not selected",
                () => blueprintContainer.SelectionBlueprints.First(s => s.Item == secondSlider).IsSelected,
                () => Is.False);
            AddAssert("slider still has 2 anchors", () => secondSlider.Path.ControlPoints.Count, () => Is.EqualTo(2));
        }

        [Test]
        public void TestControlClickDoesNotDiscardExistingSelectionEvenIfNothingHit()
        {
            var firstSlider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(0, 0),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };

            AddStep("add object", () => EditorBeatmap.AddRange([firstSlider]));
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.AddRange([firstSlider]));

            AddStep("move mouse to middle of playfield", () => InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.Centre));
            AddStep("control-click left mouse", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("selection preserved", () => EditorBeatmap.SelectedHitObjects.Count, () => Is.EqualTo(1));
        }

        [Test]
        public void TestQuickDeleteOnUnselectedControlPointOnlyRemovesThatControlPoint()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint { Type = PathType.LINEAR },
                        new PathControlPoint(new Vector2(100, 0)),
                        new PathControlPoint(new Vector2(100)),
                        new PathControlPoint(new Vector2(0, 100))
                    }
                }
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));
            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));

            AddStep("select second node", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("also select third node", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(2));
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddStep("quick-delete fourth node", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(3));
                InputManager.Click(MouseButton.Middle);
            });
            AddUntilStep("slider not deleted", () => EditorBeatmap.HitObjects.OfType<Slider>().Count(), () => Is.EqualTo(1));
            AddUntilStep("slider path has 3 nodes", () => EditorBeatmap.HitObjects.OfType<Slider>().Single().Path.ControlPoints.Count, () => Is.EqualTo(3));
        }

        [Test]
        public void TestQuickDeleteOnSelectedControlPointRemovesEntireSelection()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint { Type = PathType.LINEAR },
                        new PathControlPoint(new Vector2(100, 0)),
                        new PathControlPoint(new Vector2(100)),
                        new PathControlPoint(new Vector2(0, 100))
                    }
                }
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));
            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));

            AddStep("select second node", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1));
                InputManager.Click(MouseButton.Left);
            });
            AddStep("also select third node", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(2));
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddStep("quick-delete second node", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1));
                InputManager.Click(MouseButton.Middle);
            });
            AddUntilStep("slider not deleted", () => EditorBeatmap.HitObjects.OfType<Slider>().Count(), () => Is.EqualTo(1));
            AddUntilStep("slider path has 2 nodes", () => EditorBeatmap.HitObjects.OfType<Slider>().Single().Path.ControlPoints.Count, () => Is.EqualTo(2));
        }

        [Test]
        public void TestSliderDragMarkerDoesNotBlockControlPointContextMenu()
        {
            var slider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(100, 100),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint { Type = PathType.LINEAR },
                        new PathControlPoint(new Vector2(50, 100)),
                        new PathControlPoint(new Vector2(145, 100)),
                    },
                    ExpectedDistance = { Value = 162.62 }
                },
            };
            AddStep("add slider", () => EditorBeatmap.Add(slider));
            AddStep("select slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));

            AddStep("select last node", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<PathControlPointPiece<Slider>>().Last());
                InputManager.Click(MouseButton.Left);
            });
            AddStep("right click node", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("context menu open", () => this.ChildrenOfType<ContextMenuContainer>().Single().ChildrenOfType<Menu>().All(m => m.State == MenuState.Open));
        }

        [Test]
        public void TestSliderDragMarkerBlocksSelectionOfObjectsUnderneath()
        {
            var firstSlider = new Slider
            {
                StartTime = 0,
                Position = new Vector2(10, 50),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(100))
                    }
                }
            };
            var secondSlider = new Slider
            {
                StartTime = 500,
                Position = new Vector2(200, 0),
                Path = new SliderPath
                {
                    ControlPoints =
                    {
                        new PathControlPoint(),
                        new PathControlPoint(new Vector2(-100, 100))
                    }
                }
            };

            AddStep("add objects", () => EditorBeatmap.AddRange(new HitObject[] { firstSlider, secondSlider }));
            AddStep("select second slider", () => EditorBeatmap.SelectedHitObjects.Add(secondSlider));

            AddStep("move to marker", () =>
            {
                var marker = this.ChildrenOfType<SliderEndDragMarker>().First();
                var position = (marker.ScreenSpaceDrawQuad.TopRight + marker.ScreenSpaceDrawQuad.BottomRight) / 2;
                InputManager.MoveMouseTo(position);
            });
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second slider still selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondSlider));
        }

        private ComposeBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<ComposeBlueprintContainer>().First();

        private ContextMenuContainer contextMenuContainer
            => Editor.ChildrenOfType<ContextMenuContainer>().First();

        private void moveMouseToObject(Func<HitObject> targetFunc)
        {
            AddStep("move mouse to object", () =>
            {
                var pos = blueprintContainer.SelectionBlueprints
                                            .First(s => s.Item == targetFunc())
                                            .ChildrenOfType<HitCirclePiece>()
                                            .First().ScreenSpaceDrawQuad.Centre;

                InputManager.MoveMouseTo(pos);
            });
        }
    }
}
