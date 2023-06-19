﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.GameplayTest;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneBeatmapEditorNavigation : OsuGameTestScene
    {
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

        private EditorBeatmap getEditorBeatmap() => getEditor().ChildrenOfType<EditorBeatmap>().Single();

        private Editor getEditor() => (Editor)Game.ScreenStack.CurrentScreen;
    }
}
