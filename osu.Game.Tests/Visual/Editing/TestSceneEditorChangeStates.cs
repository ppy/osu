// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorChangeStates : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Test]
        public void TestSelectedObjects()
        {
            HitCircle obj = null;
            AddStep("add hitobject", () => EditorBeatmap.Add(obj = new HitCircle { StartTime = 1000 }));
            AddStep("select hitobject", () => EditorBeatmap.SelectedHitObjects.Add(obj));
            AddAssert("confirm 1 selected", () => EditorBeatmap.SelectedHitObjects.Count == 1);
            AddStep("deselect hitobject", () => EditorBeatmap.SelectedHitObjects.Remove(obj));
            AddAssert("confirm 0 selected", () => EditorBeatmap.SelectedHitObjects.Count == 0);
        }

        [Test]
        public void TestUndoFromInitialState()
        {
            int hitObjectCount = 0;

            AddStep("get initial state", () => hitObjectCount = EditorBeatmap.HitObjects.Count);

            addUndoSteps();

            AddAssert("no change occurred", () => hitObjectCount == EditorBeatmap.HitObjects.Count);
            AddAssert("no unsaved changes", () => !Editor.HasUnsavedChanges);
        }

        [Test]
        public void TestRedoFromInitialState()
        {
            int hitObjectCount = 0;

            AddStep("get initial state", () => hitObjectCount = EditorBeatmap.HitObjects.Count);

            addRedoSteps();

            AddAssert("no change occurred", () => hitObjectCount == EditorBeatmap.HitObjects.Count);
            AddAssert("no unsaved changes", () => !Editor.HasUnsavedChanges);
        }

        [Test]
        public void TestAddObjectAndUndo()
        {
            HitObject addedObject = null;
            HitObject removedObject = null;
            HitObject expectedObject = null;

            AddStep("bind removal", () =>
            {
                EditorBeatmap.HitObjectAdded += h => addedObject = h;
                EditorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => EditorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
            AddAssert("hitobject added", () => addedObject == expectedObject);
            AddAssert("unsaved changes", () => Editor.HasUnsavedChanges);

            addUndoSteps();
            AddAssert("hitobject removed", () => removedObject == expectedObject);
            AddAssert("no unsaved changes", () => !Editor.HasUnsavedChanges);
        }

        [Test]
        public void TestAddObjectThenUndoThenRedo()
        {
            HitObject addedObject = null;
            HitObject removedObject = null;
            HitObject expectedObject = null;

            AddStep("bind removal", () =>
            {
                EditorBeatmap.HitObjectAdded += h => addedObject = h;
                EditorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => EditorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
            addUndoSteps();

            AddStep("reset variables", () =>
            {
                addedObject = null;
                removedObject = null;
            });

            addRedoSteps();
            AddAssert("hitobject added", () => addedObject.StartTime == expectedObject.StartTime); // Can't compare via equality (new hitobject instance)
            AddAssert("no hitobject removed", () => removedObject == null);
            AddAssert("unsaved changes", () => Editor.HasUnsavedChanges);
        }

        [Test]
        public void TestAddObjectThenSaveHasNoUnsavedChanges()
        {
            AddStep("add hitobject", () => EditorBeatmap.Add(new HitCircle { StartTime = 1000 }));

            AddAssert("unsaved changes", () => Editor.HasUnsavedChanges);
            AddStep("save changes", () => Editor.Save());
            AddAssert("no unsaved changes", () => !Editor.HasUnsavedChanges);
        }

        [Test]
        public void TestRemoveObjectThenUndo()
        {
            HitObject addedObject = null;
            HitObject removedObject = null;
            HitObject expectedObject = null;

            AddStep("bind removal", () =>
            {
                EditorBeatmap.HitObjectAdded += h => addedObject = h;
                EditorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => EditorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
            AddStep("remove object", () => EditorBeatmap.Remove(expectedObject));
            AddStep("reset variables", () =>
            {
                addedObject = null;
                removedObject = null;
            });

            addUndoSteps();
            AddAssert("hitobject added", () => addedObject.StartTime == expectedObject.StartTime); // Can't compare via equality (new hitobject instance)
            AddAssert("no hitobject removed", () => removedObject == null);
            AddAssert("unsaved changes", () => Editor.HasUnsavedChanges); // 2 steps performed, 1 undone
        }

        [Test]
        public void TestRemoveObjectThenUndoThenRedo()
        {
            HitObject addedObject = null;
            HitObject removedObject = null;
            HitObject expectedObject = null;

            AddStep("bind removal", () =>
            {
                EditorBeatmap.HitObjectAdded += h => addedObject = h;
                EditorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => EditorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
            AddStep("remove object", () => EditorBeatmap.Remove(expectedObject));
            addUndoSteps();

            AddStep("reset variables", () =>
            {
                addedObject = null;
                removedObject = null;
            });

            addRedoSteps();
            AddAssert("hitobject removed", () => removedObject.StartTime == expectedObject.StartTime); // Can't compare via equality (new hitobject instance after undo)
            AddAssert("no hitobject added", () => addedObject == null);
            AddAssert("no changes", () => !Editor.HasUnsavedChanges); // end result is empty beatmap, matching original state
        }

        private void addUndoSteps() => AddStep("undo", () => Editor.Undo());

        private void addRedoSteps() => AddStep("redo", () => Editor.Redo());
    }
}
