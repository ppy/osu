// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Framework.Utils;
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
            AddStep("Add hold note", () => EditorBeatmap.Add(getMatchingNotes()[0]));

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().Single();
                dragForward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is higher", () => getFirstNote().Duration > 937.5f);
        }

        [Test]
        public void TestSimpleTailDragBackwards()
        {
            AddStep("Add hold note", () => EditorBeatmap.Add(getMatchingNotes()[0]));

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().Single();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower", () => getFirstNote().Duration < 937.5f);
        }

        [Test]
        public void TestSamePositionButNotSelectedDragForward()
        {
            AddStep("Add hold notes", () => EditorBeatmap.AddRange(getMatchingNotes()));

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragForward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is higher, other is unchanged", () =>
                getFirstNote().Duration > 937.5f && Precision.AlmostEquals(getLastNote().Duration, 937.5f)
            );
        }

        [Test]
        public void TestSamePositionButNotSelectedDragBackward()
        {
            AddStep("Add hold notes", () => EditorBeatmap.AddRange(getMatchingNotes()));

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower, other is unchanged", () =>
                getFirstNote().Duration < 937.5f && Precision.AlmostEquals(getLastNote().Duration, 937.5f)
            );
        }

        [Test]
        public void TestSamePositionSelectedDragForward()
        {
            AddStep("Add hold notes", () => EditorBeatmap.AddRange(getMatchingNotes()));

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragForward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Both durations are higher", () =>
                getFirstNote().Duration > 937.5f && getLastNote().Duration > 937.5f
            );
        }

        [Test]
        public void TestSamePositionSelectedDragBackward()
        {
            AddStep("Add hold notes", () => EditorBeatmap.AddRange(getMatchingNotes()));

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Both durations are lower", () =>
                getFirstNote().Duration < 937.5f && getLastNote().Duration < 937.5f
            );
        }

        [Test]
        public void TestSelectedButDifferentPositions()
        {
            AddStep("Add hold notes", () =>
            {
                var unmatchingNotes = getMatchingNotes();
                unmatchingNotes[^1].StartTime = 2404;

                EditorBeatmap.AddRange(unmatchingNotes);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is unchanged, other is lower", () =>
                Precision.AlmostEquals(getFirstNote().Duration, 937.5f) && getLastNote().Duration < 937.5f
            );
        }

        [Test]
        public void TestSelectedSameStartTimeDifferentDurations()
        {
            AddStep("Add hold notes", () =>
            {
                var unmatchingNotes = getMatchingNotes();
                unmatchingNotes[^1].Duration = 1171.8;

                EditorBeatmap.AddRange(unmatchingNotes);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag until both match", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                InputManager.MoveMouseTo(noteDragArea);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(new Vector2(1000, 110));
            });

            AddStep("Continue the drag", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is unchanged, other is lower", () =>
                Precision.AlmostEquals(getFirstNote().Duration, 937.5f) && getLastNote().Duration < 937.5f
            );
        }

        [Test]
        public void TestSelectedSameDurationDifferentStartTimes()
        {
            AddStep("Add hold notes", () =>
            {
                var unmatchingNotes = getMatchingNotes();
                unmatchingNotes[^1].StartTime = 2638.7;

                EditorBeatmap.AddRange(unmatchingNotes);
            });

            AddStep("Select all", () =>
            {
                EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects);
            });

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is unchanged, other is lower", () =>
                Precision.AlmostEquals(getFirstNote().Duration, 937.5f) && getLastNote().Duration < 937.5f
            );
        }

        [Test]
        public void TestDragNoteOutsideOfSelection()
        {
            AddStep("Add hold notes", () => EditorBeatmap.AddRange(getMatchingNotes()));

            AddStep("Select the back stack slider", () =>
            {
                EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.Last());
            });

            AddStep("Drag tail", () =>
            {
                var noteDragArea = this.ChildrenOfType<DragArea>().First();
                dragBackward(noteDragArea);
            });

            AddStep("Release tail", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("Duration is lower, other is unchanged", () =>
                getFirstNote().Duration < 937.5f && Precision.AlmostEquals(getLastNote().Duration, 937.5f)
            );
        }

        private HoldNote getFirstNote()
        {
            return (HoldNote)EditorBeatmap.HitObjects[0];
        }

        private HoldNote getLastNote()
        {
            return (HoldNote)EditorBeatmap.HitObjects[^1];
        }

        private HoldNote[] getMatchingNotes()
        {
            return
            [
                new HoldNote { StartTime = 2170, Duration = 937.5, Column = 0 },
                new HoldNote { StartTime = 2170, Duration = 937.5, Column = 1 }
            ];
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
