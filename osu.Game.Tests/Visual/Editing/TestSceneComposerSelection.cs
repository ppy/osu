// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneComposerSelection : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

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
        public void TestNudgeSelection()
        {
            HitCircle[] addedObjects = null;

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

        [TestCase(false)]
        [TestCase(true)]
        public void TestMultiSelectFromDrag(bool alreadySelectedBeforeDrag)
        {
            HitCircle[] addedObjects = null;

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
            Slider slider = null;

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
                var pos = blueprintContainer.ChildrenOfType<PathControlPointPiece>().ElementAt(1).ScreenSpaceDrawQuad.Centre;
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
    }
}
