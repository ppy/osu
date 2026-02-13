// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Tests.Visual;
using osuTK;
using osuTK.Input;
using DragArea = osu.Game.Screens.Edit.Compose.Components.Timeline.TimelineHitObjectBlueprint.DragArea;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneHoldNoteTailDrag : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("Clear objects", () => EditorBeatmap.Clear());
        }

        [Test]
        public void TestSimpleTailDragForward()
        {
            AddStep("Add hold note", () =>
            {
                EditorBeatmap.Add(new HoldNote { StartTime = 2170, Duration = 937.5 });
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().Single();
                dragForward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is higher", () => ((HoldNote)EditorBeatmap.HitObjects.First())!.Duration > 937.5f);
        }

        [Test]
        public void TestSimpleTailDragBackwards()
        {
            AddStep("Add hold note", () =>
            {
                EditorBeatmap.Add(new HoldNote { StartTime = 2170, Duration = 937.5 });
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().Single();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower", () => ((HoldNote)EditorBeatmap.HitObjects[0]).Duration < 937.5f);
        }

        [Test]
        public void TestSamePositionButNotSelectedDragForward()
        {
            AddStep("Add hold notes", () =>
            {
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

            AddAssert("Duration is higher, other is unchanged", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration > 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration == 937.5f
            );
        }

        [Test]
        public void TestSamePositionButNotSelectedDragBackward()
        {
            AddStep("Add hold notes", () =>
            {
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

            AddAssert("Duration is lower, other is unchanged", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration < 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration == 937.5f
            );
        }

        [Test]
        public void TestSamePositionSelectedDragForward()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragForward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Both durations are higher", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration > 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration > 937.5f
            );
        }

        [Test]
        public void TestSamePositionSelectedDragBackward()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Both durations are lower", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration < 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration < 937.5f
            );
        }

        [Test]
        public void TestSelectedButDifferentPositions()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2404, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is unchanged, other is lower", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration == 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration < 937.5f
            );
        }

        [Test]
        public void TestSelectedSameStartTimeDifferentDurations()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2170, Duration = 1171.8, Column = 1 }
                ]);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag until both match", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                InputManager.MoveMouseTo(blueprintDragArea);
                InputManager.PressKey(Key.LShift);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(new Vector2(1000, 110));
            });

            AddStep("Continue the drag", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is unchanged, other is lower", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration == 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration < 937.5f
            );
        }

        [Test]
        public void TestSelectedSameDurationDifferentStartTimes()
        {
            AddStep("Add hold notes", () =>
            {
                EditorBeatmap.AddRange([
                    new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                    new HoldNote { StartTime = 2638.7, Duration = 937.5, Column = 1 }
                ]);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var blueprintDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(blueprintDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is unchanged, other is lower", () =>
                ((HoldNote)EditorBeatmap.HitObjects[0]).Duration == 937.5f &&
                ((HoldNote)EditorBeatmap.HitObjects[^1]).Duration < 937.5f
            );
        }

        private void dragForward(DragArea dragArea)
        {
            InputManager.MoveMouseTo(dragArea);
            InputManager.PressButton(MouseButton.Left);
            InputManager.MoveMouseTo(new Vector2(1100, 110));
        }

        private void dragBackward(DragArea dragArea)
        {
            InputManager.MoveMouseTo(dragArea);
            InputManager.PressButton(MouseButton.Left);
            InputManager.MoveMouseTo(new Vector2(700, 110));
        }
    }
}
