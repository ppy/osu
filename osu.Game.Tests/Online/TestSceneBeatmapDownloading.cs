// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Online
{
    [HeadlessTest]
    public class TestSceneBeatmapDownloading : OsuTestScene
    {
        private BeatmapModelDownloader beatmaps;
        private ProgressNotification recentNotification;

        private static readonly BeatmapSetInfo test_db_model = new BeatmapSetInfo
        {
            OnlineID = 1,
            Beatmaps =
            {
                new BeatmapInfo
                {
                    Metadata = new BeatmapMetadata
                    {
                        Artist = "test author",
                        Title = "test title",
                        Author = new RealmUser
                        {
                            Username = "mapper"
                        }
                    }
                }
            }
        };

        private static readonly APIBeatmapSet test_online_model = new APIBeatmapSet
        {
            OnlineID = 2,
            Artist = "test author",
            Title = "test title",
            Author = new APIUser
            {
                Username = "mapper"
            }
        };

        [BackgroundDependencyLoader]
        private void load(BeatmapModelDownloader beatmaps)
        {
            this.beatmaps = beatmaps;

            beatmaps.PostNotification = n => recentNotification = n as ProgressNotification;
        }

        private static readonly object[][] notification_test_cases =
        {
            new object[] { test_db_model },
            new object[] { test_online_model }
        };

        [TestCaseSource(nameof(notification_test_cases))]
        public void TestNotificationMessage(IBeatmapSetInfo model)
        {
            AddStep("clear recent notification", () => recentNotification = null);
            AddStep("download beatmap", () => beatmaps.Download(model));

            AddUntilStep("wait for notification", () => recentNotification != null);
            AddUntilStep("notification text correct", () => recentNotification.Text.ToString() == "Downloading test author - test title (mapper)");
        }

        [Test]
        public void TestCancelDownloadFromRequest()
        {
            AddStep("download beatmap", () => beatmaps.Download(test_db_model));

            AddStep("cancel download from request", () => beatmaps.GetExistingDownload(test_db_model).Cancel());

            AddUntilStep("is removed from download list", () => beatmaps.GetExistingDownload(test_db_model) == null);
            AddAssert("is notification cancelled", () => recentNotification.State == ProgressNotificationState.Cancelled);
        }

        [Test]
        public void TestCancelDownloadFromNotification()
        {
            AddStep("download beatmap", () => beatmaps.Download(test_db_model));

            AddStep("cancel download from notification", () => recentNotification.Close());

            AddUntilStep("is removed from download list", () => beatmaps.GetExistingDownload(test_db_model) == null);
            AddAssert("is notification cancelled", () => recentNotification.State == ProgressNotificationState.Cancelled);
        }
    }
}
