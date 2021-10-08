// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
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

            AddUntilStep("wait for editor load", () => editor != null);

            AddStep("Add timing point", () => editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint()));

            AddStep("Enter compose mode", () => InputManager.Key(Key.F1));
            AddUntilStep("Wait for compose mode load", () => editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true);

            AddStep("Change to placement mode", () => InputManager.Key(Key.Number2));
            AddStep("Move to playfield", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));
            AddStep("Place single hitcircle", () => InputManager.Click(MouseButton.Left));

            AddStep("Save and exit", () =>
            {
                InputManager.Keys(PlatformAction.Save);
                InputManager.Key(Key.Escape);
            });

            AddUntilStep("Wait for main menu", () => Game.ScreenStack.CurrentScreen is MainMenu);

            PushAndConfirm(() => new PlaySongSelect());

            AddUntilStep("Wait for beatmap selected", () => !Game.Beatmap.IsDefault);
            AddStep("Open options", () => InputManager.Key(Key.F3));
            AddStep("Enter editor", () => InputManager.Key(Key.Number5));

            AddUntilStep("Wait for editor load", () => editor != null);
            AddAssert("Beatmap contains single hitcircle", () => editorBeatmap.HitObjects.Count == 1);
        }
    }
}
