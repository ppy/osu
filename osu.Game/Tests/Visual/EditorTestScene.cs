// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.IO.Stores;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.IO.Archives;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestScene : ScreenTestScene
    {
        protected EditorBeatmap EditorBeatmap;

        protected TestEditor Editor { get; private set; }

        protected EditorClock EditorClock { get; private set; }

        /// <summary>
        /// Whether any saves performed by the editor should be isolate (and not persist) to the underlying <see cref="BeatmapManager"/>.
        /// </summary>
        protected virtual bool IsolateSavingFromDatabase => true;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio, RulesetStore rulesets)
        {
            var working = CreateWorkingBeatmap(Ruleset.Value);

            Beatmap.Value = working;

            if (IsolateSavingFromDatabase)
                Dependencies.CacheAs<BeatmapManager>(new TestBeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default, working));
        }

        protected virtual bool EditorComponentsReady => Editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true
                                                        && Editor.ChildrenOfType<TimelineArea>().FirstOrDefault()?.IsLoaded == true;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load editor", LoadEditor);
            AddUntilStep("wait for editor to load", () => EditorComponentsReady);
            AddStep("get beatmap", () => EditorBeatmap = Editor.ChildrenOfType<EditorBeatmap>().Single());
            AddStep("get clock", () => EditorClock = Editor.ChildrenOfType<EditorClock>().Single());
        }

        protected virtual void LoadEditor()
        {
            LoadScreen(Editor = CreateEditor());
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

        private class TestBeatmapManager : BeatmapManager
        {
            private readonly WorkingBeatmap testBeatmap;

            public TestBeatmapManager(Storage storage, IDatabaseContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, [NotNull] AudioManager audioManager, IResourceStore<byte[]> resources, GameHost host, WorkingBeatmap defaultBeatmap, WorkingBeatmap testBeatmap)
                : base(storage, contextFactory, rulesets, api, audioManager, resources, host, defaultBeatmap, false)
            {
                this.testBeatmap = testBeatmap;
            }

            protected override string ComputeHash(BeatmapSetInfo item, ArchiveReader reader = null)
                => string.Empty;

            public override WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo)
                => testBeatmap;

            public override void Save(BeatmapInfo info, IBeatmap beatmapContent, ISkin beatmapSkin = null)
            {
                // don't actually care about saving for this context.
            }
        }
    }
}
