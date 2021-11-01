// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorSaving : OsuGameTestScene
    {
        private Editor editor => Game.ChildrenOfType<Editor>().FirstOrDefault();

        private EditorBeatmap editorBeatmap => (EditorBeatmap)editor.Dependencies.Get(typeof(EditorBeatmap));

        /// <summary>
        /// Tests the general expected flow of creating a new beatmap, saving it, then loading it back from song select.
        /// </summary>
        [Test]
        public void TestNewBeatmapSaveThenLoad()
        {
            AddStep("set default beatmap", () => Game.Beatmap.SetDefault());

            PushAndConfirm(() => new EditorLoader());

            AddUntilStep("wait for editor load", () => editor?.IsLoaded == true);

            AddUntilStep("wait for metadata screen load", () => editor.ChildrenOfType<MetadataSection>().FirstOrDefault()?.IsLoaded == true);

            // We intentionally switch away from the metadata screen, else there is a feedback loop with the textbox handling which causes metadata changes below to get overwritten.

            AddStep("Enter compose mode", () => InputManager.Key(Key.F1));
            AddUntilStep("Wait for compose mode load", () => editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true);

            AddStep("Set overall difficulty", () => editorBeatmap.Difficulty.OverallDifficulty = 7);
            AddStep("Set artist and title", () =>
            {
                editorBeatmap.BeatmapInfo.Metadata.Artist = "artist";
                editorBeatmap.BeatmapInfo.Metadata.Title = "title";
            });
            AddStep("Set difficulty name", () => editorBeatmap.BeatmapInfo.Version = "difficulty");

            AddStep("Add timing point", () => editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint()));

            AddStep("Change to placement mode", () => InputManager.Key(Key.Number2));
            AddStep("Move to playfield", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));
            AddStep("Place single hitcircle", () => InputManager.Click(MouseButton.Left));

            checkMutations();

            // After placement these must be non-default as defaults are read-only.
            AddAssert("Placed object has non-default control points", () =>
                editorBeatmap.HitObjects[0].SampleControlPoint != SampleControlPoint.DEFAULT &&
                editorBeatmap.HitObjects[0].DifficultyControlPoint != DifficultyControlPoint.DEFAULT);

            AddStep("Save", () => InputManager.Keys(PlatformAction.Save));

            checkMutations();

            AddStep("Exit", () => InputManager.Key(Key.Escape));

            AddUntilStep("Wait for main menu", () => Game.ScreenStack.CurrentScreen is MainMenu);

            PushAndConfirm(() => new PlaySongSelect());

            AddUntilStep("Wait for beatmap selected", () => !Game.Beatmap.IsDefault);
            AddStep("Open options", () => InputManager.Key(Key.F3));
            AddStep("Enter editor", () => InputManager.Key(Key.Number5));

            AddUntilStep("Wait for editor load", () => editor != null);

            checkMutations();
        }

        private void checkMutations()
        {
            AddAssert("Beatmap contains single hitcircle", () => editorBeatmap.HitObjects.Count == 1);
            AddAssert("Beatmap has correct overall difficulty", () => editorBeatmap.Difficulty.OverallDifficulty == 7);
            AddAssert("Beatmap has correct metadata", () => editorBeatmap.BeatmapInfo.Metadata.Artist == "artist" && editorBeatmap.BeatmapInfo.Metadata.Title == "title");
            AddAssert("Beatmap has correct difficulty name", () => editorBeatmap.BeatmapInfo.Version == "difficulty");
        }
    }
}
