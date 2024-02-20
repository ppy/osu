// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.Timelines.Summary;
using osu.Game.Screens.Edit.GameplayTest;
using osu.Game.Screens.Play;
using osu.Game.Storyboards;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorTestGameplay : EditorTestScene
    {
        protected override bool IsolateSavingFromDatabase => false;

        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        [Resolved]
        private OsuGameBase game { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private BeatmapSetInfo importedBeatmapSet;

        private Bindable<float> editorDim;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            editorDim = config.GetBindable<float>(OsuSetting.EditorDim);
        }

        public override void SetUpSteps()
        {
            AddStep("import test beatmap", () => importedBeatmapSet = BeatmapImportHelper.LoadOszIntoOsu(game).GetResultSafely());
            base.SetUpSteps();
        }

        protected override WorkingBeatmap CreateWorkingBeatmap(IBeatmap beatmap, Storyboard storyboard = null)
            => beatmaps.GetWorkingBeatmap(importedBeatmapSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0));

        protected override void LoadEditor()
        {
            SelectedMods.Value = new[] { new ModCinema() };
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
            AddUntilStep("background has correct params", () =>
            {
                // the test gameplay player's beatmap may be the "same" beatmap as the one being edited, *but* the `BeatmapInfo` references may differ
                // due to the beatmap refetch logic ran on editor suspend.
                // this test cares about checking the background belonging to the editor specifically, so check that using reference equality
                // (as `.Equals()` cannot discern between the two, as they technically share the same database GUID).
                var background = this.ChildrenOfType<BackgroundScreenBeatmap>().Single(b => ReferenceEquals(b.Beatmap.BeatmapInfo, EditorBeatmap.BeatmapInfo));
                return background.DimWhenUserSettingsIgnored.Value == editorDim.Value && background.BlurAmount.Value == 0;
            });
            AddAssert("no mods selected", () => SelectedMods.Value.Count == 0);
        }

        [Test]
        public void TestGameplayTestWhenTrackRunning()
        {
            AddStep("start track", () => EditorClock.Start());
            AddAssert("sample playback enabled", () => !Editor.SamplePlaybackDisabled.Value);

            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            EditorPlayer editorPlayer = null;
            AddUntilStep("player pushed", () => (editorPlayer = Stack.CurrentScreen as EditorPlayer) != null);
            AddAssert("editor track stopped", () => !EditorClock.IsRunning);
            AddAssert("sample playback disabled", () => Editor.SamplePlaybackDisabled.Value);

            AddStep("exit player", () => editorPlayer.Exit());
            AddUntilStep("current screen is editor", () => Stack.CurrentScreen is Editor);
            AddUntilStep("background has correct params", () =>
            {
                // the test gameplay player's beatmap may be the "same" beatmap as the one being edited, *but* the `BeatmapInfo` references may differ
                // due to the beatmap refetch logic ran on editor suspend.
                // this test cares about checking the background belonging to the editor specifically, so check that using reference equality
                // (as `.Equals()` cannot discern between the two, as they technically share the same database GUID).
                var background = this.ChildrenOfType<BackgroundScreenBeatmap>().Single(b => ReferenceEquals(b.Beatmap.BeatmapInfo, EditorBeatmap.BeatmapInfo));
                return background.DimWhenUserSettingsIgnored.Value == editorDim.Value && background.BlurAmount.Value == 0;
            });

            AddStep("start track", () => EditorClock.Start());
            AddAssert("sample playback re-enabled", () => !Editor.SamplePlaybackDisabled.Value);
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
            AddUntilStep("save prompt shown", () => DialogOverlay.CurrentDialog is SaveRequiredPopupDialog);

            AddStep("dismiss prompt", () =>
            {
                var button = DialogOverlay.CurrentDialog!.Buttons.Last();
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
            AddUntilStep("save prompt shown", () => DialogOverlay.CurrentDialog is SaveRequiredPopupDialog);

            AddStep("save changes", () => DialogOverlay.CurrentDialog!.PerformOkAction());

            EditorPlayer editorPlayer = null;
            AddUntilStep("player pushed", () => (editorPlayer = Stack.CurrentScreen as EditorPlayer) != null);
            AddAssert("beatmap has 1 object", () => editorPlayer.Beatmap.Value.Beatmap.HitObjects.Count == 1);

            AddUntilStep("wait for return to editor", () => Stack.CurrentScreen is Editor);
            AddAssert("track stopped", () => !Beatmap.Value.Track.IsRunning);
        }

        [Test]
        public void TestClockTimeTransferIsOneDirectional()
        {
            AddStep("seek to 00:01:00", () => EditorClock.Seek(60_000));
            AddStep("click test gameplay button", () =>
            {
                var button = Editor.ChildrenOfType<TestGameplayButton>().Single();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            EditorPlayer editorPlayer = null;
            AddUntilStep("player pushed", () => (editorPlayer = Stack.CurrentScreen as EditorPlayer) != null);

            GameplayClockContainer gameplayClockContainer = null;
            AddStep("fetch gameplay clock", () => gameplayClockContainer = editorPlayer.ChildrenOfType<GameplayClockContainer>().First());
            AddUntilStep("gameplay clock running", () => gameplayClockContainer.IsRunning);
            // when the gameplay test is entered, the clock is expected to continue from where it was in the main editor...
            AddAssert("gameplay time past 00:01:00", () => gameplayClockContainer.CurrentTime >= 60_000);

            AddWaitStep("wait some", 5);

            AddStep("exit player", () => editorPlayer.Exit());
            AddUntilStep("current screen is editor", () => Stack.CurrentScreen is Editor);
            // but when exiting from gameplay test back to editor, the expectation is that the editor time should revert to what it was at the point of initiating the gameplay test.
            AddAssert("time reverted to 00:01:00", () => EditorClock.CurrentTime, () => Is.EqualTo(60_000));
        }

        public override void TearDownSteps()
        {
            base.TearDownSteps();
            AddStep("delete imported", () => Realm.Write(r =>
            {
                // delete from realm directly rather than via `BeatmapManager` to avoid cross-test pollution
                // (`BeatmapManager.Delete()` uses soft deletion, which can lead to beatmap reuse between test cases).
                r.RemoveAll<BeatmapMetadata>();
                r.RemoveAll<BeatmapInfo>();
                r.RemoveAll<BeatmapSetInfo>();
            }));
        }
    }
}
