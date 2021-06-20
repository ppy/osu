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
        public void TestSupporterOnlyFiltersPlaceholderNoBeatmaps()
        {
            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // test non-supporter on Rank Achieved filter
            toggleRandomRankFilter();
            AddUntilStep("supporter-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("Clear Rank Achieved filter", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Ranks.Clear());
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            // test non-supporter on Played filter
            toggleRandomSupporterOnlyPlayedFilter();
            AddUntilStep("supporter-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("Set Played filter to Any", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Played.Value = SearchPlayed.Any);
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // test supporter on Rank Achieved filter
            toggleRandomRankFilter();
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            AddStep("Clear Rank Achieved filter", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Ranks.Clear());
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            // test supporter on Played filter
            toggleRandomSupporterOnlyPlayedFilter();
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);

            AddStep("Set Played filter to Any", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Played.Value = SearchPlayed.Any);
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().SingleOrDefault()?.IsPresent == true);
        }

        [Test]
        public void TestSupporterOnlyFiltersPlaceholderOneBeatmap()
        {
            AddStep("fetch for 1 beatmap", () => fetchFor(CreateBeatmap(Ruleset.Value).BeatmapInfo.BeatmapSet));
            AddStep("set dummy as non-supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = false);

            // test non-supporter on Rank Achieved filter
            toggleRandomRankFilter();
            AddUntilStep("supporter-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("Clear Rank Achieved filter", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Ranks.Clear());
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            // test non-supporter on Played filter
            toggleRandomSupporterOnlyPlayedFilter();
            AddUntilStep("supporter-placeholder shown", () => overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().SingleOrDefault()?.IsPresent == true);
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("Set Played filter to Any", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Played.Value = SearchPlayed.Any);
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("set dummy as supporter", () => ((DummyAPIAccess)API).LocalUser.Value.IsSupporter = true);

            // test supporter on Rank Achieved filter
            toggleRandomRankFilter();
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("Clear Rank Achieved filter", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Ranks.Clear());
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            // test supporter on Played filter
            toggleRandomSupporterOnlyPlayedFilter();
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());

            AddStep("Set Played filter to Any", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Played.Value = SearchPlayed.Any);
            AddUntilStep("supporter-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.SupporterRequiredDrawable>().Any());
            AddUntilStep("not-found-placeholder hidden", () => !overlay.ChildrenOfType<BeatmapListingOverlay.NotFoundDrawable>().Any());
        }


        private void fetchFor(params BeatmapSetInfo[] beatmaps)
        {
            setsForResponse.Clear();
            setsForResponse.AddRange(beatmaps.Select(b => new TestAPIBeatmapSet(b)));

            // trigger arbitrary change for fetching.
            overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Query.TriggerChange();
        }

        private void toggleRandomRankFilter()
        {
            short r = TestContext.CurrentContext.Random.NextShort();
            AddStep("toggle Random Rank Achieved filter", () =>
            {
                overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Ranks.Clear();
                overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Ranks.Add((Scoring.ScoreRank)(r % 8));
            });
        }

        private void toggleRandomSupporterOnlyPlayedFilter()
        {
            short r = TestContext.CurrentContext.Random.NextShort();
            AddStep("toggle Random Played filter", () => overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Played.Value = (SearchPlayed)(r % 2 + 1));
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
