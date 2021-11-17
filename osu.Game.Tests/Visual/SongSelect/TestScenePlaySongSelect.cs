// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Utils;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;
using osuTK.Input;

namespace osu.Game.Tests.Visual.SongSelect
{
    [TestFixture]
    public class TestScenePlaySongSelect : ScreenTestScene
    {
        private BeatmapManager manager;
        private RulesetStore rulesets;
        private MusicController music;
        private WorkingBeatmap defaultBeatmap;
        private TestSongSelect songSelect;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, defaultBeatmap = Beatmap.Default));

            Dependencies.Cache(music = new MusicController());

            // required to get bindables attached
            Add(music);

            Dependencies.Cache(config = new OsuConfigManager(LocalStorage));
        }

        private OsuConfigManager config;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("delete all beatmaps", () =>
            {
                Ruleset.Value = new OsuRuleset().RulesetInfo;
                manager?.Delete(manager.GetAllUsableBeatmapSets());

                Beatmap.SetDefault();
            });
        }

        [Test]
        public void TestSingleFilterOnEnter()
        {
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            createSongSelect();

            AddAssert("filter count is 1", () => songSelect.FilterCount == 1);
        }

        [Test]
        public void TestChangeBeatmapBeforeEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            waitForInitialSelection();

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.Key(Key.Down);
                InputManager.Key(Key.Enter);
            });

            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());
            AddAssert("ensure selection changed", () => selected != Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapAfterEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            waitForInitialSelection();

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.Key(Key.Enter);
                InputManager.Key(Key.Down);
            });

            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());
            AddAssert("ensure selection didn't change", () => selected == Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapViaMouseBeforeEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddUntilStep("wait for beatmaps to load", () => songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmap>().Any());

            AddStep("select next and enter", () =>
            {
                InputManager.MoveMouseTo(songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmap>()
                                                   .First(b => ((CarouselBeatmap)b.Item).BeatmapInfo != songSelect.Carousel.SelectedBeatmapInfo));

                InputManager.Click(MouseButton.Left);

                InputManager.Key(Key.Enter);
            });

            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());
            AddAssert("ensure selection changed", () => selected != Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapViaMouseAfterEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            waitForInitialSelection();

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.MoveMouseTo(songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmap>()
                                                   .First(b => ((CarouselBeatmap)b.Item).BeatmapInfo != songSelect.Carousel.SelectedBeatmapInfo));

                InputManager.PressButton(MouseButton.Left);

                InputManager.Key(Key.Enter);

                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());
            AddAssert("ensure selection didn't change", () => selected == Beatmap.Value);
        }

        [Test]
        public void TestNoFilterOnSimpleResume()
        {
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            createSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());

            AddStep("return", () => songSelect.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect.IsCurrentScreen());
            AddAssert("filter count is 1", () => songSelect.FilterCount == 1);
        }

        [Test]
        public void TestFilterOnResumeAfterChange()
        {
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            createSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            AddStep("return", () => songSelect.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect.IsCurrentScreen());
            AddAssert("filter count is 2", () => songSelect.FilterCount == 2);
        }

        [Test]
        public void TestAudioResuming()
        {
            createSongSelect();

            addRulesetImportStep(0);
            addRulesetImportStep(0);

            checkMusicPlaying(true);
            AddStep("select first", () => songSelect.Carousel.SelectBeatmap(songSelect.Carousel.BeatmapSets.First().Beatmaps.First()));
            checkMusicPlaying(true);

            AddStep("manual pause", () => music.TogglePause());
            checkMusicPlaying(false);
            AddStep("select next difficulty", () => songSelect.Carousel.SelectNext(skipDifficulties: false));
            checkMusicPlaying(false);

            AddStep("select next set", () => songSelect.Carousel.SelectNext());
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
                    var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();
                    manager.Import(createTestBeatmapSet(usableRulesets)).Wait();
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
            AddAssert("dummy selected", () => songSelect.CurrentBeatmap == defaultBeatmap);

            AddUntilStep("dummy shown on wedge", () => songSelect.CurrentBeatmapDetailsBeatmap == defaultBeatmap);

            addManyTestMaps();
            AddWaitStep("wait for select", 3);

            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);
        }

        [Test]
        public void TestSorting()
        {
            createSongSelect();
            addManyTestMaps();
            AddWaitStep("wait for add", 3);

            AddAssert("random map selected", () => songSelect.CurrentBeatmap != defaultBeatmap);

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
            AddUntilStep("no selection", () => songSelect.Carousel.SelectedBeatmapInfo == null);
        }

        [Test]
        public void TestImportUnderCurrentRuleset()
        {
            createSongSelect();
            changeRuleset(2);
            addRulesetImportStep(2);
            addRulesetImportStep(1);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo.RulesetID == 2);

            changeRuleset(1);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo.RulesetID == 1);

            changeRuleset(0);
            AddUntilStep("no selection", () => songSelect.Carousel.SelectedBeatmapInfo == null);
        }

        [Test]
        public void TestPresentNewRulesetNewBeatmap()
        {
            createSongSelect();
            changeRuleset(2);

            addRulesetImportStep(2);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo.RulesetID == 2);

            addRulesetImportStep(0);
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            BeatmapInfo target = null;

            AddStep("select beatmap/ruleset externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets()
                                .Last(b => b.Beatmaps.Any(bi => bi.RulesetID == 0)).Beatmaps.Last();

                Ruleset.Value = rulesets.AvailableRulesets.First(r => r.ID == 0);
                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo.Equals(target));

            // this is an important check, to make sure updateComponentFromBeatmap() was actually run
            AddUntilStep("selection shown on wedge", () => songSelect.CurrentBeatmapDetailsBeatmap.BeatmapInfo.MatchesOnlineID(target));
        }

        [Test]
        public void TestPresentNewBeatmapNewRuleset()
        {
            createSongSelect();
            changeRuleset(2);

            addRulesetImportStep(2);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo.RulesetID == 2);

            addRulesetImportStep(0);
            addRulesetImportStep(0);
            addRulesetImportStep(0);

            BeatmapInfo target = null;

            AddStep("select beatmap/ruleset externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets()
                                .Last(b => b.Beatmaps.Any(bi => bi.RulesetID == 0)).Beatmaps.Last();

                Beatmap.Value = manager.GetWorkingBeatmap(target);
                Ruleset.Value = rulesets.AvailableRulesets.First(r => r.ID == 0);
            });

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo.Equals(target));

            AddUntilStep("has correct ruleset", () => Ruleset.Value.ID == 0);

            // this is an important check, to make sure updateComponentFromBeatmap() was actually run
            AddUntilStep("selection shown on wedge", () => songSelect.CurrentBeatmapDetailsBeatmap.BeatmapInfo.MatchesOnlineID(target));
        }

        [Test]
        public void TestRulesetChangeResetsMods()
        {
            createSongSelect();
            changeRuleset(0);

            changeMods(new OsuModHardRock());

            int actionIndex = 0;
            int modChangeIndex = 0;
            int rulesetChangeIndex = 0;

            AddStep("change ruleset", () =>
            {
                SelectedMods.ValueChanged += onModChange;
                songSelect.Ruleset.ValueChanged += onRulesetChange;

                Ruleset.Value = new TaikoRuleset().RulesetInfo;

                SelectedMods.ValueChanged -= onModChange;
                songSelect.Ruleset.ValueChanged -= onRulesetChange;
            });

            AddAssert("mods changed before ruleset", () => modChangeIndex < rulesetChangeIndex);
            AddAssert("empty mods", () => !SelectedMods.Value.Any());

            void onModChange(ValueChangedEvent<IReadOnlyList<Mod>> e) => modChangeIndex = actionIndex++;
            void onRulesetChange(ValueChangedEvent<RulesetInfo> e) => rulesetChangeIndex = actionIndex++;
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
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo != null);

            bool startRequested = false;

            AddStep("set filter and finalize", () =>
            {
                songSelect.StartRequested = () => startRequested = true;

                songSelect.Carousel.Filter(new FilterCriteria { SearchText = "somestringthatshouldn'tbematchable" });
                songSelect.FinaliseSelection();

                songSelect.StartRequested = null;
            });

            AddAssert("start not requested", () => !startRequested);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestExternalBeatmapChangeWhileFiltered(bool differentRuleset)
        {
            createSongSelect();
            addManyTestMaps();

            changeRuleset(0);

            // used for filter check below
            AddStep("allow convert display", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo != null);

            AddStep("set filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");

            AddUntilStep("dummy selected", () => Beatmap.Value is DummyWorkingBeatmap);

            AddUntilStep("has no selection", () => songSelect.Carousel.SelectedBeatmapInfo == null);

            BeatmapInfo target = null;

            int targetRuleset = differentRuleset ? 1 : 0;

            AddStep("select beatmap externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets()
                                .Where(b => b.Beatmaps.Any(bi => bi.RulesetID == targetRuleset))
                                .ElementAt(5).Beatmaps.First(bi => bi.RulesetID == targetRuleset);

                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo != null);

            AddAssert("selected only shows expected ruleset (plus converts)", () =>
            {
                var selectedPanel = songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().First(s => s.Item.State.Value == CarouselItemState.Selected);

                // special case for converts checked here.
                return selectedPanel.ChildrenOfType<FilterableDifficultyIcon>().All(i =>
                    i.IsFiltered || i.Item.BeatmapInfo.Ruleset.ID == targetRuleset || i.Item.BeatmapInfo.Ruleset.ID == 0);
            });

            AddUntilStep("carousel has correct", () => songSelect.Carousel.SelectedBeatmapInfo?.MatchesOnlineID(target) == true);
            AddUntilStep("game has correct", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(target));

            AddStep("reset filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = string.Empty);

            AddAssert("game still correct", () => Beatmap.Value?.BeatmapInfo.MatchesOnlineID(target) == true);
            AddAssert("carousel still correct", () => songSelect.Carousel.SelectedBeatmapInfo.MatchesOnlineID(target));
        }

        [Test]
        public void TestExternalBeatmapChangeWhileFilteredThenRefilter()
        {
            createSongSelect();
            addManyTestMaps();

            changeRuleset(0);

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo != null);

            AddStep("set filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");

            AddUntilStep("dummy selected", () => Beatmap.Value is DummyWorkingBeatmap);

            AddUntilStep("has no selection", () => songSelect.Carousel.SelectedBeatmapInfo == null);

            BeatmapInfo target = null;

            AddStep("select beatmap externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets().Where(b => b.Beatmaps.Any(bi => bi.RulesetID == 1))
                                .ElementAt(5).Beatmaps.First();

                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmapInfo != null);

            AddUntilStep("carousel has correct", () => songSelect.Carousel.SelectedBeatmapInfo?.MatchesOnlineID(target) == true);
            AddUntilStep("game has correct", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(target));

            AddStep("set filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nononoo");

            AddUntilStep("game lost selection", () => Beatmap.Value is DummyWorkingBeatmap);
            AddAssert("carousel lost selection", () => songSelect.Carousel.SelectedBeatmapInfo == null);
        }

        [Test]
        public void TestAutoplayViaCtrlEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.Key(Key.Enter);
                InputManager.ReleaseKey(Key.ControlLeft);
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is PlayerLoader);

            AddAssert("autoplay enabled", () => songSelect.Mods.Value.FirstOrDefault() is ModAutoplay);

            AddUntilStep("wait for return to ss", () => songSelect.IsCurrentScreen());

            AddAssert("mod disabled", () => songSelect.Mods.Value.Count == 0);
        }

        [Test]
        public void TestHideSetSelectsCorrectBeatmap()
        {
            int? previousID = null;
            createSongSelect();
            addRulesetImportStep(0);
            AddStep("Move to last difficulty", () => songSelect.Carousel.SelectBeatmap(songSelect.Carousel.BeatmapSets.First().Beatmaps.Last()));
            AddStep("Store current ID", () => previousID = songSelect.Carousel.SelectedBeatmapInfo.ID);
            AddStep("Hide first beatmap", () => manager.Hide(songSelect.Carousel.SelectedBeatmapSet.Beatmaps.First()));
            AddAssert("Selected beatmap has not changed", () => songSelect.Carousel.SelectedBeatmapInfo.ID == previousID);
        }

        [Test]
        public void TestDifficultyIconSelecting()
        {
            addRulesetImportStep(0);
            createSongSelect();

            DrawableCarouselBeatmapSet set = null;
            AddStep("Find the DrawableCarouselBeatmapSet", () =>
            {
                set = songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().First();
            });

            FilterableDifficultyIcon difficultyIcon = null;
            AddUntilStep("Find an icon", () =>
            {
                return (difficultyIcon = set.ChildrenOfType<FilterableDifficultyIcon>()
                                            .FirstOrDefault(icon => getDifficultyIconIndex(set, icon) != getCurrentBeatmapIndex())) != null;
            });

            AddStep("Click on a difficulty", () =>
            {
                InputManager.MoveMouseTo(difficultyIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Selected beatmap correct", () => getCurrentBeatmapIndex() == getDifficultyIconIndex(set, difficultyIcon));

            double? maxBPM = null;
            AddStep("Filter some difficulties", () => songSelect.Carousel.Filter(new FilterCriteria
            {
                BPM = new FilterCriteria.OptionalRange<double>
                {
                    Min = maxBPM = songSelect.Carousel.SelectedBeatmapSet.MaxBPM,
                    IsLowerInclusive = true
                }
            }));

            BeatmapInfo filteredBeatmap = null;
            FilterableDifficultyIcon filteredIcon = null;

            AddStep("Get filtered icon", () =>
            {
                filteredBeatmap = songSelect.Carousel.SelectedBeatmapSet.Beatmaps.First(b => b.BPM < maxBPM);
                int filteredBeatmapIndex = getBeatmapIndex(filteredBeatmap.BeatmapSet, filteredBeatmap);
                filteredIcon = set.ChildrenOfType<FilterableDifficultyIcon>().ElementAt(filteredBeatmapIndex);
            });

            AddStep("Click on a filtered difficulty", () =>
            {
                InputManager.MoveMouseTo(filteredIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Selected beatmap correct", () => songSelect.Carousel.SelectedBeatmapInfo == filteredBeatmap);
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
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();
                manager.Import(createTestBeatmapSet(usableRulesets)).Wait();
            });

            int previousSetID = 0;

            AddUntilStep("wait for selection", () => !Beatmap.IsDefault);

            AddStep("record set ID", () => previousSetID = ((IBeatmapSetInfo)Beatmap.Value.BeatmapSetInfo).OnlineID);
            AddAssert("selection changed once", () => changeCount == 1);

            AddAssert("Check ruleset is osu!", () => Ruleset.Value.ID == 0);

            changeRuleset(3);

            AddUntilStep("Check ruleset changed to mania", () => Ruleset.Value.ID == 3);

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
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();
                manager.Import(createTestBeatmapSet(usableRulesets)).Wait();
            });

            DrawableCarouselBeatmapSet set = null;
            AddUntilStep("Find the DrawableCarouselBeatmapSet", () =>
            {
                set = songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().FirstOrDefault();
                return set != null;
            });

            FilterableDifficultyIcon difficultyIcon = null;
            AddUntilStep("Find an icon for different ruleset", () =>
            {
                difficultyIcon = set.ChildrenOfType<FilterableDifficultyIcon>()
                                    .FirstOrDefault(icon => icon.Item.BeatmapInfo.Ruleset.ID == 3);
                return difficultyIcon != null;
            });

            AddAssert("Check ruleset is osu!", () => Ruleset.Value.ID == 0);

            int previousSetID = 0;

            AddStep("record set ID", () => previousSetID = ((IBeatmapSetInfo)Beatmap.Value.BeatmapSetInfo).OnlineID);

            AddStep("Click on a difficulty", () =>
            {
                InputManager.MoveMouseTo(difficultyIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Check ruleset changed to mania", () => Ruleset.Value.ID == 3);

            AddAssert("Selected beatmap still same set", () => songSelect.Carousel.SelectedBeatmapInfo.BeatmapSet.OnlineID == previousSetID);
            AddAssert("Selected beatmap is mania", () => Beatmap.Value.BeatmapInfo.Ruleset.OnlineID == 3);
        }

        [Test]
        public void TestGroupedDifficultyIconSelecting()
        {
            changeRuleset(0);

            createSongSelect();

            BeatmapSetInfo imported = null;

            AddStep("import huge difficulty count map", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();
                imported = manager.Import(createTestBeatmapSet(usableRulesets, 50)).Result.Value;
            });

            AddStep("select the first beatmap of import", () => Beatmap.Value = manager.GetWorkingBeatmap(imported.Beatmaps.First()));

            DrawableCarouselBeatmapSet set = null;
            AddUntilStep("Find the DrawableCarouselBeatmapSet", () =>
            {
                set = songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmapSet>().FirstOrDefault();
                return set != null;
            });

            FilterableGroupedDifficultyIcon groupIcon = null;
            AddUntilStep("Find group icon for different ruleset", () =>
            {
                return (groupIcon = set.ChildrenOfType<FilterableGroupedDifficultyIcon>()
                                       .FirstOrDefault(icon => icon.Items.First().BeatmapInfo.Ruleset.ID == 3)) != null;
            });

            AddAssert("Check ruleset is osu!", () => Ruleset.Value.ID == 0);

            AddStep("Click on group", () =>
            {
                InputManager.MoveMouseTo(groupIcon);

                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Check ruleset changed to mania", () => Ruleset.Value.ID == 3);

            AddAssert("Check first item in group selected", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(groupIcon.Items.First().BeatmapInfo));
        }

        [Test]
        public void TestChangeRulesetWhilePresentingScore()
        {
            BeatmapInfo getPresentBeatmap() => manager.QueryBeatmap(b => !b.BeatmapSet.DeletePending && b.RulesetID == 0);
            BeatmapInfo getSwitchBeatmap() => manager.QueryBeatmap(b => !b.BeatmapSet.DeletePending && b.RulesetID == 1);

            changeRuleset(0);

            createSongSelect();

            addRulesetImportStep(0);
            addRulesetImportStep(1);

            AddStep("present score", () =>
            {
                // this ruleset change should be overridden by the present.
                Ruleset.Value = getSwitchBeatmap().Ruleset;

                songSelect.PresentScore(new ScoreInfo
                {
                    User = new APIUser { Username = "woo" },
                    BeatmapInfo = getPresentBeatmap(),
                    Ruleset = getPresentBeatmap().Ruleset
                });
            });

            AddUntilStep("wait for results screen presented", () => !songSelect.IsCurrentScreen());

            AddAssert("check beatmap is correct for score", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(getPresentBeatmap()));
            AddAssert("check ruleset is correct for score", () => Ruleset.Value.ID == 0);
        }

        [Test]
        public void TestChangeBeatmapWhilePresentingScore()
        {
            BeatmapInfo getPresentBeatmap() => manager.QueryBeatmap(b => !b.BeatmapSet.DeletePending && b.RulesetID == 0);
            BeatmapInfo getSwitchBeatmap() => manager.QueryBeatmap(b => !b.BeatmapSet.DeletePending && b.RulesetID == 1);

            changeRuleset(0);

            addRulesetImportStep(0);
            addRulesetImportStep(1);

            createSongSelect();

            AddStep("present score", () =>
            {
                // this beatmap change should be overridden by the present.
                Beatmap.Value = manager.GetWorkingBeatmap(getSwitchBeatmap());

                songSelect.PresentScore(new ScoreInfo
                {
                    User = new APIUser { Username = "woo" },
                    BeatmapInfo = getPresentBeatmap(),
                    Ruleset = getPresentBeatmap().Ruleset
                });
            });

            AddUntilStep("wait for results screen presented", () => !songSelect.IsCurrentScreen());

            AddAssert("check beatmap is correct for score", () => Beatmap.Value.BeatmapInfo.MatchesOnlineID(getPresentBeatmap()));
            AddAssert("check ruleset is correct for score", () => Ruleset.Value.ID == 0);
        }

        private void waitForInitialSelection()
        {
            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);
            AddUntilStep("wait for difficulty panels visible", () => songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmap>().Any());
        }

        private int getBeatmapIndex(BeatmapSetInfo set, BeatmapInfo info) => set.Beatmaps.FindIndex(b => b == info);

        private int getCurrentBeatmapIndex() => getBeatmapIndex(songSelect.Carousel.SelectedBeatmapSet, songSelect.Carousel.SelectedBeatmapInfo);

        private int getDifficultyIconIndex(DrawableCarouselBeatmapSet set, FilterableDifficultyIcon icon)
        {
            return set.ChildrenOfType<FilterableDifficultyIcon>().ToList().FindIndex(i => i == icon);
        }

        private void addRulesetImportStep(int id) => AddStep($"import test map for ruleset {id}", () => importForRuleset(id));

        private void importForRuleset(int id) => manager.Import(createTestBeatmapSet(rulesets.AvailableRulesets.Where(r => r.ID == id).ToArray())).Wait();

        private static int importId;

        private int getImportId() => ++importId;

        private void checkMusicPlaying(bool playing) =>
            AddUntilStep($"music {(playing ? "" : "not ")}playing", () => music.IsPlaying == playing);

        private void changeMods(params Mod[] mods) => AddStep($"change mods to {string.Join(", ", mods.Select(m => m.Acronym))}", () => SelectedMods.Value = mods);

        private void changeRuleset(int id) => AddStep($"change ruleset to {id}", () => Ruleset.Value = rulesets.AvailableRulesets.First(r => r.ID == id));

        private void createSongSelect()
        {
            AddStep("create song select", () => LoadScreen(songSelect = new TestSongSelect()));
            AddUntilStep("wait for present", () => songSelect.IsCurrentScreen());
            AddUntilStep("wait for carousel loaded", () => songSelect.Carousel.IsAlive);
        }

        private void addManyTestMaps()
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();

                for (int i = 0; i < 100; i += 10)
                    manager.Import(createTestBeatmapSet(usableRulesets)).Wait();
            });
        }

        private BeatmapSetInfo createTestBeatmapSet(RulesetInfo[] rulesets, int countPerRuleset = 6)
        {
            int j = 0;
            RulesetInfo getRuleset() => rulesets[j++ % rulesets.Length];

            int setId = getImportId();

            var beatmaps = new List<BeatmapInfo>();

            for (int i = 0; i < countPerRuleset; i++)
            {
                int beatmapId = setId * 1000 + i;

                int length = RNG.Next(30000, 200000);
                double bpm = RNG.NextSingle(80, 200);

                beatmaps.Add(new BeatmapInfo
                {
                    Ruleset = getRuleset(),
                    OnlineID = beatmapId,
                    DifficultyName = $"{beatmapId} (length {TimeSpan.FromMilliseconds(length):m\\:ss}, bpm {bpm:0.#})",
                    Length = length,
                    BPM = bpm,
                    BaseDifficulty = new BeatmapDifficulty
                    {
                        OverallDifficulty = 3.5f,
                    },
                });
            }

            return new BeatmapSetInfo
            {
                OnlineID = setId,
                Hash = new MemoryStream(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString())).ComputeMD5Hash(),
                Metadata = new BeatmapMetadata
                {
                    // Create random metadata, then we can check if sorting works based on these
                    Artist = "Some Artist " + RNG.Next(0, 9),
                    Title = $"Some Song (set id {setId}, max bpm {beatmaps.Max(b => b.BPM):0.#})",
                    AuthorString = "Some Guy " + RNG.Next(0, 9),
                },
                Beatmaps = beatmaps,
                DateAdded = DateTimeOffset.UtcNow,
            };
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            rulesets?.Dispose();
        }

        private class TestSongSelect : PlaySongSelect
        {
            public Action StartRequested;

            public new Bindable<RulesetInfo> Ruleset => base.Ruleset;

            public new FilterControl FilterControl => base.FilterControl;

            public WorkingBeatmap CurrentBeatmap => Beatmap.Value;
            public IWorkingBeatmap CurrentBeatmapDetailsBeatmap => BeatmapDetails.Beatmap;
            public new BeatmapCarousel Carousel => base.Carousel;

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
