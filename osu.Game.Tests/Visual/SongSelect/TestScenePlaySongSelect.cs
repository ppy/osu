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
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
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

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Screens.Select.SongSelect),
            typeof(BeatmapCarousel),

            typeof(CarouselItem),
            typeof(CarouselGroup),
            typeof(CarouselGroupEagerSelect),
            typeof(CarouselBeatmap),
            typeof(CarouselBeatmapSet),

            typeof(DrawableCarouselItem),
            typeof(CarouselItemState),

            typeof(DrawableCarouselBeatmap),
            typeof(DrawableCarouselBeatmapSet),
        };

        private TestSongSelect songSelect;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, defaultBeatmap = Beatmap.Default));

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

            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.PressKey(Key.Down);
                InputManager.ReleaseKey(Key.Down);
                InputManager.PressKey(Key.Enter);
                InputManager.ReleaseKey(Key.Enter);
            });

            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());
            AddAssert("ensure selection changed", () => selected != Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapAfterEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.PressKey(Key.Enter);
                InputManager.ReleaseKey(Key.Enter);
                InputManager.PressKey(Key.Down);
                InputManager.ReleaseKey(Key.Down);
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

            AddStep("select next and enter", () =>
            {
                InputManager.MoveMouseTo(songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmap>()
                                                   .First(b => ((CarouselBeatmap)b.Item).Beatmap != songSelect.Carousel.SelectedBeatmap));

                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);

                InputManager.PressKey(Key.Enter);
                InputManager.ReleaseKey(Key.Enter);
            });

            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());
            AddAssert("ensure selection changed", () => selected != Beatmap.Value);
        }

        [Test]
        public void TestChangeBeatmapViaMouseAfterEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddUntilStep("wait for initial selection", () => !Beatmap.IsDefault);

            WorkingBeatmap selected = null;

            AddStep("store selected beatmap", () => selected = Beatmap.Value);

            AddStep("select next and enter", () =>
            {
                InputManager.MoveMouseTo(songSelect.Carousel.ChildrenOfType<DrawableCarouselBeatmap>()
                                                   .First(b => ((CarouselBeatmap)b.Item).Beatmap != songSelect.Carousel.SelectedBeatmap));

                InputManager.PressButton(MouseButton.Left);

                InputManager.PressKey(Key.Enter);
                InputManager.ReleaseKey(Key.Enter);

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

            AddStep("change convert setting", () => config.Set(OsuSetting.ShowConvertedBeatmaps, false));

            createSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            AddUntilStep("wait for not current", () => !songSelect.IsCurrentScreen());

            AddStep("change convert setting", () => config.Set(OsuSetting.ShowConvertedBeatmaps, true));

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
                    manager.Import(createTestBeatmapSet(0, usableRulesets)).Wait();
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

            var sortMode = config.GetBindable<SortMode>(OsuSetting.SongSelectSortingMode);

            AddStep(@"Sort by Artist", delegate { sortMode.Value = SortMode.Artist; });
            AddStep(@"Sort by Title", delegate { sortMode.Value = SortMode.Title; });
            AddStep(@"Sort by Author", delegate { sortMode.Value = SortMode.Author; });
            AddStep(@"Sort by DateAdded", delegate { sortMode.Value = SortMode.DateAdded; });
            AddStep(@"Sort by BPM", delegate { sortMode.Value = SortMode.BPM; });
            AddStep(@"Sort by Length", delegate { sortMode.Value = SortMode.Length; });
            AddStep(@"Sort by Difficulty", delegate { sortMode.Value = SortMode.Difficulty; });
        }

        [Test]
        public void TestImportUnderDifferentRuleset()
        {
            createSongSelect();
            addRulesetImportStep(2);
            AddUntilStep("no selection", () => songSelect.Carousel.SelectedBeatmap == null);
        }

        [Test]
        public void TestImportUnderCurrentRuleset()
        {
            createSongSelect();
            changeRuleset(2);
            addRulesetImportStep(2);
            addRulesetImportStep(1);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap.RulesetID == 2);

            changeRuleset(1);
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap.RulesetID == 1);

            changeRuleset(0);
            AddUntilStep("no selection", () => songSelect.Carousel.SelectedBeatmap == null);
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
            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap != null);

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

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap != null);

            AddStep("set filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");

            AddUntilStep("dummy selected", () => Beatmap.Value is DummyWorkingBeatmap);

            AddUntilStep("has no selection", () => songSelect.Carousel.SelectedBeatmap == null);

            BeatmapInfo target = null;

            AddStep("select beatmap externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets().Where(b => b.Beatmaps.Any(bi => bi.RulesetID == (differentRuleset ? 1 : 0)))
                                .ElementAt(5).Beatmaps.First();

                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap != null);

            AddUntilStep("carousel has correct", () => songSelect.Carousel.SelectedBeatmap?.OnlineBeatmapID == target.OnlineBeatmapID);
            AddUntilStep("game has correct", () => Beatmap.Value.BeatmapInfo.OnlineBeatmapID == target.OnlineBeatmapID);

            AddStep("reset filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = string.Empty);

            AddAssert("game still correct", () => Beatmap.Value?.BeatmapInfo.OnlineBeatmapID == target.OnlineBeatmapID);
            AddAssert("carousel still correct", () => songSelect.Carousel.SelectedBeatmap.OnlineBeatmapID == target.OnlineBeatmapID);
        }

        [Test]
        public void TestExternalBeatmapChangeWhileFilteredThenRefilter()
        {
            createSongSelect();
            addManyTestMaps();

            changeRuleset(0);

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap != null);

            AddStep("set filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nonono");

            AddUntilStep("dummy selected", () => Beatmap.Value is DummyWorkingBeatmap);

            AddUntilStep("has no selection", () => songSelect.Carousel.SelectedBeatmap == null);

            BeatmapInfo target = null;

            AddStep("select beatmap externally", () =>
            {
                target = manager.GetAllUsableBeatmapSets().Where(b => b.Beatmaps.Any(bi => bi.RulesetID == 1))
                                .ElementAt(5).Beatmaps.First();

                Beatmap.Value = manager.GetWorkingBeatmap(target);
            });

            AddUntilStep("has selection", () => songSelect.Carousel.SelectedBeatmap != null);

            AddUntilStep("carousel has correct", () => songSelect.Carousel.SelectedBeatmap?.OnlineBeatmapID == target.OnlineBeatmapID);
            AddUntilStep("game has correct", () => Beatmap.Value.BeatmapInfo.OnlineBeatmapID == target.OnlineBeatmapID);

            AddStep("set filter text", () => songSelect.FilterControl.ChildrenOfType<SearchTextBox>().First().Text = "nononoo");

            AddUntilStep("game lost selection", () => Beatmap.Value is DummyWorkingBeatmap);
            AddAssert("carousel lost selection", () => songSelect.Carousel.SelectedBeatmap == null);
        }

        [Test]
        public void TestAutoplayViaCtrlEnter()
        {
            addRulesetImportStep(0);

            createSongSelect();

            AddStep("press ctrl+enter", () =>
            {
                InputManager.PressKey(Key.ControlLeft);
                InputManager.PressKey(Key.Enter);

                InputManager.ReleaseKey(Key.ControlLeft);
                InputManager.ReleaseKey(Key.Enter);
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
            AddStep("Store current ID", () => previousID = songSelect.Carousel.SelectedBeatmap.ID);
            AddStep("Hide first beatmap", () => manager.Hide(songSelect.Carousel.SelectedBeatmapSet.Beatmaps.First()));
            AddAssert("Selected beatmap has not changed", () => songSelect.Carousel.SelectedBeatmap.ID == previousID);
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

            DrawableCarouselBeatmapSet.FilterableDifficultyIcon difficultyIcon = null;
            AddStep("Find an icon", () =>
            {
                difficultyIcon = set.ChildrenOfType<DrawableCarouselBeatmapSet.FilterableDifficultyIcon>()
                                    .First(icon => getDifficultyIconIndex(set, icon) != getCurrentBeatmapIndex());
            });
            AddStep("Click on a difficulty", () =>
            {
                InputManager.MoveMouseTo(difficultyIcon);

                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
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

            DrawableCarouselBeatmapSet.FilterableDifficultyIcon filteredIcon = null;
            AddStep("Get filtered icon", () =>
            {
                var filteredBeatmap = songSelect.Carousel.SelectedBeatmapSet.Beatmaps.Find(b => b.BPM < maxBPM);
                int filteredBeatmapIndex = getBeatmapIndex(filteredBeatmap.BeatmapSet, filteredBeatmap);
                filteredIcon = set.ChildrenOfType<DrawableCarouselBeatmapSet.FilterableDifficultyIcon>().ElementAt(filteredBeatmapIndex);
            });

            int? previousID = null;
            AddStep("Store current ID", () => previousID = songSelect.Carousel.SelectedBeatmap.ID);
            AddStep("Click on a filtered difficulty", () =>
            {
                InputManager.MoveMouseTo(filteredIcon);

                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });
            AddAssert("Selected beatmap has not changed", () => songSelect.Carousel.SelectedBeatmap.ID == previousID);
        }

        private int getBeatmapIndex(BeatmapSetInfo set, BeatmapInfo info) => set.Beatmaps.FindIndex(b => b == info);

        private int getCurrentBeatmapIndex() => getBeatmapIndex(songSelect.Carousel.SelectedBeatmapSet, songSelect.Carousel.SelectedBeatmap);

        private int getDifficultyIconIndex(DrawableCarouselBeatmapSet set, DrawableCarouselBeatmapSet.FilterableDifficultyIcon icon)
        {
            return set.ChildrenOfType<DrawableCarouselBeatmapSet.FilterableDifficultyIcon>().ToList().FindIndex(i => i == icon);
        }

        private void addRulesetImportStep(int id) => AddStep($"import test map for ruleset {id}", () => importForRuleset(id));

        private void importForRuleset(int id) => manager.Import(createTestBeatmapSet(getImportId(), rulesets.AvailableRulesets.Where(r => r.ID == id).ToArray())).Wait();

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
        }

        private void addManyTestMaps()
        {
            AddStep("import test maps", () =>
            {
                var usableRulesets = rulesets.AvailableRulesets.Where(r => r.ID != 2).ToArray();

                for (int i = 0; i < 100; i += 10)
                    manager.Import(createTestBeatmapSet(i, usableRulesets)).Wait();
            });
        }

        private BeatmapSetInfo createTestBeatmapSet(int setId, RulesetInfo[] rulesets)
        {
            int j = 0;
            RulesetInfo getRuleset() => rulesets[j++ % rulesets.Length];

            var beatmaps = new List<BeatmapInfo>();

            for (int i = 0; i < 6; i++)
            {
                int beatmapId = setId * 10 + i;

                int length = RNG.Next(30000, 200000);
                double bpm = RNG.NextSingle(80, 200);

                beatmaps.Add(new BeatmapInfo
                {
                    Ruleset = getRuleset(),
                    OnlineBeatmapID = beatmapId,
                    Path = "normal.osu",
                    Version = $"{beatmapId} (length {TimeSpan.FromMilliseconds(length):m\\:ss}, bpm {bpm:0.#})",
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
                OnlineBeatmapSetID = setId,
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
            public WorkingBeatmap CurrentBeatmapDetailsBeatmap => BeatmapDetails.Beatmap;
            public new BeatmapCarousel Carousel => base.Carousel;

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
