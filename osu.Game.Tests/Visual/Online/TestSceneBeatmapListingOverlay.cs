// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapListingOverlay : OsuTestScene
    {
        private readonly List<APIBeatmapSet> setsForResponse = new List<APIBeatmapSet>();

        private BeatmapListingOverlay overlay;

        private BeatmapListingSearchControl searchControl => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single();

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = overlay = new BeatmapListingOverlay { State = { Value = Visibility.Visible } };

            ((DummyAPIAccess)API).HandleRequest = req =>
            {
                if (!(req is SearchBeatmapSetsRequest searchBeatmapSetsRequest)) return false;

                searchBeatmapSetsRequest.TriggerSuccess(new SearchBeatmapSetsResponse
                {
                    BeatmapSets = setsForResponse,
                });

                return true;
            };

            AddStep("initialize dummy", () =>
            {
                // non-supporter user
                ((DummyAPIAccess)API).LocalUser.Value = new User
                {
                    Username = "TestBot",
                    Id = API.LocalUser.Value.Id + 1,
                };
            });
        }

        [Test]
        public void TestNoBeatmapsPlaceholder()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            AddStep("fetch for 1 beatmap", () => fetchFor(CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet));
            AddUntilStep("placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            // fetch once more to ensure nothing happens in displaying placeholder again when it already is present.
            AddStep("fetch for 0 beatmaps again", () => fetchFor());
            AddUntilStep("placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        [Test]
        public void TestNonSupportUseSupporterOnlyFiltersPlaceholderNoBeatmaps()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // test non-supporter on Rank Achieved filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            supporterRequiredPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            notFoundPlaceholderShown();

            // test non-supporter on Played filter
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            notFoundPlaceholderShown();

            // test non-supporter on both Rank Achieved and Played filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            notFoundPlaceholderShown();
        }

        [Test]
        public void TestSupportUseSupporterOnlyFiltersPlaceholderNoBeatmaps()
        {
            AddStep("fetch for 0 beatmaps", () => fetchFor());
            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // test supporter on Rank Achieved filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            notFoundPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            notFoundPlaceholderShown();

            // test supporter on Played filter
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            notFoundPlaceholderShown();

            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            notFoundPlaceholderShown();

            // test supporter on both Rank Achieved and Played filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            notFoundPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            notFoundPlaceholderShown();
        }

        [Test]
        public void TestNonSupporterUseSupporterOnlyFiltersPlaceholderOneBeatmap()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet));
            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // test non-supporter on Rank Achieved filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            supporterRequiredPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            noPlaceholderShown();

            // test non-supporter on Played filter
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            noPlaceholderShown();

            // test non-supporter on both Rank Achieved and Played filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            supporterRequiredPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            noPlaceholderShown();
        }

        [Test]
        public void TestSupporterUseSupporterOnlyFiltersPlaceholderOneBeatmap()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet));
            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // test supporter on Rank Achieved filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            noPlaceholderShown();

            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            noPlaceholderShown();

            // test supporter on Played filter
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            noPlaceholderShown();

            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            noPlaceholderShown();

            // test supporter on both Rank Achieved and Played filter
            toggleRankFilter(Scoring.ScoreRank.XH);
            toggleSupporterOnlyPlayedFilter(SearchPlayed.Played);
            noPlaceholderShown();

            AddStep("Set Played filter to Any", () => searchControl.Played.Value = SearchPlayed.Any);
            AddStep("Clear Rank Achieved filter", () => searchControl.Ranks.Clear());
            noPlaceholderShown();
        }

        private void fetchFor(params BeatmapSetInfo[] beatmaps)
        {
            setsForResponse.Clear();
            setsForResponse.AddRange(beatmaps.Select(b => new TestAPIBeatmapSet(b)));

            // trigger arbitrary change for fetching.
            searchControl.Query.TriggerChange();
        }

        private void toggleRankFilter(Scoring.ScoreRank rank)
        {
            AddStep("toggle Rank Achieved filter", () =>
            {
                searchControl.Ranks.Clear();
                searchControl.Ranks.Add(rank);
            });
        }

        private void toggleSupporterOnlyPlayedFilter(SearchPlayed played)
        {
            AddStep("toggle Played filter", () => searchControl.Played.Value = played);
        }

        private void supporterRequiredPlaceholderShown()
        {
            AddUntilStep("supporter-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        private void notFoundPlaceholderShown()
        {
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        private void noPlaceholderShown()
        {
            AddUntilStep("no placeholder shown", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any() && !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());
        }

        private class TestAPIBeatmapSet : APIBeatmapSet
        {
            private readonly BeatmapSetInfo beatmapSet;

            public TestAPIBeatmapSet(BeatmapSetInfo beatmapSet)
            {
                this.beatmapSet = beatmapSet;
            }

            public override BeatmapSetInfo ToBeatmapSet(RulesetStore rulesets) => beatmapSet;
        }
    }
}
