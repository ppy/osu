// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Dialog;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps.IO;

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
            AddStep("import test beatmap", () => importedBeatmapSet = BeatmapImportHelper.LoadOszIntoOsu(game, virtualTrack: true).GetResultSafely());
            base.SetUpSteps();
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First());

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
        public void TestClockPositionPreservedBetweenSwitches()
        {
            BeatmapInfo targetDifficulty = null;
            AddStep("seek editor to 00:05:00", () => EditorClock.Seek(5000));

            AddStep("set target difficulty", () => targetDifficulty = importedBeatmapSet.Beatmaps.Last(beatmap => !beatmap.Equals(Beatmap.Value.BeatmapInfo)));
            switchToDifficulty(() => targetDifficulty);
            confirmEditingBeatmap(() => targetDifficulty);
            AddAssert("editor clock at 00:05:00", () => EditorClock.CurrentTime == 5000);

            AddStep("exit editor", () => Stack.Exit());
            // ensure editor loader didn't resume.
            AddAssert("stack empty", () => Stack.CurrentScreen == null);
        }

        [Test]
        public void TestClipboardPreservedAfterSwitch([Values] bool sameRuleset)
        {
            BeatmapInfo targetDifficulty = null;

            AddStep("select first object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.First()));
            AddStep("copy object", () => Editor.Copy());

            AddStep("set target difficulty", () =>
            {
                targetDifficulty = sameRuleset
                    ? importedBeatmapSet.Beatmaps.Last(beatmap => !beatmap.Equals(Beatmap.Value.BeatmapInfo) && beatmap.Ruleset.ShortName == Beatmap.Value.BeatmapInfo.Ruleset.ShortName)
                    : importedBeatmapSet.Beatmaps.Last(beatmap => !beatmap.Equals(Beatmap.Value.BeatmapInfo) && beatmap.Ruleset.ShortName != Beatmap.Value.BeatmapInfo.Ruleset.ShortName);
            });
            switchToDifficulty(() => targetDifficulty);
            confirmEditingBeatmap(() => targetDifficulty);

            AddAssert("no objects selected", () => !EditorBeatmap.SelectedHitObjects.Any());
            AddUntilStep("wait for drawable ruleset", () => Editor.ChildrenOfType<DrawableRuleset>().SingleOrDefault()?.IsLoaded == true);
            AddStep("paste object", () => Editor.Paste());

            if (sameRuleset)
                AddAssert("object was pasted", () => EditorBeatmap.SelectedHitObjects.Any());
            else
                AddAssert("object was not pasted", () => !EditorBeatmap.SelectedHitObjects.Any());

            AddStep("exit editor", () => Stack.Exit());

            if (sameRuleset)
            {
                AddUntilStep("prompt for save dialog shown", () => DialogOverlay.CurrentDialog is PromptForSaveDialog);
                AddStep("discard changes", () => ((PromptForSaveDialog)DialogOverlay.CurrentDialog).PerformOkAction());
            }

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

        private void switchToDifficulty(Func<BeatmapInfo> difficulty) => AddStep("switch to difficulty", () => Editor.SwitchToDifficulty(difficulty.Invoke()));

        private void confirmEditingBeatmap(Func<BeatmapInfo> targetDifficulty)
        {
            AddUntilStep("current beatmap is correct", () => Beatmap.Value.BeatmapInfo.Equals(targetDifficulty.Invoke()));
            AddUntilStep("current screen is editor", () => Stack.CurrentScreen == Editor && Editor?.IsLoaded == true);
        }
    }
}
