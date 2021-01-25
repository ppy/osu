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
                if (req is SearchBeatmapSetsRequest searchBeatmapSetsRequest)
                {
                    searchBeatmapSetsRequest.TriggerSuccess(new SearchBeatmapSetsResponse
                    {
                        BeatmapSets = setsForResponse,
                    });
                }
            };
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

        private void fetchFor(params BeatmapSetInfo[] beatmaps)
        {
            setsForResponse.Clear();
            setsForResponse.AddRange(beatmaps.Select(b => new TestAPIBeatmapSet(b)));

            // trigger arbitrary change for fetching.
            overlay.ChildrenOfType<BeatmapListingSearchControl>().Single().Query.TriggerChange();
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
