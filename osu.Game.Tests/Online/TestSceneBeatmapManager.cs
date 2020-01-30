// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    [HeadlessTest]
    public class TestSceneBeatmapManager : OsuTestScene
    {
        private BeatmapManager beatmaps;
        private ProgressNotification recentNotification;

        private static readonly BeatmapSetInfo test_model = new BeatmapSetInfo { OnlineBeatmapSetID = 1 };

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;

            beatmaps.PostNotification = n => recentNotification = n as ProgressNotification;
        }

        [Test]
        public void TestCancelDownloadFromRequest()
        {
            AddStep("download beatmap", () => beatmaps.Download(test_model));

            AddStep("cancel download from request", () => beatmaps.GetExistingDownload(test_model).Cancel());

            AddUntilStep("is removed from download list", () => beatmaps.GetExistingDownload(test_model) == null);
            AddAssert("is notification cancelled", () => recentNotification.State == ProgressNotificationState.Cancelled);
        }

        [Test]
        public void TestCancelDownloadFromNotification()
        {
            AddStep("download beatmap", () => beatmaps.Download(test_model));

            AddStep("cancel download from notification", () => recentNotification.Close());

            AddUntilStep("is removed from download list", () => beatmaps.GetExistingDownload(test_model) == null);
            AddAssert("is notification cancelled", () => recentNotification.State == ProgressNotificationState.Cancelled);
        }
    }
}
