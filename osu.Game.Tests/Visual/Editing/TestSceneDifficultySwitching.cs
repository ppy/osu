// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Menus;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneDifficultySwitching : ScreenTestScene
    {
        private BeatmapSetInfo importedBeatmapSet;

        [Resolved]
        private OsuGameBase game { get; set; }

        private Editor editor;

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [SetUpSteps]
        public void SetUp()
        {
            AddStep("import test beatmap", () => importedBeatmapSet = ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Result);

            AddStep("set current beatmap", () => Beatmap.Value = beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First()));
            AddStep("push loader", () => Stack.Push(new EditorLoader()));

            AddUntilStep("wait for editor to load", () => Stack.CurrentScreen is Editor);
            AddStep("store editor", () => editor = (Editor)Stack.CurrentScreen);
        }

        [Test]
        public void TestBasicSwitch()
        {
            BeatmapInfo targetDifficulty = null;

            AddStep("set target difficulty", () => targetDifficulty = importedBeatmapSet.Beatmaps.Last(beatmap => !beatmap.Equals(Beatmap.Value.BeatmapInfo)));
            switchToDifficulty(() => targetDifficulty);
            confirmEditingBeatmap(() => targetDifficulty);

            AddStep("exit editor", () => Stack.Exit());
            // ensure editor loader didn't resume.
            AddAssert("stack empty", () => Stack.CurrentScreen == null);
        }

        private void switchToDifficulty(Func<BeatmapInfo> difficulty)
        {
            AddUntilStep("wait for menubar to load", () => editor.ChildrenOfType<EditorMenuBar>().Any());
            AddStep("open file menu", () =>
            {
                var menuBar = editor.ChildrenOfType<EditorMenuBar>().Single();
                var fileMenu = menuBar.ChildrenOfType<DrawableOsuMenuItem>().First();
                InputManager.MoveMouseTo(fileMenu);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("open difficulty menu", () =>
            {
                var difficultySelector =
                    editor.ChildrenOfType<DrawableOsuMenuItem>().Single(item => item.Item.Text.Value.ToString().Contains("Change difficulty"));
                InputManager.MoveMouseTo(difficultySelector);
            });
            AddWaitStep("wait for open", 3);

            AddStep("switch to target difficulty", () =>
            {
                var difficultyMenuItem =
                    editor.ChildrenOfType<DrawableOsuMenuItem>()
                          .Last(item => item.Item is DifficultyMenuItem difficultyItem && difficultyItem.Beatmap.Equals(difficulty.Invoke()));
                InputManager.MoveMouseTo(difficultyMenuItem);
                InputManager.Click(MouseButton.Left);
            });
        }

        private void confirmEditingBeatmap(Func<BeatmapInfo> targetDifficulty)
        {
            AddUntilStep("current beatmap is correct", () => Beatmap.Value.BeatmapInfo.Equals(targetDifficulty.Invoke()));
            AddUntilStep("current screen is editor", () => Stack.CurrentScreen is Editor);
        }
    }
}
