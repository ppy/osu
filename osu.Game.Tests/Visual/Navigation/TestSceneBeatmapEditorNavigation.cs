// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Overlays;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.GameplayTest;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneBeatmapEditorNavigation : OsuGameTestScene
    {
        [Test]
        public void TestChangeMetadataExitWhileTextboxFocusedPromptsSave()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);
            AddStep("switch ruleset", () => Game.Ruleset.Value = new ManiaRuleset().RulesetInfo);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);

            AddStep("change to song setup", () => InputManager.Key(Key.F4));

            TextBox textbox = null!;

            AddUntilStep("wait for metadata section", () =>
            {
                var t = Game.ChildrenOfType<MetadataSection>().SingleOrDefault().ChildrenOfType<TextBox>().FirstOrDefault();

                if (t == null)
                    return false;

                textbox = t;
                return true;
            });

            AddStep("focus textbox", () =>
            {
                InputManager.MoveMouseTo(textbox);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("simulate changing textbox", () =>
            {
                // Can't simulate text input but this should work.
                InputManager.Keys(PlatformAction.SelectAll);
                InputManager.Keys(PlatformAction.Copy);
                InputManager.Keys(PlatformAction.Paste);
                InputManager.Keys(PlatformAction.Paste);
            });

            AddStep("exit", () => Game.ChildrenOfType<Editor>().Single().Exit());

            AddUntilStep("save dialog displayed", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault()?.CurrentDialog is PromptForSaveDialog);
        }

        [Test]
        public void TestEditorGameplayTestAlwaysUsesOriginalRuleset()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);
            AddStep("switch ruleset", () => Game.Ruleset.Value = new ManiaRuleset().RulesetInfo);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);
            AddStep("test gameplay", () => getEditor().TestGameplay());

            AddUntilStep("wait for player", () =>
            {
                // notifications may fire at almost any inopportune time and cause annoying test failures.
                // relentlessly attempt to dismiss any and all interfering overlays, which includes notifications.
                // this is theoretically not foolproof, but it's the best that can be done here.
                Game.CloseAllOverlays();
                return Game.ScreenStack.CurrentScreen is EditorPlayer editorPlayer && editorPlayer.IsLoaded;
            });

            AddAssert("current ruleset is osu!", () => Game.Ruleset.Value.Equals(new OsuRuleset().RulesetInfo));

            AddStep("exit to song select", () => Game.PerformFromScreen(_ => { }, typeof(PlaySongSelect).Yield()));
            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
            AddAssert("previous ruleset restored", () => Game.Ruleset.Value.Equals(new ManiaRuleset().RulesetInfo));
        }

        /// <summary>
        /// When entering the editor, a new beatmap is created as part of the asynchronous load process.
        /// This test ensures that in the case of an early exit from the editor (ie. while it's still loading)
        /// doesn't leave a dangling beatmap behind.
        ///
        /// This may not fail 100% due to timing, but has a pretty high chance of hitting a failure so works well enough
        /// as a test.
        /// </summary>
        [Test]
        public void TestCancelNavigationToEditor()
        {
            BeatmapSetInfo[] beatmapSets = null!;

            AddStep("Fetch initial beatmaps", () => beatmapSets = allBeatmapSets());

            AddStep("Set current beatmap to default", () => Game.Beatmap.SetDefault());

            AddStep("Push editor loader", () => Game.ScreenStack.Push(new EditorLoader()));
            AddUntilStep("Wait for loader current", () => Game.ScreenStack.CurrentScreen is EditorLoader);
            AddStep("Close editor while loading", () => Game.ScreenStack.CurrentScreen.Exit());

            AddUntilStep("Wait for menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("Check no new beatmaps were made", () => allBeatmapSets().SequenceEqual(beatmapSets));

            BeatmapSetInfo[] allBeatmapSets() => Game.Realm.Run(realm => realm.All<BeatmapSetInfo>().Where(x => !x.DeletePending).ToArray());
        }

        [Test]
        public void TestExitEditorWithoutSelection()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);

            AddStep("escape once", () => InputManager.Key(Key.Escape));

            AddUntilStep("wait for editor exit", () => Game.ScreenStack.CurrentScreen is not Editor);
        }

        [Test]
        public void TestExitEditorWithSelection()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);

            AddStep("make selection", () =>
            {
                var beatmap = getEditorBeatmap();
                beatmap.SelectedHitObjects.AddRange(beatmap.HitObjects.Take(5));
            });

            AddAssert("selection exists", () => getEditorBeatmap().SelectedHitObjects, () => Has.Count.GreaterThan(0));

            AddStep("escape once", () => InputManager.Key(Key.Escape));

            AddAssert("selection empty", () => getEditorBeatmap().SelectedHitObjects, () => Has.Count.Zero);

            AddStep("escape again", () => InputManager.Key(Key.Escape));

            AddUntilStep("wait for editor exit", () => Game.ScreenStack.CurrentScreen is not Editor);
        }

        [Test]
        public void TestLastTimestampRememberedOnExit()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);

            AddStep("seek to arbitrary time", () => getEditor().ChildrenOfType<EditorClock>().First().Seek(1234));
            AddUntilStep("time is correct", () => getEditor().ChildrenOfType<EditorClock>().First().CurrentTime, () => Is.EqualTo(1234));

            AddStep("exit editor", () => InputManager.Key(Key.Escape));
            AddUntilStep("wait for editor exit", () => Game.ScreenStack.CurrentScreen is not Editor);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit());

            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);
            AddUntilStep("time is correct", () => getEditor().ChildrenOfType<EditorClock>().First().CurrentTime, () => Is.EqualTo(1234));
        }

        [Test]
        public void TestAttemptGlobalMusicOperationFromEditor()
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);

            AddUntilStep("wait for music playing", () => Game.MusicController.IsPlaying);
            AddStep("user request stop", () => Game.MusicController.Stop(requestedByUser: true));
            AddUntilStep("wait for music stopped", () => !Game.MusicController.IsPlaying);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);

            AddUntilStep("music still stopped", () => !Game.MusicController.IsPlaying);
            AddStep("user request play", () => Game.MusicController.Play(requestedByUser: true));
            AddUntilStep("music still stopped", () => !Game.MusicController.IsPlaying);

            AddStep("exit to song select", () => Game.PerformFromScreen(_ => { }, typeof(PlaySongSelect).Yield()));
            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);

            AddUntilStep("wait for music playing", () => Game.MusicController.IsPlaying);
            AddStep("user request stop", () => Game.MusicController.Stop(requestedByUser: true));
            AddUntilStep("wait for music stopped", () => !Game.MusicController.IsPlaying);
        }

        [TestCase(SortMode.Title)]
        [TestCase(SortMode.Difficulty)]
        public void TestSelectionRetainedOnExit(SortMode sortMode)
        {
            BeatmapSetInfo beatmapSet = null!;

            AddStep("import test beatmap", () => Game.BeatmapManager.Import(TestResources.GetTestBeatmapForImport()).WaitSafely());
            AddStep("retrieve beatmap", () => beatmapSet = Game.BeatmapManager.QueryBeatmapSet(set => !set.Protected).AsNonNull().Value.Detach());

            AddStep($"set sort mode to {sortMode}", () => Game.LocalConfig.SetValue(OsuSetting.SongSelectSortingMode, sortMode));
            AddStep("present beatmap", () => Game.PresentBeatmap(beatmapSet));
            AddUntilStep("wait for song select",
                () => Game.Beatmap.Value.BeatmapSetInfo.Equals(beatmapSet)
                      && Game.ScreenStack.CurrentScreen is PlaySongSelect songSelect
                      && songSelect.IsLoaded);

            AddStep("open editor", () => ((PlaySongSelect)Game.ScreenStack.CurrentScreen).Edit(beatmapSet.Beatmaps.First(beatmap => beatmap.Ruleset.OnlineID == 0)));
            AddUntilStep("wait for editor open", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.ReadyForUse);

            AddStep("exit editor", () => InputManager.Key(Key.Escape));
            AddUntilStep("wait for editor exit", () => Game.ScreenStack.CurrentScreen is not Editor);

            AddUntilStep("selection retained on song select",
                () => Game.Beatmap.Value.BeatmapInfo.ID,
                () => Is.EqualTo(beatmapSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0).ID));
        }

        [Test]
        public void TestCreateNewDifficultyOnNonExistentBeatmap()
        {
            AddUntilStep("wait for dialog overlay", () => Game.ChildrenOfType<DialogOverlay>().SingleOrDefault() != null);

            AddStep("open editor", () => Game.ChildrenOfType<ButtonSystem>().Single().OnEditBeatmap?.Invoke());
            AddUntilStep("wait for editor", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.IsLoaded);
            AddStep("click on file", () =>
            {
                var item = getEditor().ChildrenOfType<Menu.DrawableMenuItem>().Single(i => i.Item.Text.Value.ToString() == "File");
                item.TriggerClick();
            });
            AddStep("click on create new difficulty", () =>
            {
                var item = getEditor().ChildrenOfType<Menu.DrawableMenuItem>().Single(i => i.Item.Text.Value.ToString() == "Create new difficulty");
                item.TriggerClick();
            });
            AddStep("click on catch", () =>
            {
                var item = getEditor().ChildrenOfType<Menu.DrawableMenuItem>().Single(i => i.Item.Text.Value.ToString() == "osu!catch");
                item.TriggerClick();
            });
            AddAssert("save dialog displayed", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog is SaveRequiredPopupDialog);

            AddStep("press save", () => Game.ChildrenOfType<DialogOverlay>().Single().CurrentDialog!.PerformOkAction());
            AddUntilStep("wait for editor", () => Game.ScreenStack.CurrentScreen is Editor editor && editor.IsLoaded);
            AddAssert("editor beatmap uses catch ruleset", () => getEditorBeatmap().BeatmapInfo.Ruleset.ShortName == "fruits");
        }

        private EditorBeatmap getEditorBeatmap() => getEditor().ChildrenOfType<EditorBeatmap>().Single();

        private Editor getEditor() => (Editor)Game.ScreenStack.CurrentScreen;
    }
}
