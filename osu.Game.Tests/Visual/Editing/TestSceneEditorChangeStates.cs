// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorChangeStates : EditorTestScene
    {
        private EditorBeatmap editorBeatmap;

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected new TestEditor Editor => (TestEditor)base.Editor;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("get beatmap", () => editorBeatmap = Editor.ChildrenOfType<EditorBeatmap>().Single());
        }

        [Test]
        public void TestSelectedObjects()
        {
            HitCircle obj = null;
            AddStep("add hitobject", () => editorBeatmap.Add(obj = new HitCircle { StartTime = 1000 }));
            AddStep("select hitobject", () => editorBeatmap.SelectedHitObjects.Add(obj));
            AddAssert("confirm 1 selected", () => editorBeatmap.SelectedHitObjects.Count == 1);
            AddStep("deselect hitobject", () => editorBeatmap.SelectedHitObjects.Remove(obj));
            AddAssert("confirm 0 selected", () => editorBeatmap.SelectedHitObjects.Count == 0);
        }

        [Test]
        public void TestUndoFromInitialState()
        {
            int hitObjectCount = 0;

            AddStep("get initial state", () => hitObjectCount = editorBeatmap.HitObjects.Count);

            addUndoSteps();

            AddAssert("no change occurred", () => hitObjectCount == editorBeatmap.HitObjects.Count);
            AddAssert("no unsaved changes", () => !Editor.HasUnsavedChanges);
        }

        [Test]
        public void TestRedoFromInitialState()
        {
            int hitObjectCount = 0;

            AddStep("get initial state", () => hitObjectCount = editorBeatmap.HitObjects.Count);

            addRedoSteps();

            AddAssert("no change occurred", () => hitObjectCount == editorBeatmap.HitObjects.Count);
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
                editorBeatmap.HitObjectAdded += h => addedObject = h;
                editorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => editorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
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
                editorBeatmap.HitObjectAdded += h => addedObject = h;
                editorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => editorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
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
            AddStep("add hitobject", () => editorBeatmap.Add(new HitCircle { StartTime = 1000 }));

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
                editorBeatmap.HitObjectAdded += h => addedObject = h;
                editorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => editorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
            AddStep("remove object", () => editorBeatmap.Remove(expectedObject));
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
                editorBeatmap.HitObjectAdded += h => addedObject = h;
                editorBeatmap.HitObjectRemoved += h => removedObject = h;
            });

            AddStep("add hitobject", () => editorBeatmap.Add(expectedObject = new HitCircle { StartTime = 1000 }));
            AddStep("remove object", () => editorBeatmap.Remove(expectedObject));
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

        protected override Editor CreateEditor() => new TestEditor();

        protected class TestEditor : Editor
        {
            public new void Undo() => base.Undo();

            public new void Redo() => base.Redo();

            public new void Save() => base.Save();

            public new bool HasUnsavedChanges => base.HasUnsavedChanges;
        }
    }
}
