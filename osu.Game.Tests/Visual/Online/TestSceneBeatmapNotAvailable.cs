// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Overlays.BeatmapSet;

namespace osu.Game.Tests.Visual.Online
{
    public class TestSceneBeatmapNotAvailable : OsuTestScene
    {
        public TestSceneBeatmapNotAvailable()
        {
            var container = new BeatmapNotAvailable();

            Add(container);

            AddAssert("is container hidden", () => container.Alpha == 0);
            AddStep("set undownloadable beatmapset", () => container.BeatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Availability = new BeatmapSetOnlineAvailability
                    {
                        DownloadDisabled = true,
                        ExternalLink = @"https://gist.githubusercontent.com/peppy/99e6959772083cdfde8a/raw",
                    },
                },
            });

            AddAssert("is container visible", () => container.Alpha == 1);
            AddStep("set downloadable beatmapset", () => container.BeatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Availability = new BeatmapSetOnlineAvailability
                    {
                        DownloadDisabled = false,
                        ExternalLink = @"https://gist.githubusercontent.com/peppy/99e6959772083cdfde8a/raw",
                    },
                },
            });

            AddAssert("is container still visible", () => container.Alpha == 1);
            AddStep("set normal beatmapset", () => container.BeatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo(),
            });

            AddAssert("is container hidden", () => container.Alpha == 0);
        }
    }
}
