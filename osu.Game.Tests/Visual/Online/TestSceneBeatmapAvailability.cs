// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneBeatmapAvailability : OsuTestScene
    {
        private readonly BeatmapAvailability container;

        public TestSceneBeatmapAvailability()
        {
            Add(container = new BeatmapAvailability());
        }

        [Test]
        public void TestUndownloadableWithLink()
        {
            AddStep("set undownloadable beatmapset with link", () => container.BeatmapSet = new APIBeatmapSet
            {
                Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = true,
                    ExternalLink = @"https://osu.ppy.sh",
                },
            });

            visibilityAssert(true);
        }

        [Test]
        public void TestUndownloadableNoLink()
        {
            AddStep("set undownloadable beatmapset without link", () => container.BeatmapSet = new APIBeatmapSet
            {
                Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = true,
                },
            });

            visibilityAssert(true);
        }

        [Test]
        public void TestPartsRemovedWithLink()
        {
            AddStep("set parts-removed beatmapset with link", () => container.BeatmapSet = new APIBeatmapSet
            {
                Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = false,
                    ExternalLink = @"https://osu.ppy.sh",
                },
            });

            visibilityAssert(true);
        }

        [Test]
        public void TestNormal()
        {
            AddStep("set normal beatmapset", () => container.BeatmapSet = new APIBeatmapSet
            {
                Availability = new BeatmapSetOnlineAvailability
                {
                    DownloadDisabled = false,
                },
            });

            visibilityAssert(false);
        }

        private void visibilityAssert(bool shown)
        {
            AddAssert($"is container {(shown ? "visible" : "hidden")}", () => container.Alpha == (shown ? 1 : 0));
        }
    }
}
