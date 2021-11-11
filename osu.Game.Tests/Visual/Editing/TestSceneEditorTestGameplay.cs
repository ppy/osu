// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorTestGameplay : EditorTestScene
    {
        protected override bool IsolateSavingFromDatabase => false;

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private BeatmapSetInfo importedBeatmapSet;

        public override void SetUpSteps()
        {
            AddStep("import test beatmap", () => importedBeatmapSet = ImportBeatmapTest.LoadOszIntoOsu(game).Result);
            base.SetUpSteps();
        }

        protected override void LoadEditor()
        {
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First(b => b.RulesetID == 0));
            base.LoadEditor();
        }

        [Test]
        public void TestBasicGameplayTest()
        {
            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            EditorPlayer editorPlayer = null;
            AddUntilStep("player pushed", () => (editorPlayer = Stack.CurrentScreen as EditorPlayer) != null);
            AddStep("exit player", () => editorPlayer.Exit());
            AddUntilStep("current screen is editor", () => Stack.CurrentScreen is Editor);
        }

        [Test]
        public void TestCancelGameplayTestWithUnsavedChanges()
        {
            AddStep("delete all but first object", () => EditorBeatmap.RemoveRange(EditorBeatmap.HitObjects.Skip(1).ToList()));

            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("save prompt shown", () => DialogOverlay.CurrentDialog is SaveBeforeGameplayTestDialog);

            AddStep("dismiss prompt", () =>
            {
                var button = DialogOverlay.CurrentDialog.Buttons.Last();
                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            AddWaitStep("wait some", 3);
            AddAssert("stayed in editor", () => Stack.CurrentScreen is Editor);
        }

        [Test]
        public void TestSaveChangesBeforeGameplayTest()
        {
            AddStep("delete all but first object", () => EditorBeatmap.RemoveRange(EditorBeatmap.HitObjects.Skip(1).ToList()));
            // bit of a hack to ensure this test can be ran multiple times without running into UNIQUE constraint failures
            AddStep("set unique difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = Guid.NewGuid().ToString());

            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("save prompt shown", () => DialogOverlay.CurrentDialog is SaveBeforeGameplayTestDialog);

            AddStep("save changes", () => DialogOverlay.CurrentDialog.PerformOkAction());

            EditorPlayer editorPlayer = null;
            AddUntilStep("player pushed", () => (editorPlayer = Stack.CurrentScreen as EditorPlayer) != null);
            AddAssert("beatmap has 1 object", () => editorPlayer.Beatmap.Value.Beatmap.HitObjects.Count == 1);

            AddUntilStep("wait for return to editor", () => Stack.CurrentScreen is Editor);
            AddAssert("track stopped", () => !Beatmap.Value.Track.IsRunning);
        }

        public override void TearDownSteps()
        {
            base.TearDownSteps();
            AddStep("delete imported", () =>
            {
                beatmaps.Delete(importedBeatmapSet);
            });
        }
    }
}
