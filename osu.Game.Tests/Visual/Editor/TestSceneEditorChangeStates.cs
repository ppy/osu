// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Tests.Visual.Editor
{
    public class TestSceneEditorChangeStates : ScreenTestScene
    {
        private EditorBeatmap editorBeatmap;
        private TestEditor editor;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load editor", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
                LoadScreen(editor = new TestEditor());
            });

            AddUntilStep("wait for editor to load", () => editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true
                                                          && editor.ChildrenOfType<TimelineArea>().FirstOrDefault()?.IsLoaded == true);
            AddStep("get beatmap", () => editorBeatmap = editor.ChildrenOfType<EditorBeatmap>().Single());
        }

        [Test]
        public void TestUndoFromInitialState()
        {
            int hitObjectCount = 0;

            AddStep("get initial state", () => hitObjectCount = editorBeatmap.HitObjects.Count);

            addUndoSteps();

            AddAssert("no change occurred", () => hitObjectCount == editorBeatmap.HitObjects.Count);
        }

        [Test]
        public void TestRedoFromInitialState()
        {
            int hitObjectCount = 0;

            AddStep("get initial state", () => hitObjectCount = editorBeatmap.HitObjects.Count);

            addRedoSteps();

            AddAssert("no change occurred", () => hitObjectCount == editorBeatmap.HitObjects.Count);
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

            addUndoSteps();
            AddAssert("hitobject removed", () => removedObject == expectedObject);
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
        }

        private void addUndoSteps() => AddStep("undo", () => editor.Undo());

        private void addRedoSteps() => AddStep("redo", () => editor.Redo());

        private class TestEditor : Screens.Edit.Editor
        {
            public new void Undo() => base.Undo();

            public new void Redo() => base.Redo();
        }
    }
}
