// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual
{
    /// <summary>
    /// Tests the general expected flow of creating a new beatmap, saving it, then loading it back from song select.
    /// </summary>
    public abstract partial class EditorSavingTestScene : OsuGameTestScene
    {
        protected Editor Editor => Game.ChildrenOfType<Editor>().FirstOrDefault();

        protected EditorBeatmap EditorBeatmap => (EditorBeatmap)Editor.Dependencies.Get(typeof(EditorBeatmap));

        [CanBeNull]
        protected Func<WorkingBeatmap> CreateInitialBeatmap { get; set; }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            if (CreateInitialBeatmap == null)
                AddStep("set default beatmap", () => Game.Beatmap.SetDefault());
            else
            {
                AddStep("set test beatmap", () => Game.Beatmap.Value = CreateInitialBeatmap?.Invoke());
            }

            PushAndConfirm(() => new EditorLoader());

            AddUntilStep("wait for editor load", () => Editor?.IsLoaded == true);

            if (CreateInitialBeatmap == null)
                AddUntilStep("wait for metadata screen load", () => Editor.ChildrenOfType<MetadataSection>().FirstOrDefault()?.IsLoaded == true);

            // We intentionally switch away from the metadata screen, else there is a feedback loop with the textbox handling which causes metadata changes below to get overwritten.

            AddStep("Enter compose mode", () => InputManager.Key(Key.F1));
            AddUntilStep("Wait for compose mode load", () => Editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true);
        }

        protected void SaveEditor()
        {
            AddStep("Save", () => InputManager.Keys(PlatformAction.Save));
        }

        protected void ReloadEditorToSameBeatmap()
        {
            Guid beatmapSetGuid = Guid.Empty;
            Guid beatmapGuid = Guid.Empty;

            AddStep("Store beatmap GUIDs", () =>
            {
                beatmapSetGuid = EditorBeatmap.BeatmapInfo.BeatmapSet!.ID;
                beatmapGuid = EditorBeatmap.BeatmapInfo.ID;
            });
            AddStep("Exit", () => InputManager.Key(Key.Escape));

            AddUntilStep("Wait for main menu", () => Game.ScreenStack.CurrentScreen is MainMenu);

            SongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new PlaySongSelect());
            AddUntilStep("wait for carousel load", () => songSelect.BeatmapSetsLoaded);

            AddStep("Present same beatmap", () => Game.PresentBeatmap(Game.BeatmapManager.QueryBeatmapSet(set => set.ID == beatmapSetGuid)!.Value, beatmap => beatmap.ID == beatmapGuid));
            AddUntilStep("Wait for beatmap selected", () => Game.Beatmap.Value.BeatmapInfo.ID == beatmapGuid);
            AddStep("Open options", () => InputManager.Key(Key.F3));
            AddStep("Enter editor", () => InputManager.Key(Key.Number5));

            AddUntilStep("Wait for editor load", () => Editor != null);
        }
    }
}
