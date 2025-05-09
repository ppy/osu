// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using osu.Game.Tests.Resources;
using osuTK.Input;
using BeatmapCarousel = osu.Game.Screens.SelectV2.BeatmapCarousel;
using FilterControl = osu.Game.Screens.SelectV2.FilterControl;
using NoResultsPlaceholder = osu.Game.Screens.SelectV2.NoResultsPlaceholder;
using BeatmapDeleteDialog = osu.Game.Screens.Select.BeatmapDeleteDialog;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectFiltering : ScreenTestScene
    {
        private BeatmapManager manager = null!;
        private RealmRulesetStore rulesets = null!;

        private OsuConfigManager config = null!;

        private SoloSongSelect songSelect = null!;
        private BeatmapCarousel carousel => songSelect.ChildrenOfType<BeatmapCarousel>().Single();

        private FilterControl filter => songSelect.ChildrenOfType<FilterControl>().Single();
        private ShearedFilterTextBox filterTextBox => songSelect.ChildrenOfType<ShearedFilterTextBox>().Single();
        private int filterOperationsCount;

        [Cached]
        private readonly ScreenFooter screenFooter;

        [Cached]
        private readonly OsuLogo logo;

        [Cached(typeof(INotificationOverlay))]
        private readonly INotificationOverlay notificationOverlay = new NotificationOverlay();

        public TestSceneSongSelectFiltering()
        {
            Children = new Drawable[]
            {
                new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Toolbar
                        {
                            State = { Value = Visibility.Visible },
                        },
                        screenFooter = new ScreenFooter
                        {
                            OnBack = () => Stack.CurrentScreen.Exit(),
                        },
                        logo = new OsuLogo
                        {
                            Alpha = 0f,
                        },
                    },
                },
            };

            Stack.Padding = new MarginPadding { Top = Toolbar.HEIGHT };
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            RealmDetachedBeatmapStore beatmapStore;

            // These DI caches are required to ensure for interactive runs this test scene doesn't nuke all user beatmaps in the local install.
            // At a point we have isolated interactive test runs enough, this can likely be removed.
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(Realm);
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, Audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs<BeatmapStore>(beatmapStore = new RealmDetachedBeatmapStore());

            Add(beatmapStore);

            Dependencies.Cache(config = new OsuConfigManager(LocalStorage));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Stack.ScreenPushed += updateFooter;
            Stack.ScreenExited += updateFooter;
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("reset defaults", () =>
            {
                Ruleset.Value = new OsuRuleset().RulesetInfo;

                Beatmap.SetDefault();
                SelectedMods.SetDefault();

                config.SetValue(OsuSetting.SongSelectSortingMode, SortMode.Title);
                config.SetValue(OsuSetting.SongSelectGroupingMode, GroupMode.All);

                songSelect = null!;
                filterOperationsCount = 0;
            });

            AddStep("delete all beatmaps", () => manager.Delete());
        }

        [Test]
        public void TestSingleFilterOnEnter()
        {
            importBeatmapForRuleset(0);
            importBeatmapForRuleset(0);

            loadSongSelect();

            AddAssert("filter count is 0", () => filterOperationsCount, () => Is.EqualTo(0));
        }

        [Test]
        public void TestNoFilterOnSimpleResume()
        {
            importBeatmapForRuleset(0);
            importBeatmapForRuleset(0);

            loadSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            waitForSuspension();

            AddStep("return", () => songSelect.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect.IsCurrentScreen());
            AddAssert("filter count is 0", () => filterOperationsCount, () => Is.EqualTo(0));
        }

        [Test]
        public void TestFilterOnResumeAfterChange()
        {
            importBeatmapForRuleset(0);
            importBeatmapForRuleset(0);

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            loadSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            waitForSuspension();

            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            AddStep("return", () => songSelect.MakeCurrent());
            AddUntilStep("wait for current", () => songSelect.IsCurrentScreen());
            AddAssert("filter count is 1", () => filterOperationsCount, () => Is.EqualTo(1));
        }

        [Test]
        public void TestSorting()
        {
            loadSongSelect();
            addManyTestMaps();

            // TODO: old test has this step, but there doesn't seem to be any purpose for it.
            // AddUntilStep("random map selected", () => Beatmap.Value != defaultBeatmap);

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
        public void TestCutInFilterTextBox()
        {
            loadSongSelect();

            AddStep("set filter text", () => filterTextBox.Current.Value = "nonono");
            AddStep("select all", () => InputManager.Keys(PlatformAction.SelectAll));
            AddStep("press ctrl/cmd-x", () => InputManager.Keys(PlatformAction.Cut));

            AddAssert("filter text cleared", () => filterTextBox.Current.Value, () => Is.Empty);
        }

        [Test]
        public void TestNonFilterableModChange()
        {
            importBeatmapForRuleset(0);

            loadSongSelect();

            // Mod that is guaranteed to never re-filter.
            AddStep("add non-filterable mod", () => SelectedMods.Value = new Mod[] { new OsuModCinema() });
            AddAssert("filter count is 0", () => filterOperationsCount, () => Is.EqualTo(0));

            // Removing the mod should still not re-filter.
            AddStep("remove non-filterable mod", () => SelectedMods.Value = Array.Empty<Mod>());
            AddAssert("filter count is 0", () => filterOperationsCount, () => Is.EqualTo(0));
        }

        [Test]
        public void TestFilterableModChange()
        {
            importBeatmapForRuleset(3);

            loadSongSelect();

            // Change to mania ruleset.
            AddStep("filter to mania ruleset", () => Ruleset.Value = rulesets.AvailableRulesets.First(r => r.OnlineID == 3));
            AddAssert("filter count is 1", () => filterOperationsCount, () => Is.EqualTo(1));

            // Apply a mod, but this should NOT re-filter because there's no search text.
            AddStep("add filterable mod", () => SelectedMods.Value = new Mod[] { new ManiaModKey3() });
            AddAssert("filter count is 1", () => filterOperationsCount, () => Is.EqualTo(1));

            // Set search text. Should re-filter.
            AddStep("set search text to match mods", () => filterTextBox.Current.Value = "keys=3");
            AddAssert("filter count is 2", () => filterOperationsCount, () => Is.EqualTo(2));

            // Change filterable mod. Should re-filter.
            AddStep("change new filterable mod", () => SelectedMods.Value = new Mod[] { new ManiaModKey5() });
            AddAssert("filter count is 3", () => filterOperationsCount, () => Is.EqualTo(3));

            // Add non-filterable mod. Should NOT re-filter.
            AddStep("apply non-filterable mod", () => SelectedMods.Value = new Mod[] { new ManiaModNoFail(), new ManiaModKey5() });
            AddAssert("filter count is 3", () => filterOperationsCount, () => Is.EqualTo(3));

            // Remove filterable mod. Should re-filter.
            AddStep("remove filterable mod", () => SelectedMods.Value = new Mod[] { new ManiaModNoFail() });
            AddAssert("filter count is 4", () => filterOperationsCount, () => Is.EqualTo(4));

            // Remove non-filterable mod. Should NOT re-filter.
            AddStep("remove non-filterable mod", () => SelectedMods.Value = Array.Empty<Mod>());
            AddAssert("filter count is 4", () => filterOperationsCount, () => Is.EqualTo(4));

            // Add filterable mod. Should re-filter.
            AddStep("add filterable mod", () => SelectedMods.Value = new Mod[] { new ManiaModKey3() });
            AddAssert("filter count is 5", () => filterOperationsCount, () => Is.EqualTo(5));
        }

        // This test should probably not be in this test class, it has nothing to do with filtering.
        // TestSceneSongSelect is a better place, but doesn't have local storage isolation setup (yet).
        [Test]
        public void TestDeleteHotkey()
        {
            loadSongSelect();

            importBeatmapForRuleset(0);

            AddAssert("beatmap imported", () => manager.GetAllUsableBeatmapSets().Any(), () => Is.True);

            // song select should automatically select the beatmap for us but this is not implemented yet.
            // todo: remove when that's the case.
            AddAssert("no beatmap selected", () => Beatmap.IsDefault);
            AddStep("select beatmap", () => Beatmap.Value = manager.GetWorkingBeatmap(manager.GetAllUsableBeatmapSets().Single().Beatmaps.First()));
            AddAssert("beatmap selected", () => !Beatmap.IsDefault);

            AddStep("press shift-delete", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Delete);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            AddUntilStep("delete dialog shown", () => DialogOverlay.CurrentDialog, Is.InstanceOf<BeatmapDeleteDialog>);
            AddStep("confirm deletion", () => DialogOverlay.CurrentDialog!.PerformAction<PopupDialogDangerousButton>());

            AddAssert("beatmap set deleted", () => manager.GetAllUsableBeatmapSets().Any(), () => Is.False);
        }

        [Test]
        public void TestPlaceholderVisibleAfterDeleteAll()
        {
            loadSongSelect();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            importBeatmapForRuleset(0);
            AddUntilStep("wait for placeholder hidden", () => getPlaceholder()?.State.Value == Visibility.Hidden);

            AddStep("delete all beatmaps", () => manager.Delete());
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestPlaceholderVisibleAfterStarDifficultyFilter()
        {
            importBeatmapForRuleset(0);
            AddStep("change star filter", () => config.SetValue(OsuSetting.DisplayStarsMinimum, 10.0));

            loadSongSelect();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddStep("click link in placeholder", () => getPlaceholder().ChildrenOfType<DrawableLinkCompiler>().First().TriggerClick());

            AddUntilStep("star filter reset", () => config.Get<double>(OsuSetting.DisplayStarsMinimum) == 0.0);
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestPlaceholderVisibleWithConvertSetting()
        {
            importBeatmapForRuleset(0);
            AddStep("change convert setting", () => config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            loadSongSelect();

            changeRuleset(2);

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddStep("click link in placeholder", () => getPlaceholder().ChildrenOfType<DrawableLinkCompiler>().First().TriggerClick());

            AddUntilStep("convert setting changed", () => config.Get<bool>(OsuSetting.ShowConvertedBeatmaps));
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestCorrectMatchCountAfterDeleteAll()
        {
            loadSongSelect();
            checkMatchedBeatmaps(0);

            importBeatmapForRuleset(0);
            checkMatchedBeatmaps(3);

            AddStep("delete all beatmaps", () => manager.Delete());
            checkMatchedBeatmaps(0);
        }

        [Test]
        public void TestCorrectMatchCountAfterHardDelete()
        {
            loadSongSelect();
            checkMatchedBeatmaps(0);

            importBeatmapForRuleset(0);
            checkMatchedBeatmaps(3);

            AddStep("hard delete beatmap", () => Realm.Write(r => r.RemoveRange(r.All<BeatmapSetInfo>().Where(s => !s.Protected))));
            checkMatchedBeatmaps(0);
        }

        private void loadSongSelect()
        {
            AddStep("load screen", () => Stack.Push(songSelect = new SoloSongSelect()));
            AddUntilStep("wait for load", () => Stack.CurrentScreen == songSelect && songSelect.IsLoaded);
            AddStep("hook events", () =>
            {
                filterOperationsCount = 0;
                filter.CriteriaChanged += _ => filterOperationsCount++;
            });
        }

        private NoResultsPlaceholder? getPlaceholder() => songSelect.ChildrenOfType<NoResultsPlaceholder>().FirstOrDefault();

        private void importBeatmapForRuleset(int rulesetId)
        {
            int beatmapsCount = 0;

            AddStep($"import test map for ruleset {rulesetId}", () =>
            {
                beatmapsCount = songSelect.IsNull() ? 0 : carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count;
                manager.Import(TestResources.CreateTestBeatmapSetInfo(3, rulesets.AvailableRulesets.Where(r => r.OnlineID == rulesetId).ToArray()));
            });

            // This is specifically for cases where the add is happening post song select load.
            // For cases where song select is null, the assertions are provided by the load checks.
            AddUntilStep("wait for imported to arrive in carousel", () => songSelect.IsNull() || carousel.Filters.OfType<BeatmapCarouselFilterGrouping>().Single().SetItems.Count > beatmapsCount);
        }

        private void changeRuleset(int rulesetId)
        {
            AddStep($"change ruleset to {rulesetId}", () => Ruleset.Value = rulesets.AvailableRulesets.First(r => r.OnlineID == rulesetId));
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

        private void checkMatchedBeatmaps(int expected) =>
            AddUntilStep($"{expected} matching shown", () => carousel.MatchedBeatmapsCount, () => Is.EqualTo(expected));

        private void waitForSuspension() => AddUntilStep("wait for not current", () => !songSelect.AsNonNull().IsCurrentScreen());

        private void updateFooter(IScreen? _, IScreen? newScreen)
        {
            if (newScreen is IOsuScreen osuScreen && osuScreen.ShowFooter)
            {
                screenFooter.Show();
                screenFooter.SetButtons(osuScreen.CreateFooterButtons());
            }
            else
            {
                screenFooter.Hide();
                screenFooter.SetButtons(Array.Empty<ScreenFooterButton>());
            }
        }
    }
}
