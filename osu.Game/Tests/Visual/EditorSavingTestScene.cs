// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Input;
using osu.Framework.Testing;
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
    public abstract class EditorSavingTestScene : OsuGameTestScene
    {
        protected Editor Editor => Game.ChildrenOfType<Editor>().FirstOrDefault();

        protected EditorBeatmap EditorBeatmap => (EditorBeatmap)Editor.Dependencies.Get(typeof(EditorBeatmap));

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set default beatmap", () => Game.Beatmap.SetDefault());

            PushAndConfirm(() => new EditorLoader());

            AddUntilStep("wait for editor load", () => Editor?.IsLoaded == true);

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
            AddStep("Exit", () => InputManager.Key(Key.Escape));

            AddUntilStep("Wait for main menu", () => Game.ScreenStack.CurrentScreen is MainMenu);

            SongSelect songSelect = null;

            PushAndConfirm(() => songSelect = new PlaySongSelect());
            AddUntilStep("wait for carousel load", () => songSelect.BeatmapSetsLoaded);

            AddUntilStep("Wait for beatmap selected", () => !Game.Beatmap.IsDefault);
            AddStep("Open options", () => InputManager.Key(Key.F3));
            AddStep("Enter editor", () => InputManager.Key(Key.Number5));

            AddUntilStep("Wait for editor load", () => Editor != null);
        }
    }
}
