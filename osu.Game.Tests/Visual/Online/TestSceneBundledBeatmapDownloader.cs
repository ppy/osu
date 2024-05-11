// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Tests.Visual.Online
{
    [Ignore("Only for visual testing")]
    public partial class TestSceneBundledBeatmapDownloader : OsuTestScene
    {
        private BundledBeatmapDownloader downloader;

        [Test]
        public void TestDownloader()
        {
            AddStep("Create downloader", () =>
            {
                downloader?.Expire();
                Add(downloader = new BundledBeatmapDownloader(false));
            });
        }
    }
}
