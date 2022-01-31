// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneTimelineSelection : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private TimelineBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<TimelineBlueprintContainer>().First();

        private void moveMouseToObject(Func<HitObject> targetFunc)
        {
            AddStep("move mouse to object", () =>
            {
                var pos = blueprintContainer.SelectionBlueprints
                                            .First(s => s.Item == targetFunc())
                                            .ChildrenOfType<TimelineHitObjectBlueprint>()
                                            .First().ScreenSpaceDrawQuad.Centre;

                InputManager.MoveMouseTo(pos);
            });
        }

        [Test]
        public void TestNudgeSelection()
        {
            HitCircle[] addedObjects = null;

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects = new[]
            {
                new HitCircle { StartTime = 500 },
                new HitCircle { StartTime = 1000, Position = new Vector2(100) },
                new HitCircle { StartTime = 1500, Position = new Vector2(200) },
                new HitCircle { StartTime = 2000, Position = new Vector2(300) },
            }));

            AddStep("select objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));

            AddStep("nudge forwards", () => InputManager.Key(Key.K));
            AddAssert("objects moved forwards in time", () => addedObjects[0].StartTime > 500);

            AddStep("nudge backwards", () => InputManager.Key(Key.J));
            AddAssert("objects reverted to original position", () => addedObjects[0].StartTime == 500);
        }

        [Test]
        public void TestBasicSelect()
        {
            var addedObject = new HitCircle { StartTime = 500 };
            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            moveMouseToObject(() => addedObject);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));

            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);

            var addedObject2 = new HitCircle
            {
                StartTime = 1000,
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
                new HitCircle { StartTime = 500 },
                new HitCircle { StartTime = 1000, Position = new Vector2(100) },
                new HitCircle { StartTime = 1500, Position = new Vector2(200) },
                new HitCircle { StartTime = 2000, Position = new Vector2(300) },
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

            AddStep("release control", () => InputManager.ReleaseKey(Key.ControlLeft));
        }

        [Test]
        public void TestBasicDeselect()
        {
            var addedObject = new HitCircle { StartTime = 500 };
            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            moveMouseToObject(() => addedObject);
            AddStep("left click", () => InputManager.Click(MouseButton.Left));

            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObject);

            AddStep("click away", () =>
            {
                InputManager.MoveMouseTo(Editor.ChildrenOfType<TimelineArea>().Single().ScreenSpaceDrawQuad.TopLeft + Vector2.One);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("selection lost", () => EditorBeatmap.SelectedHitObjects.Count == 0);
        }

        [Test]
        public void TestQuickDelete()
        {
            var addedObject = new HitCircle
            {
                StartTime = 0,
            };

            AddStep("add hitobject", () => EditorBeatmap.Add(addedObject));

            moveMouseToObject(() => addedObject);

            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));

            AddAssert("no hitobjects in beatmap", () => EditorBeatmap.HitObjects.Count == 0);
        }

        [Test]
        public void TestRangeSelect()
        {
            var addedObjects = new[]
            {
                new HitCircle { StartTime = 500 },
                new HitCircle { StartTime = 1000, Position = new Vector2(100) },
                new HitCircle { StartTime = 1500, Position = new Vector2(200) },
                new HitCircle { StartTime = 2000, Position = new Vector2(300) },
                new HitCircle { StartTime = 2500, Position = new Vector2(400) },
            };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects));

            moveMouseToObject(() => addedObjects[1]);
            AddStep("click second", () => InputManager.Click(MouseButton.Left));

            AddAssert("hitobject selected", () => EditorBeatmap.SelectedHitObjects.Single() == addedObjects[1]);

            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));

            moveMouseToObject(() => addedObjects[3]);
            AddStep("click fourth", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Skip(1).Take(3));

            moveMouseToObject(() => addedObjects[0]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Take(2));

            AddStep("clear selection", () => EditorBeatmap.SelectedHitObjects.Clear());
            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));

            moveMouseToObject(() => addedObjects[0]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Take(1));

            AddStep("hold ctrl", () => InputManager.PressKey(Key.ControlLeft));
            moveMouseToObject(() => addedObjects[2]);
            AddStep("click third", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(new[] { addedObjects[0], addedObjects[2] });

            AddStep("hold shift", () => InputManager.PressKey(Key.ShiftLeft));
            moveMouseToObject(() => addedObjects[4]);
            AddStep("click fifth", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Except(new[] { addedObjects[1] }));

            moveMouseToObject(() => addedObjects[0]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects);

            AddStep("clear selection", () => EditorBeatmap.SelectedHitObjects.Clear());
            moveMouseToObject(() => addedObjects[0]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Take(1));

            moveMouseToObject(() => addedObjects[1]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Take(2));

            moveMouseToObject(() => addedObjects[2]);
            AddStep("click first", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Take(3));

            AddStep("release keys", () =>
            {
                InputManager.ReleaseKey(Key.ControlLeft);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
        }

        [Test]
        public void TestRangeSelectAfterExternalSelection()
        {
            var addedObjects = new[]
            {
                new HitCircle { StartTime = 500 },
                new HitCircle { StartTime = 1000, Position = new Vector2(100) },
                new HitCircle { StartTime = 1500, Position = new Vector2(200) },
                new HitCircle { StartTime = 2000, Position = new Vector2(300) },
            };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects));

            AddStep("select all without mouse", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            assertSelectionIs(addedObjects);

            AddStep("hold down shift", () => InputManager.PressKey(Key.ShiftLeft));

            moveMouseToObject(() => addedObjects[1]);
            AddStep("click second object", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects);

            moveMouseToObject(() => addedObjects[3]);
            AddStep("click fourth object", () => InputManager.Click(MouseButton.Left));
            assertSelectionIs(addedObjects.Skip(1));

            AddStep("release shift", () => InputManager.ReleaseKey(Key.ShiftLeft));
        }

        private void assertSelectionIs(IEnumerable<HitObject> hitObjects)
            => AddAssert("correct hitobjects selected", () => EditorBeatmap.SelectedHitObjects.OrderBy(h => h.StartTime).SequenceEqual(hitObjects));
    }
}
