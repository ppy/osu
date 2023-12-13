// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Notifications;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public partial class TestScenePlaySongSelect : ScreenTestScene
    {
        private BeatmapManager manager = null!;
        private RulesetStore rulesets = null!;
        private MusicController music = null!;
        private WorkingBeatmap defaultBeatmap = null!;
        private OsuConfigManager config = null!;
        private TestSongSelect? songSelect;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            // These DI caches are required to ensure for interactive runs this test scene doesn't nuke all user beatmaps in the local install.
            // At a point we have isolated interactive test runs enough, this can likely be removed.
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, defaultBeatmap = Beatmap.Default));

            Dependencies.Cache(music = new MusicController());

            // required to get bindables attached
            Add(music);

            Dependencies.Cache(config = new OsuConfigManager(LocalStorage));
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset defaults", () =>
            {
                Ruleset.Value = new OsuRuleset().RulesetInfo;

                Beatmap.SetDefault();
                SelectedMods.SetDefault();

                songSelect = null;
            });

            AddStep("delete all beatmaps", () => manager.Delete());
        }

        [Test]
        public void TestPlaceholderBeatmapPresence()
        {
            createSongSelect();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            addRulesetImportStep(0);
            AddUntilStep("wait for placeholder hidden", () => getPlaceholder()?.State.Value == Visibility.Hidden);

            AddStep("delete all beatmaps", () => manager.Delete());
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestPlaceholderStarDifficulty()
        {
            addRulesetImportStep(0);
            AddStep("change star filter", () => config.SetValue(OsuSetting.DisplayStarsMinimum, 10.0));

            createSongSelect();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddStep("click link in placeholder", () => getPlaceholder().ChildrenOfType<DrawableLinkCompiler>().First().TriggerClick());

            AddUntilStep("star filter reset", () => config.Get<double>(OsuSetting.DisplayStarsMinimum) == 0.0);
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestPlaceholderConvertSetting()
        {
            addRulesetImportStep(0);
            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            createSongSelect();

            changeRuleset(2);

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddStep("click link in placeholder", () => getPlaceholder().ChildrenOfType<DrawableLinkCompiler>().First().TriggerClick());

            AddUntilStep("convert setting changed", () => config.Get<bool>(OsuSetting.ShowConvertedBeatmaps));
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestSingleFilterOnEnter()
        {
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            createSongSelect();

            AddAssert("filter count is 1", () => songSelect?.FilterCount == 1);
        }

        [Test]
        public void TestChangeBeatmapBeforeEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            waitForInitialSelection();

            WorkingBeatmap? selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.Key(Key.Down);
                InputManager.Key(Key.Enter);
            });

            waitForDismissed();
            AddAssert("ensure selection changed", () => selected != Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapAfterEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            waitForInitialSelection();

            WorkingBeatmap? selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.Key(Key.Enter);
                InputManager.Key(Key.Down);
            });

            waitForDismissed();
            AddAssert("ensure selection didn't change", () => selected == Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapViaMouseBeforeEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);

            WorkingBeatmap? selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddUntilStep("wait for beatmaps to load", () => songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmap>().Any());

            AddStep("select next and enter", () =>
            {
                InputManager.MoveMouseTo(songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmap>()
                                                    .First(b => !((CarouselBeatmap)b.Item!).BeatmapInfo.Equals(songSelect!.Carousel.SelectedBeatmapInfo)));

                InputManager.Click(MouseButton.Left);

                InputManager.Key(Key.Enter);
            });

            waitForDismissed();
            AddAssert("ensure selection changed", () => selected != Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapViaMouseAfterEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            waitForInitialSelection();

            WorkingBeatmap? selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.MoveMouseTo(songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmap>()
                                                    .First(b => !((CarouselBeatmap)b.Item!).BeatmapInfo.Equals(songSelect!.Carousel.SelectedBeatmapInfo)));

                InputManager.PressButton(MouseButton.Left);

                InputManager.Key(Key.Enter);

                InputManager.ReleaseButton(MouseButton.Left);
            });

            waitForDismissed();
            AddAssert("ensure selection didn't change", () => selected == Beatmap.Value);
        }

        [Test]
        public void TestNoFilterOnSimpleResume()
        {
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            createSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            waitForDismissed();

            AddStep("return", () => songSelect!.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect!.IsCurrentScreen());
            AddAssert("filter count is 1", () => songSelect!.FilterCount == 1);
        }

        [Test]
        public void TestFilterOnResumeAfterChange()
        {
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            createSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            waitForDismissed();

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            AddStep("return", () => songSelect!.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect!.IsCurrentScreen());
            AddAssert("filter count is 2", () => songSelect!.FilterCount == 2);
        }

        [Test]
        public void TestCarouselSelectionUpdatesOnResume()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            waitForDismissed();

            AddStep("update beatmap", () =>
            {
                var selectedBeatmap = Beatmap.Value.BeatmapInfo;
                var anotherBeatmap = Beatmap.Value.BeatmapSetInfo.Beatmaps.Except(selectedBeatmap.Yield()).First();
                Beatmap.Value = manager.GetWorkingBeatmap(anotherBeatmap);
            });

            AddStep("return", () => songSelect!.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect!.IsCurrentScreen());
            AddAssert("carousel updated", () => songSelect!.Carousel.SelectedBeatmapInfo?.Equals(Beatmap.Value.BeatmapInfo) == true);
        }

        [Test]
        public void TestAudioResuming()
        {
            createSongSelect();

            // We need to use one real beatmap to trigger the "same-track-transfer" logic that we're looking to test here.
            // See `SongSelect.ensurePlayingSelected` and `WorkingBeatmap.TryTransferTrack`.
            AddStep("import test beatmap", () => manager.Import(new ImportTask(TestResources.GetTestBeatmapForImport())).WaitSafely());
            addRulesetImportStep(0);

            checkMusicPlaying(true);
            AddStep("select first", () => songSelect!.Carousel.SelectBeatmap(songSelect!.Carousel.BeatmapSets.First().Beatmaps.First()));
            checkMusicPlaying(true);

            AddStep("manual pause", () => music.TogglePause());
            checkMusicPlaying(false);

            // Track should not have changed, so music should still not be playing.
            AddStep("select next difficulty", () => songSelect!.Carousel.SelectNext(skipDifficulties: false));
            checkMusicPlaying(false);

            AddStep("select next set", () => songSelect!.Carousel.SelectNext());
            checkMusicPlaying(true);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestAudioRemainsCorrectOnRulesetChange(bool rulesetsInSameBeatmap)
        {
            createSongSelect();

            // start with non-osu! to avoid convert confusion
            changeRuleset(1);

            if (rulesetsInSameBeatmap)
            {
                AddStep("import multi-ruleset map", () =>
                {
                    var usableRulesets = rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();
                    manager.Import(TestResources.CreateTestBeatmapSetInfo(rulesets: usableRulesets));
                });
            }
            else
            {
                addRulesetImportStep(1);
                addRulesetImportStep(0);
            }

            checkMusicPlaying(true);

            AddStep("manual pause", () => music.TogglePause());
            checkMusicPlaying(false);

            changeRuleset(0);
            checkMusicPlaying(!rulesetsInSameBeatmap);
        }

        [Test]
        public void TestDummy()
        {
            createSongSelect();
            AddUntilStep("dummy selected", () => songSelect!.CurrentBeatmap == defaultBeatmap);

            AddUntilStep("dummy shown on wedge", () => songSelect!.CurrentBeatmapDetailsBeatmap == defaultBeatmap);

            addManyTestMaps();

            AddUntilStep("random map selected", () => songSelect!.CurrentBeatmap != defaultBeatmap);
        }

        [Test]
        public void TestSorting()
        {
            createSongSelect();
            addManyTestMaps();

            AddUntilStep("random map selected", () => songSelect!.CurrentBeatmap != defaultBeatmap);

            AddStep(@"Sort by Artist", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Artist));
            AddStep(@"Sort by Title", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Title));
            AddStep(@"Sort by Author", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Author));
            AddStep(@"Sort by DateAdded", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.DateAdded));
            AddStep(@"Sort by BPM", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.BPM));
            AddStep(@"Sort by Length", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Length));
            AddStep(@"Sort by Difficulty", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Difficulty));
            AddStep(@"Sort by Source", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Source));
        }

        [Test]
        public void TestImportUnderDifferentRuleset()
        {
            createSongSelect();
            addRulesetImportStep(2);
            AddUntilStep("no selection", () => songSelect!.Carousel.SelectedBeatmapInfo == null);
        }

        [Test]
        public void TestImportUnderCurrentRuleset()
        {
            createSongSelect();
            changeRuleset(2);
            addRulesetImportStep(2);
            addRulesetImportStep(1);
            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.Ruleset.OnlineID == 2);

            changeRuleset(1);
            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.Ruleset.OnlineID == 1);

            changeRuleset(0);
            AddUntilStep("no selection", () => songSelect!.Carousel.SelectedBeatmapInfo == null);
        }

        [Test]
        public void TestSelectionRetainedOnBeatmapUpdate()
        {
            createSongSelect();
            changeRuleset(0);

            Live<BeatmapSetInfo>? original = null;
            int originalOnlineSetID = 0;

            AddStep(@"Sort by artist", () => config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Artist));

            AddStep("import original", () =>
            {
                original = manager.Import(new ImportTask(TestResources.GetQuickTestBeatmapForImport())).GetResultSafely();

                Debug.Assert(original != null);

                originalOnlineSetID = original.Value.OnlineID;
            });

            // This will move the beatmap set to a different location in the carousel.
            AddStep("Update original with bogus info", () =>
            {
                Debug.Assert(original != null);

                original.PerformWrite(set =>
                {
                    foreach (var beatmap in set.Beatmaps)
                    {
                        beatmap.Metadata.Artist = "ZZZZZ";
                        beatmap.OnlineID = 12804;
                    }
                });
            });

            AddRepeatStep("import other beatmaps", () =>
            {
                var testBeatmapSetInfo = TestResources.CreateTestBeatmapSetInfo();

                foreach (var beatmap in testBeatmapSetInfo.Beatmaps)
                    beatmap.Metadata.Artist = ((char)RNG.Next('A', 'Z')).ToString();

                manager.Import(testBeatmapSetInfo);
            }, 10);

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.BeatmapSet?.OnlineID == originalOnlineSetID);

            Task<Live<BeatmapSetInfo>?> updateTask = null!;

            AddStep("update beatmap", () =>
            {
                Debug.Assert(original != null);

                updateTask = manager.ImportAsUpdate(new ProgressNotification(), new ImportTask(TestResources.GetQuickTestBeatmapForImport()), original.Value);
            });
            AddUntilStep("wait for update completion", () => updateTask.IsCompleted);

            AddUntilStep("retained selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.BeatmapSet?.OnlineID == originalOnlineSetID);
        }

        [Test]
        public void TestPresentNewRulesetNewBeatmap()
        {
            createSongSelect();
            changeRuleset(2);

            addRulesetImportStep(2);
            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.Ruleset.OnlineID == 2);

            addRulesetImportStep(0);
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            BeatmapInfo? target = null;

            AddStep("select beatmap/ruleset externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets()
                                .Last(b => b.Beatmaps.Any(bi => bi.Ruleset.OnlineID == 0)).Beatmaps.Last();

                Ruleset.Value = rulesets.AvailableRulesets.First(r => r.OnlineID == 0);
                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.Equals(target) == true);

            // this is an important check, to make sure updateComponentFromBeatmap() was actually run
            AddUntilStep("selection shown on wedge", () => songSelect!.CurrentBeatmapDetailsBeatmap.BeatmapInfo.MatchesOnlineID(target));
        }

        [Test]
        public void TestPresentNewBeatmapNewRuleset()
        {
            createSongSelect();
            changeRuleset(2);

            addRulesetImportStep(2);
            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.Ruleset.OnlineID == 2);

            addRulesetImportStep(0);
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            BeatmapInfo? target = null;

            AddStep("select beatmap/ruleset externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets()
                                .Last(b => b.Beatmaps.Any(bi => bi.Ruleset.OnlineID == 0)).Beatmaps.Last();

                Beatmap.Value = manager.GetWorkingBeatmap(target);
                Ruleset.Value = rulesets.AvailableRulesets.First(r => r.OnlineID == 0);
            });

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo?.Equals(target) == true);

            AddUntilStep("has correct ruleset", () => Ruleset.Value.OnlineID == 0);

            // this is an important check, to make sure updateComponentFromBeatmap() was actually run
            AddUntilStep("selection shown on wedge", () => songSelect!.CurrentBeatmapDetailsBeatmap.BeatmapInfo.MatchesOnlineID(target));
        }

        [Test]
        public void TestModsRetainedBetweenSongSelect()
        {
            AddAssert("empty mods", () => !SelectedMods.Value.Any());

            createSongSelect();

            addRulesetImportStep(0);

            changeMods(new OsuModHardRock());

            createSongSelect();

            AddAssert("mods retained", () => SelectedMods.Value.Any());
        }

        [Test]
        public void TestStartAfterUnMatchingFilterDoesNotStart()
        {
            createSongSelect();
            addManyTestMaps();
            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo != null);

            bool startRequested = false;

            AddStep("set filter and finalize", () =>
            {
                songSelect!.StartRequested = () => startRequested = true;

                songSelect!.Carousel.Filter(new FilterCriteria { SearchText = "somestringthatshouldn'tbematchable" });
                songSelect!.FinaliseSelection();

                songSelect!.StartRequested = null;
            });

            AddAssert("start not requested", () => !startRequested);
        }

        [Test]
        public void TestSearchTextWithRulesetCriteria()
        {
            createSongSelect();

            addRulesetImportStep(0);

            AddStep("disallow convert display", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo != null);

            AddStep("set filter to match all", () => songSelect!.FilterControl.CurrentTextSearch.Value = "Some");

            changeRuleset(1);

            AddUntilStep("has no selection", () => songSelect!.Carousel.SelectedBeatmapInfo == null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestExternalBeatmapChangeWhileFiltered(bool differentRuleset)
        {
            createSongSelect();
            // ensure there is at least 1 difficulty for each of the rulesets
            // (catch is excluded inside of addManyTestMaps).
            addManyTestMaps(3);

            changeRuleset(0);

            // used for filter check below
            AddStep("allow convert display", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo != null);

            AddStep("set filter text", () => songSelect!.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");

            AddUntilStep("dummy selected", () => Beatmap.Value is DummyWorkingBeatmap);

            AddUntilStep("has no selection", () => songSelect!.Carousel.SelectedBeatmapInfo == null);

            BeatmapInfo? target = null;

            int targetRuleset = differentRuleset ? 1 : 0;

            AddStep("select beatmap externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets()
                                .First(b => b.Beatmaps.Any(bi => bi.Ruleset.OnlineID == targetRuleset))
                                .Beatmaps
                                .First(bi => bi.Ruleset.OnlineID == targetRuleset);

                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo != null);

            AddAssert("selected only shows expected ruleset (plus converts)", () =>
            {
                var selectedPanel = songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().First(s => s.Item!.State.Value == CarouselItemState.Selected);

                // special case for converts checked here.
                return selectedPanel.ChildrenOfType<FilterableDifficultyIcon>().All(i =>
                    i.IsFiltered || i.Item.BeatmapInfo.Ruleset.OnlineID == targetRuleset || i.Item.BeatmapInfo.Ruleset.OnlineID == 0);
            });

            AddUntilStep("carousel has correct", () => songSelect!.Carousel.SelectedBeatmapInfo?.MatchesOnlineID(target) == true);
            AddUntilStep("game has correct", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(target));

            AddStep("reset filter text", () => songSelect!.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = string.Empty);

            AddAssert("game still correct", () => Beatmap.Value?.BeatmapInfo.MatchesOnlineID(target) == true);
            AddAssert("carousel still correct", () => songSelect!.Carousel.SelectedBeatmapInfo.MatchesOnlineID(target));
        }

        [Test]
        public void TestExternalBeatmapChangeWhileFilteredThenRefilter()
        {
            createSongSelect();
            // ensure there is at least 1 difficulty for each of the rulesets
            // (catch is excluded inside of addManyTestMaps).
            addManyTestMaps(3);

            changeRuleset(0);

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo != null);

            AddStep("set filter text", () => songSelect!.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");

            AddUntilStep("dummy selected", () => Beatmap.Value is DummyWorkingBeatmap);

            AddUntilStep("has no selection", () => songSelect!.Carousel.SelectedBeatmapInfo == null);

            BeatmapInfo? target = null;

            AddStep("select beatmap externally", () =>
            {
                target = manager
                         .GetAllUsableBeatmapSets()
                         .First(b => b.Beatmaps.Any(bi => bi.Ruleset.OnlineID == 1))
                         .Beatmaps.First();

                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect!.Carousel.SelectedBeatmapInfo != null);

            AddUntilStep("carousel has correct", () => songSelect!.Carousel.SelectedBeatmapInfo?.MatchesOnlineID(target) == true);
            AddUntilStep("game has correct", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(target));

            AddStep("set filter text", () => songSelect!.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nononoo");

            AddUntilStep("game lost selection", () => Beatmap.Value is DummyWorkingBeatmap);
            AddAssert("carousel lost selection", () => songSelect!.Carousel.SelectedBeatmapInfo == null);
        }

        [Test]
        public void TestAutoplayShortcut()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("autoplay selected", () => songSelect!.Mods.Value.Single() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => songSelect!.IsCurrentScreen());

            AddAssert("no mods selected", () => songSelect!.Mods.Value.Count == 0);
        }

        [Test]
        public void TestAutoplayShortcutKeepsAutoplayIfSelectedAlready()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            changeMods(new OsuModAutoplay());

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("autoplay selected", () => songSelect!.Mods.Value.Single() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => songSelect!.IsCurrentScreen());

            AddAssert("autoplay still selected", () => songSelect!.Mods.Value.Single() is ModAutoplay);
        }

        [Test]
        public void TestAutoplayShortcutReturnsInitialModsOnExit()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            changeMods(new OsuModRelax());

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("only autoplay selected", () => songSelect!.Mods.Value.Single() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => songSelect!.IsCurrentScreen());

            AddAssert("relax returned", () => songSelect!.Mods.Value.Single() is ModRelax);
        }

        [Test]
        public void TestHideSetSelectsCorrectBeatmap()
        {
            Guid? previousID = null;
            createSongSelect();
            addRulesetImportStep(0);
            AddStep("Move to last difficulty", () => songSelect!.Carousel.SelectBeatmap(songSelect!.Carousel.BeatmapSets.First().Beatmaps.Last()));
            AddStep("Store current ID", () => previousID = songSelect!.Carousel.SelectedBeatmapInfo!.ID);
            AddStep("Hide first beatmap", () => manager.Hide(songSelect!.Carousel.SelectedBeatmapSet!.Beatmaps.First()));
            AddAssert("Selected beatmap has not changed", () => songSelect!.Carousel.SelectedBeatmapInfo?.ID == previousID);
        }

        [Test]
        public void TestDifficultyIconSelecting()
        {
            addRulesetImportStep(0);
            createSongSelect();

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            DrawableCarouselBeatmapSet set = null!;
            AddStep("Find the DrawableCarouselBeatmapSet", () =>
            {
                set = songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().First();
            });

            FilterableDifficultyIcon difficultyIcon = null!;

            AddUntilStep("Find an icon", () =>
            {
                var foundIcon = set.ChildrenOfType<FilterableDifficultyIcon>()
                                   .FirstOrDefault(icon => getDifficultyIconIndex(set, icon) != getCurrentBeatmapIndex());

                if (foundIcon == null)
                    return false;

                difficultyIcon = foundIcon;
                return true;
            });

            AddStep("Click on a difficulty", () =>
            {
                InputManager.MoveMouseTo(difficultyIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Selected beatmap correct", () => getCurrentBeatmapIndex() == getDifficultyIconIndex(set, difficultyIcon));

            double? maxBPM = null;
            AddStep("Filter some difficulties", () => songSelect!.Carousel.Filter(new FilterCriteria
            {
                BPM = new FilterCriteria.OptionalRange<double>
                {
                    Min = maxBPM = songSelect!.Carousel.SelectedBeatmapSet!.MaxBPM,
                    IsLowerInclusive = true
                }
            }));

            BeatmapInfo? filteredBeatmap = null;
            FilterableDifficultyIcon? filteredIcon = null;

            AddStep("Get filtered icon", () =>
            {
                var selectedSet = songSelect!.Carousel.SelectedBeatmapSet;

                Debug.Assert(selectedSet != null);

                filteredBeatmap = selectedSet.Beatmaps.First(b => b.BPM < maxBPM);
                int filteredBeatmapIndex = getBeatmapIndex(selectedSet, filteredBeatmap);
                filteredIcon = set.ChildrenOfType<FilterableDifficultyIcon>().ElementAt(filteredBeatmapIndex);
            });

            AddStep("Click on a filtered difficulty", () =>
            {
                Debug.Assert(filteredIcon != null);

                InputManager.MoveMouseTo(filteredIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Selected beatmap correct", () => songSelect!.Carousel.SelectedBeatmapInfo?.Equals(filteredBeatmap) == true);
        }

        [Test]
        public void TestChangingRulesetOnMultiRulesetBeatmap()
        {
            int changeCount = 0;

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));
            AddStep("bind beatmap changed", () =>
            {
                Beatmap.ValueChanged += onChange;
                changeCount = 0;
            });

            changeRuleset(0);

            createSongSelect();

            AddStep("import multi-ruleset map", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();
                manager.Import(TestResources.CreateTestBeatmapSetInfo(3, usableRulesets));
            });

            int previousSetID = 0;

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            AddStep("record set ID", () => previousSetID = ((IBeatmapSetInfo)Beatmap.Value.BeatmapSetInfo).OnlineID);
            AddAssert("selection changed once", () => changeCount == 1);

            AddAssert("Check ruleset is osu!", () => Ruleset.Value.OnlineID == 0);

            changeRuleset(3);

            AddUntilStep("Check ruleset changed to mania", () => Ruleset.Value.OnlineID == 3);

            AddUntilStep("selection changed", () => changeCount > 1);

            AddAssert("Selected beatmap still same set", () => Beatmap.Value.BeatmapSetInfo.OnlineID == previousSetID);
            AddAssert("Selected beatmap is mania", () => Beatmap.Value.BeatmapInfo.Ruleset.OnlineID == 3);

            AddAssert("selection changed only fired twice", () => changeCount == 2);

            AddStep("unbind beatmap changed", () => Beatmap.ValueChanged -= onChange);
            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            // ReSharper disable once AccessToModifiedClosure
            void onChange(ValueChangedEvent<WorkingBeatmap> valueChangedEvent) => changeCount++;
        }

        [Test]
        public void TestDifficultyIconSelectingForDifferentRuleset()
        {
            changeRuleset(0);

            createSongSelect();

            AddStep("import multi-ruleset map", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();
                manager.Import(TestResources.CreateTestBeatmapSetInfo(3, usableRulesets));
            });

            DrawableCarouselBeatmapSet? set = null;
            AddUntilStep("Find the DrawableCarouselBeatmapSet", () =>
            {
                set = songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().FirstOrDefault();
                return set != null;
            });

            FilterableDifficultyIcon? difficultyIcon = null;
            AddUntilStep("Find an icon for different ruleset", () =>
            {
                difficultyIcon = set.ChildrenOfType<FilterableDifficultyIcon>()
                                    .FirstOrDefault(icon => icon.Item.BeatmapInfo.Ruleset.OnlineID == 3);
                return difficultyIcon != null;
            });

            AddAssert("Check ruleset is osu!", () => Ruleset.Value.OnlineID == 0);

            int previousSetID = 0;

            AddStep("record set ID", () => previousSetID = ((IBeatmapSetInfo)Beatmap.Value.BeatmapSetInfo).OnlineID);

            AddStep("Click on a difficulty", () =>
            {
                Debug.Assert(difficultyIcon != null);

                InputManager.MoveMouseTo(difficultyIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Check ruleset changed to mania", () => Ruleset.Value.OnlineID == 3);

            AddAssert("Selected beatmap still same set", () => songSelect!.Carousel.SelectedBeatmapInfo?.BeatmapSet?.OnlineID == previousSetID);
            AddAssert("Selected beatmap is mania", () => Beatmap.Value.BeatmapInfo.Ruleset.OnlineID == 3);
        }

        [Test]
        public void TestGroupedDifficultyIconSelecting()
        {
            changeRuleset(0);

            createSongSelect();

            BeatmapSetInfo? imported = null;

            AddStep("import huge difficulty count map", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();
                imported = manager.Import(TestResources.CreateTestBeatmapSetInfo(50, usableRulesets))?.Value;
            });

            AddStep("select the first beatmap of import", () => Beatmap.Value = manager.GetWorkingBeatmap(imported?.Beatmaps.First()));

            DrawableCarouselBeatmapSet? set = null;
            AddUntilStep("Find the DrawableCarouselBeatmapSet", () =>
            {
                set = songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().FirstOrDefault();
                return set != null;
            });

            GroupedDifficultyIcon groupIcon = null!;

            AddUntilStep("Find group icon for different ruleset", () =>
            {
                var foundIcon = set.ChildrenOfType<GroupedDifficultyIcon>()
                                   .FirstOrDefault(icon => icon.Items.First().BeatmapInfo.Ruleset.OnlineID == 3);

                if (foundIcon == null)
                    return false;

                groupIcon = foundIcon;
                return true;
            });

            AddAssert("Check ruleset is osu!", () => Ruleset.Value.OnlineID == 0);

            AddStep("Click on group", () =>
            {
                InputManager.MoveMouseTo(groupIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Check ruleset changed to mania", () => Ruleset.Value.OnlineID == 3);

            AddAssert("Check first item in group selected", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(groupIcon.Items.First().BeatmapInfo));
        }

        [Test]
        public void TestChangeRulesetWhilePresentingScore()
        {
            BeatmapInfo getPresentBeatmap() => manager.GetAllUsableBeatmapSets().Where(s => !s.DeletePending).SelectMany(s => s.Beatmaps).First(b => b.Ruleset.OnlineID == 0);
            BeatmapInfo getSwitchBeatmap() => manager.GetAllUsableBeatmapSets().Where(s => !s.DeletePending).SelectMany(s => s.Beatmaps).First(b => b.Ruleset.OnlineID == 1);

            changeRuleset(0);

            createSongSelect();

            addRulesetImportStep(0);
            addRulesetImportStep(1);

            AddStep("present score", () =>
            {
                // this ruleset change should be overridden by the present.
                Ruleset.Value = getSwitchBeatmap().Ruleset;

                songSelect!.PresentScore(new ScoreInfo
                {
                    User = new APIUser { Username = "woo" },
                    BeatmapInfo = getPresentBeatmap(),
                    Ruleset = getPresentBeatmap().Ruleset
                });
            });

            waitForDismissed();

            AddAssert("check beatmap is correct for score", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(getPresentBeatmap()));
            AddAssert("check ruleset is correct for score", () => Ruleset.Value.OnlineID == 0);
        }

        [Test]
        public void TestChangeBeatmapWhilePresentingScore()
        {
            BeatmapInfo getPresentBeatmap() => manager.GetAllUsableBeatmapSets().Where(s => !s.DeletePending).SelectMany(s => s.Beatmaps).First(b => b.Ruleset.OnlineID == 0);
            BeatmapInfo getSwitchBeatmap() => manager.GetAllUsableBeatmapSets().Where(s => !s.DeletePending).SelectMany(s => s.Beatmaps).First(b => b.Ruleset.OnlineID == 1);

            changeRuleset(0);

            addRulesetImportStep(0);
            addRulesetImportStep(1);

            createSongSelect();

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            AddStep("present score", () =>
            {
                // this beatmap change should be overridden by the present.
                Beatmap.Value = manager.GetWorkingBeatmap(getSwitchBeatmap());

                songSelect!.PresentScore(TestResources.CreateTestScoreInfo(getPresentBeatmap()));
            });

            waitForDismissed();

            AddAssert("check beatmap is correct for score", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(getPresentBeatmap()));
            AddAssert("check ruleset is correct for score", () => Ruleset.Value.OnlineID == 0);
        }

        [Test]
        public void TestModOverlayToggling()
        {
            changeRuleset(0);
            createSongSelect();

            AddStep("toggle mod overlay on", () => InputManager.Key(Key.F1));
            AddUntilStep("mod overlay shown", () => songSelect!.ModSelect.State.Value == Visibility.Visible);

            AddStep("toggle mod overlay off", () => InputManager.Key(Key.F1));
            AddUntilStep("mod overlay hidden", () => songSelect!.ModSelect.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestBeatmapOptionsDisabled()
        {
            createSongSelect();

            addRulesetImportStep(0);

            AddAssert("options enabled", () => songSelect.ChildrenOfType<FooterButtonOptions>().Single().Enabled.Value);
            AddStep("delete all beatmaps", () => manager.Delete());
            AddUntilStep("wait for no beatmap", () => Beatmap.IsDefault);
            AddAssert("options disabled", () => !songSelect.ChildrenOfType<FooterButtonOptions>().Single().Enabled.Value);
        }

        [Test]
        public void TestTextBoxBeatmapDifficultyCount()
        {
            createSongSelect();

            AddAssert("0 matching shown", () => songSelect.ChildrenOfType<FilterControl>().Single().InformationalText == "0 matches");

            addRulesetImportStep(0);

            AddAssert("3 matching shown", () => songSelect.ChildrenOfType<FilterControl>().Single().InformationalText == "3 matches");
            AddStep("delete all beatmaps", () => manager.Delete());
            AddUntilStep("wait for no beatmap", () => Beatmap.IsDefault);
            AddAssert("0 matching shown", () => songSelect.ChildrenOfType<FilterControl>().Single().InformationalText == "0 matches");
        }

        [Test]
        public void TestDeleteHotkey()
        {
            createSongSelect();

            addRulesetImportStep(0);
            AddAssert("3 matching shown", () => songSelect.ChildrenOfType<FilterControl>().Single().InformationalText == "3 matches");

            AddStep("press shift-delete", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Delete);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            AddUntilStep("delete dialog shown", () => DialogOverlay.CurrentDialog, Is.InstanceOf<BeatmapDeleteDialog>);
            AddStep("confirm deletion", () => DialogOverlay.CurrentDialog!.PerformAction<PopupDialogDangerousButton>());
            AddAssert("0 matching shown", () => songSelect.ChildrenOfType<FilterControl>().Single().InformationalText == "0 matches");
        }

        [Test]
        public void TestCutInFilterTextBox()
        {
            createSongSelect();

            AddStep("set filter text", () => songSelect!.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");
            AddStep("select all", () => InputManager.Keys(PlatformAction.SelectAll));
            AddStep("press ctrl-x", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.X);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddAssert("filter text cleared", () => songSelect!.FilterControl.ChildrenOfType<SearchTextBox>().First().Text, () => Is.Empty);
        }

        private void waitForInitialSelection()
        {
            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);
            AddUntilStep("wait for difficulty panels visible", () => songSelect!.Carousel.ChildrenOfType<DrawableCarouselBeatmap>().Any());
        }

        private int getBeatmapIndex(BeatmapSetInfo set, BeatmapInfo info) => set.Beatmaps.IndexOf(info);

        private NoResultsPlaceholder? getPlaceholder() => songSelect!.ChildrenOfType<NoResultsPlaceholder>().FirstOrDefault();

        private int getCurrentBeatmapIndex()
        {
            Debug.Assert(songSelect!.Carousel.SelectedBeatmapSet != null);
            Debug.Assert(songSelect!.Carousel.SelectedBeatmapInfo != null);

            return getBeatmapIndex(songSelect!.Carousel.SelectedBeatmapSet, songSelect!.Carousel.SelectedBeatmapInfo);
        }

        private int getDifficultyIconIndex(DrawableCarouselBeatmapSet set, FilterableDifficultyIcon icon)
        {
            return set.ChildrenOfType<FilterableDifficultyIcon>().ToList().FindIndex(i => i == icon);
        }

        private void addRulesetImportStep(int id)
        {
            Live<BeatmapSetInfo>? imported = null;
            AddStep($"import test map for ruleset {id}", () => imported = importForRuleset(id));
            // This is specifically for cases where the add is happening post song select load.
            // For cases where song select is null, the assertions are provided by the load checks.
            AddUntilStep("wait for imported to arrive in carousel", () => songSelect == null || songSelect!.Carousel.BeatmapSets.Any(s => s.ID == imported?.ID));
        }

        private Live<BeatmapSetInfo>? importForRuleset(int id) => manager.Import(TestResources.CreateTestBeatmapSetInfo(3, rulesets.AvailableRulesets.Where(r => r.OnlineID == id).ToArray()));

        private void checkMusicPlaying(bool playing) =>
            AddUntilStep($"music {(playing ? "" : "not ")}playing", () => music.IsPlaying == playing);

        private void changeMods(params Mod[] mods) => AddStep($"change mods to {string.Join(", ", mods.Select(m => m.Acronym))}", () => SelectedMods.Value = mods);

        private void changeRuleset(int id) => AddStep($"change ruleset to {id}", () => Ruleset.Value = rulesets.AvailableRulesets.First(r => r.OnlineID == id));

        private void createSongSelect()
        {
            AddStep("create song select", () => LoadScreen(songSelect = new TestSongSelect()));
            AddUntilStep("wait for present", () => songSelect!.IsCurrentScreen());
            AddUntilStep("wait for carousel loaded", () => songSelect!.Carousel.IsAlive);
        }

        /// <summary>
        /// Imports test beatmap sets to show in the carousel.
        /// </summary>
        /// <param name="difficultyCountPerSet">
        /// The exact count of difficulties to create for each beatmap set.
        /// A <see langword="null"/> value causes the count of difficulties to be selected randomly.
        /// </param>
        private void addManyTestMaps(int? difficultyCountPerSet = null)
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.OnlineID != 2).ToArray();

                for (int i = 0; i < 10; i++)
                    manager.Import(TestResources.CreateTestBeatmapSetInfo(difficultyCountPerSet, usableRulesets));
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }

        private void waitForDismissed() => AddUntilStep("wait for not current", () => !songSelect.AsNonNull().IsCurrentScreen());

        private partial class TestSongSelect : PlaySongSelect
        {
            public Action? StartRequested;

            public new Bindable<RulesetInfo> Ruleset => base.Ruleset;

            public new FilterControl FilterControl => base.FilterControl;

            public WorkingBeatmap CurrentBeatmap => Beatmap.Value;
            public IWorkingBeatmap CurrentBeatmapDetailsBeatmap => BeatmapDetails.Beatmap;
            public new BeatmapCarousel Carousel => base.Carousel;
            public new ModSelectOverlay ModSelect => base.ModSelect;

            public new void PresentScore(ScoreInfo score) => base.PresentScore(score);

            protected override bool OnStart()
            {
                StartRequested?.Invoke();
                return base.OnStart();
            }

            public int FilterCount;

            protected override void ApplyFilterToCarousel(FilterCriteria criteria)
            {
                FilterCount++;
                base.ApplyFilterToCarousel(criteria);
            }
        }
    }
}
