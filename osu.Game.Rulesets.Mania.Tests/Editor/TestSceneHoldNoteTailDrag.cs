// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;
using DragArea = osu.Game.Screens.Edit.Compose.Components.Timeline.TimelineHitObjectBlueprint.DragArea;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneHoldNoteTailDrag : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        [Test]
        public void TestSimpleTailDragForward()
        {
            AddStep("Add hold note", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new HoldNote { StartTime = 2170, Duration = 937.5 });
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().Single();
                dragForward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is higher", () =>
            {
                var holdNote = EditorBeatmap.HitObjects.First() as IHasDuration;
                return holdNote!.Duration > 937.5f;
            });
        }

        [Test]
        public void TestSimpleTailDragBackwards()
        {
            AddStep("Add hold note", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new HoldNote { StartTime = 2170, Duration = 937.5 });
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().Single();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower", () =>
            {
                var holdNote = EditorBeatmap.HitObjects.First() as IHasDuration;
                return holdNote!.Duration < 937.5f;
            });
        }

        [Test]
        public void TestSamePositionButNotSelectedDragForward()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragForward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is higher, and the other is unchanged", () =>
            {
                var holdNote1 = EditorBeatmap.HitObjects.First() as IHasDuration;
                var holdNote2 = EditorBeatmap.HitObjects.Last() as IHasDuration;
                return holdNote1!.Duration > 937.5f && holdNote2!.Duration == 937.5f;
            });
        }

        [Test]
        public void TestSamePositionButNotSelectedDragBackward()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower, and the other is unchanged", () =>
            {
                var holdNote1 = EditorBeatmap.HitObjects.First() as IHasDuration;
                var holdNote2 = EditorBeatmap.HitObjects.Last() as IHasDuration;
                return holdNote1!.Duration < 937.5f && holdNote2!.Duration == 937.5f;
            });
        }

        [Test]
        public void TestSamePositionSelectedDragForward()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Start select", selectAllNotes);
            AddStep("End select", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragForward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Both durations are higher", () =>
            {
                var holdNote1 = EditorBeatmap.HitObjects.First() as IHasDuration;
                var holdNote2 = EditorBeatmap.HitObjects.Last() as IHasDuration;
                return holdNote1!.Duration > 937.5f && holdNote2!.Duration > 937.5f;
            });
        }

        [Test]
        public void TestSamePositionSelectedDragBackward()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Start select", selectAllNotes);
            AddStep("End select", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Both durations are lower", () =>
            {
                var holdNote1 = EditorBeatmap.HitObjects.First() as IHasDuration;
                var holdNote2 = EditorBeatmap.HitObjects.Last() as IHasDuration;
                return holdNote1!.Duration < 937.5f && holdNote2!.Duration < 937.5f;
            });
        }

        [Test]
        public void TestSelectedButDifferentPositions()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2404, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Start select", selectAllNotes);
            AddStep("End select", () => InputManager.ReleaseButton(MouseButton.Left));

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower, and the other is unchanged", () =>
            {
                var holdNote1 = EditorBeatmap.HitObjects.First() as IHasDuration;
                var holdNote2 = EditorBeatmap.HitObjects.Last() as IHasDuration;
                return holdNote2!.Duration < 937.5f && holdNote1!.Duration == 937.5f;
            });
        }

        private void selectAllNotes()
        {
            InputManager.MoveMouseTo(new Vector2(1100, 110));
            InputManager.PressButton(MouseButton.Left);
            InputManager.MoveMouseTo(new Vector2(-50, 110));
        }

        private void dragForward(DragArea dragArea)
        {
            InputManager.MoveMouseTo(dragArea);
            InputManager.PressKey(Key.LShift);
            InputManager.PressButton(MouseButton.Left);
            InputManager.MoveMouseTo(new Vector2(1100, 110));
        }

        private void dragBackward(DragArea dragArea)
        {
            InputManager.MoveMouseTo(dragArea);
            InputManager.PressKey(Key.LShift);
            InputManager.PressButton(MouseButton.Left);
            InputManager.MoveMouseTo(new Vector2(-200, 110));
        }
    }
}
