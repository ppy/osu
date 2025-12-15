// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Select.Filter;
using osu.Game.Screens.SelectV2;
using FilterControl = osu.Game.Screens.SelectV2.FilterControl;
using NoResultsPlaceholder = osu.Game.Screens.SelectV2.NoResultsPlaceholder;

namespace osu.Game.Tests.Visual.SongSelectV2
{
    public partial class TestSceneSongSelectFiltering : SongSelectTestScene
    {
        private FilterControl filter => SongSelect.ChildrenOfType<FilterControl>().Single();
        private ShearedFilterTextBox filterTextBox => SongSelect.ChildrenOfType<ShearedFilterTextBox>().Single();
        private int filterOperationsCount;

        protected override void LoadSongSelect()
        {
            base.LoadSongSelect();

            AddStep("hook filter event", () =>
            {
                filterOperationsCount = 0;
                filter.CriteriaChanged += _ => filterOperationsCount++;
            });
        }

        [Test]
        public void TestSingleFilterOnEnter()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

            AddAssert("filter count is 0", () => filterOperationsCount, () => Is.EqualTo(0));
        }

        [Test]
        public void TestNoFilterOnSimpleResume()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            WaitForSuspension();

            AddStep("return", () => SongSelect.MakeCurrent());
            AddUntilStep("wait for current", () => SongSelect.IsCurrentScreen());
            AddAssert("filter count is 0", () => filterOperationsCount, () => Is.EqualTo(0));
        }

        [Test]
        public void TestFilterSingleResult_RetainsSelectedDifficulty()
        {
            LoadSongSelect();

            ImportBeatmapForRuleset(0);

            AddUntilStep("wait for single set", () => Carousel.ChildrenOfType<PanelBeatmapSet>().Count(), () => Is.EqualTo(1));

            AddStep("select last difficulty", () =>
            {
                Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmaps.GetAllUsableBeatmapSets().First().Beatmaps.Last());
            });

            AddStep("set filter text", () => filterTextBox.Current.Value = " ");

            AddWaitStep("wait for debounce", 5);
            AddUntilStep("wait for filter", () => !Carousel.IsFiltering);

            AddAssert("selection unchanged", () => Beatmap.Value.BeatmapInfo, () => Is.EqualTo(Beatmaps.GetAllUsableBeatmapSets().First().Beatmaps.Last()));
        }

        [Test]
        public void TestFilterSingleResult_ReselectedAfterRulesetSwitches()
        {
            LoadSongSelect();

            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            AddStep("disable converts", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));
            AddStep("set filter text", () => filterTextBox.Current.Value = $"\"{Beatmaps.GetAllUsableBeatmapSets().Last().Metadata.Title}\"");

            AddWaitStep("wait for debounce", 5);
            AddUntilStep("wait for filter", () => !Carousel.IsFiltering);
            AddUntilStep("selection is second beatmap set", () => Beatmap.Value.BeatmapInfo, () => Is.EqualTo(Beatmaps.GetAllUsableBeatmapSets().Last().Beatmaps.First()));

            AddStep("select last difficulty", () => Beatmap.Value = Beatmaps.GetWorkingBeatmap(Beatmap.Value.BeatmapSetInfo.Beatmaps.Last()));
            AddUntilStep("selection is last difficulty of second beatmap set", () => Beatmap.Value.BeatmapInfo, () => Is.EqualTo(Beatmaps.GetAllUsableBeatmapSets().Last().Beatmaps.Last()));

            ChangeRuleset(1);
            AddUntilStep("wait for filter", () => !Carousel.IsFiltering);
            AddUntilStep("selection is default", () => Beatmap.IsDefault);

            ChangeRuleset(0);
            AddUntilStep("wait for filter", () => !Carousel.IsFiltering);
            AddUntilStep("selection is last difficulty of second beatmap set", () => Beatmap.Value.BeatmapInfo, () => Is.EqualTo(Beatmaps.GetAllUsableBeatmapSets().Last().Beatmaps.Last()));
        }

        [Test]
        public void TestFilterOnResumeAfterChange()
        {
            ImportBeatmapForRuleset(0);
            ImportBeatmapForRuleset(0);

            AddStep("change convert setting", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            LoadSongSelect();

            AddStep("push child screen", () => Stack.Push(new TestSceneOsuScreenStack.TestScreen("test child")));
            WaitForSuspension();

            AddStep("change convert setting", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            AddStep("return", () => SongSelect.MakeCurrent());
            AddUntilStep("wait for current", () => SongSelect.IsCurrentScreen());
            AddAssert("filter count is 1", () => filterOperationsCount, () => Is.EqualTo(1));
        }

        [Test]
        public void TestSorting()
        {
            LoadSongSelect();
            AddManyTestMaps();

            // TODO: old test has this step, but there doesn't seem to be any purpose for it.
            // AddUntilStep("random map selected", () => Beatmap.Value != defaultBeatmap);

            SortBy(SortMode.Artist);
            SortBy(SortMode.Title);
            SortBy(SortMode.Author);
            SortBy(SortMode.DateAdded);
            SortBy(SortMode.BPM);
            SortBy(SortMode.Length);
            SortBy(SortMode.Difficulty);
            SortBy(SortMode.Source);
        }

        [Test]
        public void TestCutInFilterTextBox()
        {
            LoadSongSelect();

            AddStep("set filter text", () => filterTextBox.Current.Value = "nonono");
            AddStep("select all", () => InputManager.Keys(PlatformAction.SelectAll));
            AddStep("press ctrl/cmd-x", () => InputManager.Keys(PlatformAction.Cut));

            AddAssert("filter text cleared", () => filterTextBox.Current.Value, () => Is.Empty);
        }

        [Test]
        public void TestNonFilterableModChange()
        {
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

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
            ImportBeatmapForRuleset(3);

            LoadSongSelect();

            // Change to mania ruleset.
            AddStep("filter to mania ruleset", () => Ruleset.Value = Rulesets.AvailableRulesets.First(r => r.OnlineID == 3));
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

        [Test]
        public void TestPlaceholderVisibleAfterDeleteAll()
        {
            LoadSongSelect();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            ImportBeatmapForRuleset(0);
            AddUntilStep("wait for placeholder hidden", () => getPlaceholder()?.State.Value == Visibility.Hidden);

            AddStep("delete all beatmaps", () => Beatmaps.Delete());
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);
        }

        [Test]
        public void TestPlaceholderVisibleAfterStarDifficultyFilter()
        {
            ImportBeatmapForRuleset(0);
            AddStep("change star filter", () => Config.SetValue(OsuSetting.DisplayStarsMinimum, 10.0));

            LoadSongSelect();

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddStep("click link in placeholder", () => getPlaceholder().ChildrenOfType<DrawableLinkCompiler>().First().TriggerClick());

            AddUntilStep("star filter reset", () => Config.Get<double>(OsuSetting.DisplayStarsMinimum) == 0.0);
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestSelectionRetainedWhenFilteringAllPanelsAway()
        {
            ImportBeatmapForRuleset(0);

            LoadSongSelect();

            AddAssert("has selection", () => Beatmap.IsDefault, () => Is.False);

            AddStep("change star filter", () => Config.SetValue(OsuSetting.DisplayStarsMinimum, 10.0));
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddAssert("still has selection", () => Beatmap.IsDefault, () => Is.False);
        }

        [Test]
        public void TestPlaceholderVisibleWithConvertSetting()
        {
            ImportBeatmapForRuleset(0);
            AddStep("change convert setting", () => Config.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            LoadSongSelect();

            ChangeRuleset(2);

            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Visible);

            AddStep("click link in placeholder", () => getPlaceholder().ChildrenOfType<DrawableLinkCompiler>().First().TriggerClick());

            AddUntilStep("convert setting changed", () => Config.Get<bool>(OsuSetting.ShowConvertedBeatmaps));
            AddUntilStep("wait for placeholder visible", () => getPlaceholder()?.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestCorrectMatchCountAfterDeleteAll()
        {
            LoadSongSelect();
            checkMatchedBeatmaps(0);

            ImportBeatmapForRuleset(0);
            checkMatchedBeatmaps(3);

            AddStep("delete all beatmaps", () => Beatmaps.Delete());
            checkMatchedBeatmaps(0);
        }

        [Test]
        public void TestCorrectMatchCountAfterHardDelete()
        {
            LoadSongSelect();
            checkMatchedBeatmaps(0);

            ImportBeatmapForRuleset(0);
            checkMatchedBeatmaps(3);

            AddStep("hard delete beatmap", () => Realm.Write(r => r.RemoveRange(r.All<BeatmapSetInfo>().Where(s => !s.Protected))));
            checkMatchedBeatmaps(0);
        }

        [Test]
        public void TestHideBeatmap()
        {
            BeatmapInfo? hiddenBeatmap = null;

            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            checkMatchedBeatmaps(3);

            AddStep("hide selected", () =>
            {
                hiddenBeatmap = Beatmap.Value.BeatmapInfo;
                Beatmaps.Hide(hiddenBeatmap);
            });

            checkMatchedBeatmaps(2);

            AddUntilStep("selection changed", () => Beatmap.Value.BeatmapInfo, () => Is.Not.EqualTo(hiddenBeatmap));

            AddStep("restore", () => Beatmaps.Restore(hiddenBeatmap!));

            checkMatchedBeatmaps(3);
        }

        [Test]
        public void TestCantHideAllBeatmaps()
        {
            LoadSongSelect();
            ImportBeatmapForRuleset(0);

            checkMatchedBeatmaps(3);

            AddStep("hide selected", () => Beatmaps.Hide(Beatmap.Value.BeatmapInfo));
            checkMatchedBeatmaps(2);

            AddStep("hide selected", () => Beatmaps.Hide(Beatmap.Value.BeatmapInfo));
            checkMatchedBeatmaps(1);

            AddAssert("hide fails", () => Beatmaps.Hide(Beatmap.Value.BeatmapInfo), () => Is.False);
            checkMatchedBeatmaps(1);
        }

        private NoResultsPlaceholder? getPlaceholder() => SongSelect.ChildrenOfType<NoResultsPlaceholder>().FirstOrDefault();

        private void checkMatchedBeatmaps(int expected) => AddUntilStep($"{expected} matching shown", () => Carousel.MatchedBeatmapsCount, () => Is.EqualTo(expected));
    }
}
