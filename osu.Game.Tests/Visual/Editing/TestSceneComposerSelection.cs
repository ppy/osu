// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneComposerSelection : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private ComposeBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<ComposeBlueprintContainer>().First();

        private ContextMenuContainer contextMenuContainer
            => Editor.ChildrenOfType<ContextMenuContainer>().First();

        private SelectionBoxScaleHandle getScaleHandle(Anchor anchor)
            => Editor.ChildrenOfType<SelectionBoxScaleHandle>().First(it => it.Anchor == anchor);

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

        [Test]
        public void TestSelectAndShowContextMenu()
        {
            var addedObject = new HitCircle { StartTime = 100, Position = new Vector2(100, 100) };
            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            moveMouseToObject(() => addedObject);
            AddStep("right click", () => InputManager.Click(MouseButton.Right));

            AddUntilStep("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);
            AddUntilStep("context menu is visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Open);
        }

        [Test]
        public void TestSelectAndShowContextMenuOutsideBounds()
        {
            var addedObject = new HitCircle { StartTime = 100, Position = OsuPlayfield.BASE_SIZE };
            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("descale blueprint container", () => this.ChildrenOfType<HitObjectComposer>().Single().Scale = new Vector2(0.5f));
            AddStep("move mouse to bottom-right", () => InputManager.MoveMouseTo(blueprintContainer.ToScreenSpace(blueprintContainer.LayoutRectangle.BottomRight + new Vector2(10))));

            AddStep("right click", () => InputManager.Click(MouseButton.Right));

            AddUntilStep("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);
            AddUntilStep("context menu is visible", () => contextMenuContainer.ChildrenOfType<OsuContextMenu>().Single().State == MenuState.Open);
        }

        [Test]
        public void TestNudgeSelectionTime()
        {
            HitCircle[] addedObjects = null!;

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects = new[]
            {
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 200, Position = new Vector2(100) },
                new HitCircle { StartTime = 300, Position = new Vector2(200) },
                new HitCircle { StartTime = 400, Position = new Vector2(300) },
            }));

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));

            AddStep("nudge forwards", () => InputManager.Key(Key.K));
            AddAssert("objects moved forwards in time", () => addedObjects[0].StartTime > 100);

            AddStep("nudge backwards", () => InputManager.Key(Key.J));
            AddAssert("objects reverted to original position", () => addedObjects[0].StartTime == 100);
        }

        [Test]
        public void TestNudgeSelectionPosition()
        {
            HitCircle addedObject = null!;

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[]
            {
                addedObject = new HitCircle { StartTime = 200, Position = new Vector2(100) },
            }));

            AddStep("select object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("nudge up", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Up);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("object position moved up", () => addedObject.Position.Y, () => Is.EqualTo(99).Within(Precision.FLOAT_EPSILON));

            AddStep("nudge down", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Down);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("object position moved down", () => addedObject.Position.Y, () => Is.EqualTo(100).Within(Precision.FLOAT_EPSILON));

            AddStep("nudge left", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Left);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("object position moved left", () => addedObject.Position.X, () => Is.EqualTo(99).Within(Precision.FLOAT_EPSILON));

            AddStep("nudge right", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Right);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("object position moved right", () => addedObject.Position.X, () => Is.EqualTo(100).Within(Precision.FLOAT_EPSILON));
        }

        [Test]
        public void TestRotateHotkeys()
        {
            HitCircle[] addedObjects = null!;

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects = new[]
            {
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 200, Position = new Vector2(100) },
                new HitCircle { StartTime = 300, Position = new Vector2(200) },
                new HitCircle { StartTime = 400, Position = new Vector2(300) },
            }));

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));

            AddStep("rotate clockwise", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Period);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("objects rotated clockwise", () => addedObjects[0].Position == new Vector2(300, 0));

            AddStep("rotate counterclockwise", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Comma);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("objects reverted to original position", () => addedObjects[0].Position == new Vector2(0));
        }

        [Test]
        public void TestGlobalFlipHotkeys()
        {
            HitCircle addedObject = null!;

            AddStep("add hitobjects", () => EditorBeatmap.Add(addedObject = new HitCircle { StartTime = 100 }));

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            AddStep("flip horizontally across playfield", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.H);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("objects flipped horizontally", () => addedObject.Position == new Vector2(OsuPlayfield.BASE_SIZE.X, 0));

            AddStep("flip vertically across playfield", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.J);
                InputManager.ReleaseKey(Key.ControlLeft);
            });
            AddAssert("objects flipped vertically", () => addedObject.Position == OsuPlayfield.BASE_SIZE);
        }

        [Test]
        public void TestBasicSelect()
        {
            var addedObject = new HitCircle { StartTime = 100 };
            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            moveMouseToObject(() => addedObject);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));

            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);

            var addedObject2 = new HitCircle
            {
                StartTime = 100,
                Position = new Vector2(100),
            };

            AddStep("add one more hitobject", () => EditorBeatmap.Add(addedObject2));
            AddAssert("selection unchanged", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);

            moveMouseToObject(() => addedObject2);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject2);
        }

        [Test]
        public void TestMultiSelect()
        {
            var addedObjects = new[]
            {
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 200, Position = new Vector2(100) },
                new HitCircle { StartTime = 300, Position = new Vector2(200) },
                new HitCircle { StartTime = 400, Position = new Vector2(300) },
            };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects));

            moveMouseToObject(() => addedObjects[0]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));

            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObjects[0]);

            AddStep("hold control", () => InputManager.PressKey(Key.ControlLeft));

            moveMouseToObject(() => addedObjects[1]);
            AddStep("click second", () => InputManager.Click(MouseButton.Left));
            AddAssert("2 hitobjects selected", () => EditorBeatmap.SelectedHitObjects.Count == 2 && EditorBeatmap.SelectedHitObjects.Contains(addedObjects[1]));

            moveMouseToObject(() => addedObjects[2]);
            AddStep("click third", () => InputManager.Click(MouseButton.Left));
            AddAssert("3 hitobjects selected", () => EditorBeatmap.SelectedHitObjects.Count == 3 && EditorBeatmap.SelectedHitObjects.Contains(addedObjects[2]));

            moveMouseToObject(() => addedObjects[1]);
            AddStep("click second", () => InputManager.Click(MouseButton.Left));
            AddAssert("2 hitobjects selected", () => EditorBeatmap.SelectedHitObjects.Count == 2 && !EditorBeatmap.SelectedHitObjects.Contains(addedObjects[1]));
        }

        [Test]
        public void TestMultiSelectWithDragBox()
        {
            var addedObjects = new[]
            {
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 200, Position = new Vector2(100) },
                new HitCircle { StartTime = 300, Position = new Vector2(512, 0) },
                new HitCircle { StartTime = 400, Position = new Vector2(412, 100) },
            };
            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects));

            AddStep("start dragging", () =>
            {
                InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("drag to left corner", () => InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.TopLeft - new Vector2(5)));
            AddStep("end dragging", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("2 hitobjects selected", () => EditorBeatmap.SelectedHitObjects, () => Has.Count.EqualTo(2));

            AddStep("start dragging with control", () =>
            {
                InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
                InputManager.PressKey(Key.ControlLeft);
            });
            AddStep("drag to left corner", () => InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.TopRight + new Vector2(5, -5)));
            AddStep("end dragging", () => InputManager.ReleaseButton(MouseButton.Left));
            AddStep("release control", () => InputManager.ReleaseKey(Key.ControlLeft));

            AddAssert("4 hitobjects selected", () => EditorBeatmap.SelectedHitObjects, () => Has.Count.EqualTo(4));

            AddStep("start dragging without control", () =>
            {
                InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("drag to left corner", () => InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.TopRight + new Vector2(5, -5)));
            AddStep("end dragging", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("2 hitobjects selected", () => EditorBeatmap.SelectedHitObjects, () => Has.Count.EqualTo(2));
        }

        [Test]
        public void TestNearestSelection()
        {
            var firstObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 0 };
            var secondObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 600 };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[] { firstObject, secondObject }));

            moveMouseToObject(() => firstObject);

            AddStep("seek near first", () => EditorClock.Seek(100));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));

            AddStep("deselect", () => EditorBeatmap.SelectedHitObjects.Clear());

            AddStep("seek near second", () => EditorClock.Seek(500));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondObject));

            AddStep("deselect", () => EditorBeatmap.SelectedHitObjects.Clear());

            AddStep("seek halfway", () => EditorClock.Seek(300));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));
        }

        [Test]
        public void TestNearestSelectionWithEndTime()
        {
            var firstObject = new Slider
            {
                Position = new Vector2(256, 192),
                StartTime = 0,
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(),
                    new PathControlPoint(new Vector2(50, 0)),
                })
            };

            var secondObject = new HitCircle
            {
                Position = new Vector2(256, 192),
                StartTime = 600
            };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new HitObject[] { firstObject, secondObject }));

            moveMouseToObject(() => firstObject);

            AddStep("seek near first", () => EditorClock.Seek(100));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));

            AddStep("deselect", () => EditorBeatmap.SelectedHitObjects.Clear());

            AddStep("seek near second", () => EditorClock.Seek(500));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondObject));

            AddStep("deselect", () => EditorBeatmap.SelectedHitObjects.Clear());

            AddStep("seek roughly halfway", () => EditorClock.Seek(350));
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            // Slider gets priority due to end time.
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));
        }

        [Test]
        public void TestCyclicSelection()
        {
            var firstObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 0 };
            var secondObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 300 };
            var thirdObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 600 };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[] { firstObject, secondObject, thirdObject }));

            moveMouseToObject(() => firstObject);

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondObject));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("third selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(thirdObject));

            // cycle around
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));
        }

        [Test]
        public void TestCyclicSelectionOutwards()
        {
            var firstObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 0 };
            var secondObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 300 };
            var thirdObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 600 };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[] { firstObject, secondObject, thirdObject }));

            moveMouseToObject(() => firstObject);

            AddStep("seek near second", () => EditorClock.Seek(320));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondObject));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("third selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(thirdObject));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));

            // cycle around
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondObject));
        }

        [Test]
        public void TestCyclicSelectionBackwards()
        {
            var firstObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 0 };
            var secondObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 200 };
            var thirdObject = new HitCircle { Position = new Vector2(256, 192), StartTime = 400 };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[] { firstObject, secondObject, thirdObject }));

            moveMouseToObject(() => firstObject);

            AddStep("seek to third", () => EditorClock.Seek(350));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("third selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(thirdObject));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("second selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(secondObject));

            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("first selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(firstObject));

            // cycle around
            AddStep("left click", () => InputManager.Click(MouseButton.Left));
            AddAssert("third selected", () => EditorBeatmap.SelectedHitObjects.Single(), () => Is.EqualTo(thirdObject));
        }

        [Test]
        public void TestDoubleClickToSeek()
        {
            var hitCircle = new HitCircle { Position = new Vector2(256, 192), StartTime = 600 };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(new[] { hitCircle }));

            moveMouseToObject(() => hitCircle);

            AddRepeatStep("double click", () => InputManager.Click(MouseButton.Left), 2);

            AddUntilStep("seeked to circle", () => EditorClock.CurrentTime, () => Is.EqualTo(600));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestMultiSelectFromDrag(bool alreadySelectedBeforeDrag)
        {
            HitCircle[] addedObjects = null!;

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects = new[]
            {
                new HitCircle { StartTime = 100 },
                new HitCircle { StartTime = 200, Position = new Vector2(100) },
                new HitCircle { StartTime = 300, Position = new Vector2(200) },
                new HitCircle { StartTime = 400, Position = new Vector2(300) },
            }));

            moveMouseToObject(() => addedObjects[0]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));

            AddStep("hold control", () => InputManager.PressKey(Key.ControlLeft));

            moveMouseToObject(() => addedObjects[1]);

            if (alreadySelectedBeforeDrag)
                AddStep("click second", () => InputManager.Click(MouseButton.Left));

            AddStep("mouse down on second", () => InputManager.PressButton(MouseButton.Left));

            AddAssert("2 hitobjects selected", () => EditorBeatmap.SelectedHitObjects.Count == 2 && EditorBeatmap.SelectedHitObjects.Contains(addedObjects[1]));

            AddStep("drag to centre", () => InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.Centre));

            AddAssert("positions changed", () => addedObjects[0].Position != Vector2.Zero && addedObjects[1].Position != new Vector2(50));

            AddStep("release control", () => InputManager.ReleaseKey(Key.ControlLeft));
            AddStep("mouse up", () => InputManager.ReleaseButton(MouseButton.Left));
        }

        [Test]
        public void TestBasicDeselect()
        {
            var addedObject = new HitCircle { StartTime = 100 };
            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            moveMouseToObject(() => addedObject);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));

            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);

            AddStep("click away", () =>
            {
                InputManager.MoveMouseTo(blueprintContainer.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("selection lost", () => EditorBeatmap.SelectedHitObjects.Count == 0);
        }

        [Test]
        public void TestQuickDeleteRemovesObjectInPlacement()
        {
            var addedObject = new HitCircle
            {
                StartTime = 0,
                Position = OsuPlayfield.BASE_SIZE * 0.5f
            };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("enter placement mode", () => InputManager.PressKey(Key.Number2));

            moveMouseToObject(() => addedObject);

            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));

            AddAssert("no hitobjects in beatmap", () => EditorBeatmap.HitObjects.Count == 0);
        }

        [Test]
        public void TestQuickDeleteRemovesObjectInSelection()
        {
            var addedObject = new HitCircle
            {
                StartTime = 0,
                Position = OsuPlayfield.BASE_SIZE * 0.5f
            };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            AddStep("select added object", () => EditorBeatmap.SelectedHitObjects.Add(addedObject));

            moveMouseToObject(() => addedObject);

            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));

            AddAssert("no hitobjects in beatmap", () => EditorBeatmap.HitObjects.Count == 0);
        }

        [Test]
        public void TestQuickDeleteRemovesSliderControlPoint()
        {
            Slider slider = null!;

            PathControlPoint[] points =
            {
                new PathControlPoint(),
                new PathControlPoint(new Vector2(50, 0)),
                new PathControlPoint(new Vector2(100, 0))
            };

            AddStep("add slider", () =>
            {
                slider = new Slider
                {
                    StartTime = 1000,
                    Path = new SliderPath(points)
                };

                EditorBeatmap.Add(slider);
            });

            AddStep("select added slider", () => EditorBeatmap.SelectedHitObjects.Add(slider));

            AddStep("move mouse to controlpoint", () =>
            {
                var pos = blueprintContainer.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1).ScreenSpaceDrawQuad.Centre;
                InputManager.MoveMouseTo(pos);
            });
            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));

            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddAssert("slider has 2 points", () => slider.Path.ControlPoints.Count == 2);

            AddStep("right click", () => InputManager.Click(MouseButton.Right));

            // second click should nuke the object completely.
            AddAssert("no hitobjects in beatmap", () => EditorBeatmap.HitObjects.Count == 0);

            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));
        }

        [Test]
        public void TestShiftModifierMaintainsAspectRatio()
        {
            HitCircle[] addedObjects = null!;

            float aspectRatioBeforeDrag = 0;

            float getAspectRatio() => (addedObjects[1].X - addedObjects[0].X) / (addedObjects[1].Y - addedObjects[0].Y);

            AddStep("add hitobjects", () =>
            {
                EditorBeatmap.AddRange(addedObjects = new[]
                {
                    new HitCircle { StartTime = 100, Position = new Vector2(150, 150) },
                    new HitCircle { StartTime = 200, Position = new Vector2(250, 200) },
                });

                aspectRatioBeforeDrag = getAspectRatio();
            });

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));

            AddStep("move mouse to handle", () => InputManager.MoveMouseTo(getScaleHandle(Anchor.BottomRight).ScreenSpaceDrawQuad.Centre));

            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));

            AddStep("move mouse", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(50, 0)));

            AddStep("aspect ratio does not equal", () => Assert.AreNotEqual(aspectRatioBeforeDrag, getAspectRatio()));

            AddStep("press shift", () => InputManager.PressKey(Key.ShiftLeft));

            AddStep("aspect ratio does equal", () => Assert.AreEqual(aspectRatioBeforeDrag, getAspectRatio()));

            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));
        }

        [Test]
        public void TestAltModifierScalesAroundCenter()
        {
            HitCircle[] addedObjects = null!;

            Vector2 centerBeforeDrag = Vector2.Zero;

            Vector2 getCenter() => (addedObjects[0].Position + addedObjects[1].Position) / 2;

            AddStep("add hitobjects", () =>
            {
                EditorBeatmap.AddRange(addedObjects = new[]
                {
                    new HitCircle { StartTime = 100, Position = new Vector2(150, 150) },
                    new HitCircle { StartTime = 200, Position = new Vector2(250, 200) },
                });

                centerBeforeDrag = getCenter();
            });

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));

            AddStep("move mouse to handle", () => InputManager.MoveMouseTo(getScaleHandle(Anchor.BottomRight).ScreenSpaceDrawQuad.Centre));

            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));

            AddStep("move mouse", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(50, 0)));

            AddStep("center does not equal", () => Assert.AreNotEqual(centerBeforeDrag, getCenter()));

            AddStep("press alt", () => InputManager.PressKey(Key.AltLeft));

            AddStep("center does equal", () => Assert.AreEqual(centerBeforeDrag, getCenter()));

            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
        }

        [Test]
        public void TestShiftAndAltModifierKeys()
        {
            HitCircle[] addedObjects = null!;

            float aspectRatioBeforeDrag = 0;

            Vector2 centerBeforeDrag = Vector2.Zero;

            float getAspectRatio() => (addedObjects[1].X - addedObjects[0].X) / (addedObjects[1].Y - addedObjects[0].Y);

            Vector2 getCenter() => (addedObjects[0].Position + addedObjects[1].Position) / 2;

            AddStep("add hitobjects", () =>
            {
                EditorBeatmap.AddRange(addedObjects = new[]
                {
                    new HitCircle { StartTime = 100, Position = new Vector2(150, 150) },
                    new HitCircle { StartTime = 200, Position = new Vector2(250, 200) },
                });

                aspectRatioBeforeDrag = getAspectRatio();
                centerBeforeDrag = getCenter();
            });

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));

            AddStep("move mouse to handle", () => InputManager.MoveMouseTo(getScaleHandle(Anchor.BottomRight).ScreenSpaceDrawQuad.Centre));

            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));

            AddStep("move mouse", () => InputManager.MoveMouseTo(InputManager.CurrentState.Mouse.Position + new Vector2(50, 0)));

            AddStep("aspect ratio does not equal", () => Assert.AreNotEqual(aspectRatioBeforeDrag, getAspectRatio()));

            AddStep("center does not equal", () => Assert.AreNotEqual(centerBeforeDrag, getCenter()));

            AddStep("press shift", () => InputManager.PressKey(Key.ShiftLeft));

            AddStep("aspect ratio does equal", () => Assert.AreEqual(aspectRatioBeforeDrag, getAspectRatio()));

            AddStep("center does not equal", () => Assert.AreNotEqual(centerBeforeDrag, getCenter()));

            AddStep("press alt", () => InputManager.PressKey(Key.AltLeft));

            AddStep("center does equal", () => Assert.AreEqual(centerBeforeDrag, getCenter()));

            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));

            AddStep("release alt", () => InputManager.ReleaseKey(Key.AltLeft));
        }
    }
}
