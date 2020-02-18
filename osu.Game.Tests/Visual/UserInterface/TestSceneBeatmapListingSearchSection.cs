// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneBeatmapListingSearchSection : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(BeatmapListingSearchSection),
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        public TestSceneBeatmapListingSearchSection()
        {
            BeatmapListingSearchSection section;

            Add(section = new BeatmapListingSearchSection
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });

            var beatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = new BeatmapSetOnlineCovers
                    {
                        Cover = "https://assets.ppy.sh/beatmaps/1094296/covers/cover@2x.jpg?1581416305"
                    }
                }
            };

            var noCoverBeatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = new BeatmapSetOnlineCovers
                    {
                        Cover = string.Empty
                    }
                }
            };

            AddStep("Set beatmap", () => section.BeatmapSet = beatmapSet);
            AddStep("Set beatmap (no cover)", () => section.BeatmapSet = noCoverBeatmapSet);
            AddStep("Set null beatmap", () => section.BeatmapSet = null);
        }
    }
}
