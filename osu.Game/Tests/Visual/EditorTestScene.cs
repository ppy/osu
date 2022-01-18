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
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Menu;
using osu.Game.Skinning;

namespace osu.Game.Tests.Visual
{
    public abstract class EditorTestScene : ScreenTestScene
    {
        private TestEditorLoader editorLoader;

        protected TestEditor Editor => editorLoader.Editor;

        protected EditorBeatmap EditorBeatmap => Editor.ChildrenOfType<EditorBeatmap>().Single();
        protected EditorClock EditorClock => Editor.ChildrenOfType<EditorClock>().Single();

        /// <summary>
        /// Whether any saves performed by the editor should be isolate (and not persist) to the underlying <see cref="BeatmapManager"/>.
        /// </summary>
        protected virtual bool IsolateSavingFromDatabase => true;

        // required for screen transitions to work properly
        // (see comment in EditorLoader.LogoArriving).
        [Cached]
        private OsuLogo logo = new OsuLogo
        {
            Alpha = 0
        };

        private TestBeatmapManager testBeatmapManager;
        private WorkingBeatmap working;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio, RulesetStore rulesets)
        {
            Add(logo);

            working = CreateWorkingBeatmap(Ruleset.Value);

            if (IsolateSavingFromDatabase)
                Dependencies.CacheAs<BeatmapManager>(testBeatmapManager = new TestBeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = working;
            if (testBeatmapManager != null)
                testBeatmapManager.TestBeatmap = working;
        }

        protected virtual bool EditorComponentsReady => Editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true
                                                        && Editor.ChildrenOfType<TimelineArea>().FirstOrDefault()?.IsLoaded == true;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("load editor", LoadEditor);
            AddUntilStep("wait for editor to load", () => EditorComponentsReady);
        }

        protected virtual void LoadEditor()
        {
            LoadScreen(editorLoader = new TestEditorLoader());
        }

        /// <summary>
        /// Creates the ruleset for providing a corresponding beatmap to load the editor on.
        /// </summary>
        [NotNull]
        protected abstract Ruleset CreateEditorRuleset();

        protected sealed override Ruleset CreateRuleset() => CreateEditorRuleset();

        protected class TestEditorLoader : EditorLoader
        {
            public TestEditor Editor { get; private set; }

            protected sealed override Editor CreateEditor() => Editor = CreateTestEditor(this);

            protected virtual TestEditor CreateTestEditor(EditorLoader loader) => new TestEditor(loader);
        }

        protected class TestEditor : Editor
        {
            public new void Undo() => base.Undo();

            public new void Redo() => base.Redo();

            public new void Save() => base.Save();

            public new void Cut() => base.Cut();

            public new void Copy() => base.Copy();

            public new void Paste() => base.Paste();

            public new void SwitchToDifficulty(BeatmapInfo beatmapInfo) => base.SwitchToDifficulty(beatmapInfo);

            public new bool HasUnsavedChanges => base.HasUnsavedChanges;

            public TestEditor(EditorLoader loader = null)
                : base(loader)
            {
            }
        }

        private class TestBeatmapManager : BeatmapManager
        {
            public WorkingBeatmap TestBeatmap;

            public TestBeatmapManager(Storage storage, RealmContextFactory contextFactory, RulesetStore rulesets, IAPIProvider api, [NotNull] AudioManager audioManager, IResourceStore<byte[]> resources, GameHost host, WorkingBeatmap defaultBeatmap)
                : base(storage, contextFactory, rulesets, api, audioManager, resources, host, defaultBeatmap)
            {
            }

            protected override BeatmapModelManager CreateBeatmapModelManager(Storage storage, RealmContextFactory contextFactory, RulesetStore rulesets, BeatmapOnlineLookupQueue onlineLookupQueue)
            {
                return new TestBeatmapModelManager(storage, contextFactory, rulesets, onlineLookupQueue);
            }

            protected override WorkingBeatmapCache CreateWorkingBeatmapCache(AudioManager audioManager, IResourceStore<byte[]> resources, IResourceStore<byte[]> storage, WorkingBeatmap defaultBeatmap, GameHost host)
            {
                return new TestWorkingBeatmapCache(this, audioManager, resources, storage, defaultBeatmap, host);
            }

            private class TestWorkingBeatmapCache : WorkingBeatmapCache
            {
                private readonly TestBeatmapManager testBeatmapManager;

                public TestWorkingBeatmapCache(TestBeatmapManager testBeatmapManager, AudioManager audioManager, IResourceStore<byte[]> resourceStore, IResourceStore<byte[]> storage, WorkingBeatmap defaultBeatmap, GameHost gameHost)
                    : base(testBeatmapManager.BeatmapTrackStore, audioManager, resourceStore, storage, defaultBeatmap, gameHost)
                {
                    this.testBeatmapManager = testBeatmapManager;
                }

                public override WorkingBeatmap GetWorkingBeatmap(BeatmapInfo beatmapInfo)
                    => testBeatmapManager.TestBeatmap;
            }

            internal class TestBeatmapModelManager : BeatmapModelManager
            {
                public TestBeatmapModelManager(Storage storage, RealmContextFactory databaseContextFactory, RulesetStore rulesetStore, BeatmapOnlineLookupQueue beatmapOnlineLookupQueue)
                    : base(databaseContextFactory, storage, beatmapOnlineLookupQueue)
                {
                }

                protected override string ComputeHash(BeatmapSetInfo item)
                    => string.Empty;
            }

            public override void Save(BeatmapInfo info, IBeatmap beatmapContent, ISkin beatmapSkin = null)
            {
                // don't actually care about saving for this context.
            }
        }
    }
}
