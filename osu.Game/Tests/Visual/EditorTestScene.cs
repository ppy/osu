// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestScene : ScreenTestScene
    {
        protected EditorBeatmap EditorBeatmap;

        protected TestEditor Editor { get; private set; }

        protected EditorClock EditorClock { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
        }

        protected virtual bool EditorComponentsReady => Editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true
                                                        && Editor.ChildrenOfType<TimelineArea>().FirstOrDefault()?.IsLoaded == true;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load editor", () => LoadScreen(Editor = CreateEditor()));
            AddUntilStep("wait for editor to load", () => EditorComponentsReady);
            AddStep("get beatmap", () => EditorBeatmap = Editor.ChildrenOfType<EditorBeatmap>().Single());
            AddStep("get clock", () => EditorClock = Editor.ChildrenOfType<EditorClock>().Single());
        }

        /// <summary>
        /// Creates the ruleset for providing a corresponding beatmap to load the editor on.
        /// </summary>
        [NotNull]
        protected abstract Ruleset CreateEditorRuleset();

        protected sealed override Ruleset CreateRuleset() => CreateEditorRuleset();

        protected virtual TestEditor CreateEditor() => new TestEditor();

        protected class TestEditor : Editor
        {
            public new void Undo() => base.Undo();

            public new void Redo() => base.Redo();

            public new void Save() => base.Save();

            public new void Cut() => base.Cut();

            public new void Copy() => base.Copy();

            public new void Paste() => base.Paste();

            public new bool HasUnsavedChanges => base.HasUnsavedChanges;
        }
    }
}
