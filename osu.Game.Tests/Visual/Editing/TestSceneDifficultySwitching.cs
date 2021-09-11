// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Menus;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneDifficultySwitching : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override bool IsolateSavingFromDatabase => false;

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private BeatmapSetInfo importedBeatmapSet;

        public override void SetUpSteps()
        {
            AddStep("import test beatmap", () => importedBeatmapSet = ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Result);
            base.SetUpSteps();
        }

        protected override void LoadEditor()
        {
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First());
            base.LoadEditor();
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

        [Test]
        public void TestPreventSwitchDueToUnsavedChanges()
        {
            BeatmapInfo targetDifficulty = null;
            PromptForSaveDialog saveDialog = null;

            AddStep("remove first hitobject", () => EditorBeatmap.RemoveAt(0));

            AddStep("set target difficulty", () => targetDifficulty = importedBeatmapSet.Beatmaps.Last(beatmap => !beatmap.Equals(Beatmap.Value.BeatmapInfo)));
            switchToDifficulty(() => targetDifficulty);

            AddUntilStep("prompt for save dialog shown", () =>
            {
                saveDialog = this.ChildrenOfType<PromptForSaveDialog>().Single();
                return saveDialog != null;
            });
            AddStep("continue editing", () =>
            {
                var continueButton = saveDialog.ChildrenOfType<PopupDialogCancelButton>().Last();
                continueButton.TriggerClick();
            });

            confirmEditingBeatmap(() => importedBeatmapSet.Beatmaps.First());

            AddRepeatStep("exit editor forcefully", () => Stack.Exit(), 2);
            // ensure editor loader didn't resume.
            AddAssert("stack empty", () => Stack.CurrentScreen == null);
        }

        [Test]
        public void TestAllowSwitchAfterDiscardingUnsavedChanges()
        {
            BeatmapInfo targetDifficulty = null;
            PromptForSaveDialog saveDialog = null;

            AddStep("remove first hitobject", () => EditorBeatmap.RemoveAt(0));

            AddStep("set target difficulty", () => targetDifficulty = importedBeatmapSet.Beatmaps.Last(beatmap => !beatmap.Equals(Beatmap.Value.BeatmapInfo)));
            switchToDifficulty(() => targetDifficulty);

            AddUntilStep("prompt for save dialog shown", () =>
            {
                saveDialog = this.ChildrenOfType<PromptForSaveDialog>().Single();
                return saveDialog != null;
            });
            AddStep("discard changes", () =>
            {
                var continueButton = saveDialog.ChildrenOfType<PopupDialogOkButton>().Single();
                continueButton.TriggerClick();
            });

            confirmEditingBeatmap(() => targetDifficulty);

            AddStep("exit editor forcefully", () => Stack.Exit());
            // ensure editor loader didn't resume.
            AddAssert("stack empty", () => Stack.CurrentScreen == null);
        }

        private void switchToDifficulty(Func<BeatmapInfo> difficulty)
        {
            AddUntilStep("wait for menubar to load", () => Editor.ChildrenOfType<EditorMenuBar>().Any());
            AddStep("open file menu", () =>
            {
                var menuBar = Editor.ChildrenOfType<EditorMenuBar>().Single();
                var fileMenu = menuBar.ChildrenOfType<DrawableOsuMenuItem>().First();
                InputManager.MoveMouseTo(fileMenu);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("open difficulty menu", () =>
            {
                var difficultySelector =
                    Editor.ChildrenOfType<DrawableOsuMenuItem>().Single(item => item.Item.Text.Value.ToString().Contains("Change difficulty"));
                InputManager.MoveMouseTo(difficultySelector);
            });
            AddWaitStep("wait for open", 3);

            AddStep("switch to target difficulty", () =>
            {
                var difficultyMenuItem =
                    Editor.ChildrenOfType<DrawableOsuMenuItem>()
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
