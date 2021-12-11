// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Scoring;
using osuTK.Input;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapListingOverlay : OsuManualInputManagerTestScene
    {
        private readonly List<APIBeatmapSet> setsForResponse = new List<APIBeatmapSet>();

        private BeatmapListingOverlay overlay;

        private BeatmapListingSearchControl searchControl => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single();

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("setup overlay", () =>
            {
                Child = overlay = new BeatmapListingOverlay { State = { Value = Visibility.Visible } };
                setsForResponse.Clear();
            });

            AddStep("initialize dummy", () =>
            {
                var api = (DummyAPIAccess)API;

                api.HandleRequest = req =>
                {
                    if (!(req is SearchBeatmapSetsRequest searchBeatmapSetsRequest)) return false;

                    searchBeatmapSetsRequest.TriggerSuccess(new SearchBeatmapSetsResponse
                    {
                        BeatmapSets = setsForResponse,
                    });

                    return true;
                };

                // non-supporter user
                api.LocalUser.Value = new APIUser
                {
                    Username = "TestBot",
                    Id = API.LocalUser.Value.Id + 1,
                };
            });
        }

        [Test]
        public void TestHideViaBack()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);
            AddStep("hide", () => InputManager.Key(Key.Escape));
            AddUntilStep("is hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestHideViaBackWithSearch()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("search something", () => overlay.ChildrenOfType<SearchTextBox>().First().Text = "search");

            AddStep("kill search", () => InputManager.Key(Key.Escape));

            AddAssert("search textbox empty", () => string.IsNullOrEmpty(overlay.ChildrenOfType<SearchTextBox>().First().Text));
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("hide", () => InputManager.Key(Key.Escape));
            AddUntilStep("is hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestHideViaBackWithScrolledSearch()
        {
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("show many results", () => fetchFor(Enumerable.Repeat(CreateAPIBeatmapSet(Ruleset.Value), 100).ToArray()));

            AddUntilStep("placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any(d => d.IsPresent));

            AddStep("scroll to bottom", () => overlay.ChildrenOfType<OverlayScrollContainer>().First().ScrollToEnd());

            AddStep("kill search", () => InputManager.Key(Key.Escape));

            AddUntilStep("search textbox empty", () => string.IsNullOrEmpty(overlay.ChildrenOfType<SearchTextBox>().First().Text));
            AddUntilStep("is scrolled to top", () => overlay.ChildrenOfType<OverlayScrollContainer>().First().Current == 0);
            AddAssert("is visible", () => overlay.State.Value == Visibility.Visible);

            AddStep("hide", () => InputManager.Key(Key.Escape));
            AddUntilStep("is hidden", () => overlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestNoBeatmapsPlaceholder()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            AddStep("fetch for 1 beatmap", () => fetchFor(CreateAPIBeatmapSet(Ruleset.Value)));
            AddUntilStep("placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any(d => d.IsPresent));

            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            // fetch once more to ensure nothing happens in displaying placeholder again when it already is present.
            AddStep("fetch for 0 beatmaps again", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        [Test]
        public void TestUserWithoutSupporterUsesSupporterOnlyFiltersWithoutResults()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            notFoundPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();

            // both RankAchieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();
        }

        [Test]
        public void TestUserWithSupporterUsesSupporterOnlyFiltersWithoutResults()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            notFoundPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            notFoundPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            notFoundPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();

            // both Rank Achieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            notFoundPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            notFoundPlaceholderShown();
        }

        [Test]
        public void TestUserWithoutSupporterUsesSupporterOnlyFiltersWithResults()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateAPIBeatmapSet(Ruleset.Value)));
            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            noPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();

            // both Rank Achieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();
        }

        [Test]
        public void TestUserWithSupporterUsesSupporterOnlyFiltersWithResults()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateAPIBeatmapSet(Ruleset.Value)));
            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // only Rank Achieved filter
            setRankAchievedFilter(new[] { ScoreRank.XH });
            noPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            noPlaceholderShown();

            // only Played filter
            setPlayedFilter(SearchPlayed.Played);
            noPlaceholderShown();

            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();

            // both Rank Achieved and Played filters
            setRankAchievedFilter(new[] { ScoreRank.XH });
            setPlayedFilter(SearchPlayed.Played);
            noPlaceholderShown();

            setRankAchievedFilter(Array.Empty<ScoreRank>());
            setPlayedFilter(SearchPlayed.Any);
            noPlaceholderShown();
        }

        private static int searchCount;

        private void fetchFor(params APIBeatmapSet[] beatmaps)
        {
            setsForResponse.Clear();
            setsForResponse.AddRange(beatmaps);

            // trigger arbitrary change for fetching.
            searchControl.Query.Value = $"search {searchCount++}";
        }

        private void setRankAchievedFilter(ScoreRank[] ranks)
        {
            AddStep($"set Rank Achieved filter to [{string.Join(',', ranks)}]", () =>
            {
                searchControl.Ranks.Clear();
                searchControl.Ranks.AddRange(ranks);
            });
        }

        private void setPlayedFilter(SearchPlayed played)
        {
            AddStep($"set Played filter to {played}", () => searchControl.Played.Value = played);
        }

        private void supporterRequiredPlaceholderShown()
        {
            AddUntilStep("\"supporter required\" placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        private void notFoundPlaceholderShown()
        {
            AddUntilStep("\"no maps found\" placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        private void noPlaceholderShown()
        {
            AddUntilStep("no placeholder shown", () =>
                !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any(d => d.IsPresent)
                && !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any(d => d.IsPresent));
        }
    }
}
