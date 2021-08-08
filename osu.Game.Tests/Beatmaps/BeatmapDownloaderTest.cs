// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    [HeadlessTest]
    public class BeatmapDownloaderTest : OsuTestScene
    {
        [Resolved]
        private BeatmapDownloader downloader { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        private LinkedList<BeatmapSetInfo> rulesetList = new LinkedList<BeatmapSetInfo>();

        public BeatmapDownloaderTest()
        {
            rulesetList.AddLast(new BeatmapSetInfo()
            {
                OnlineBeatmapSetID = 3756,
                Beatmaps = new List<BeatmapInfo>
                {
                    new BeatmapInfo()
                    {
                        StarDifficulty = 0.67
                    },
                },
                OnlineInfo = new BeatmapSetOnlineInfo()
                {
                    LastUpdated = new DateTime(2008, 11, 4),
                    Ranked = null,
                },
            });

            rulesetList.AddLast(new BeatmapSetInfo()
            {
                OnlineBeatmapSetID = 1,
                Beatmaps = new List<BeatmapInfo>
                {
                    new BeatmapInfo()
                    {
                        StarDifficulty = 2.40
                    },
                },
                OnlineInfo = new BeatmapSetOnlineInfo()
                {
                    LastUpdated = new DateTime(2007, 10, 6),
                    Ranked = new DateTimeOffset(new DateTime(2007, 10, 6)),
                },
            });

            rulesetList.AddLast(new BeatmapSetInfo()
            {
                OnlineBeatmapSetID = 1,
                Beatmaps = new List<BeatmapInfo>
                {
                    new BeatmapInfo()
                    {
                        StarDifficulty = 4.14
                    },
                },
                OnlineInfo = new BeatmapSetOnlineInfo()
                {
                    LastUpdated = new DateTime(2008, 8, 10),
                    Ranked = new DateTimeOffset(new DateTime(2008, 8, 16)),
                },
            });
        }

        [Test]
        public void TestMatchingCriteriaSuccess()
        {
            config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = new DateTime(2007, 10, 1);
            config.GetBindable<double>(OsuSetting.BeatmapDownloadMinimumStarRating).Value = 0.0;
            config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset).Value = 0;
            config.GetBindable<Overlays.BeatmapListing.SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory).Value = Overlays.BeatmapListing.SearchCategory.Leaderboard;

            foreach (var beatmapsetinfo in rulesetList)
            {
                Assert.True(downloader.MatchesDownloadCriteria(beatmapsetinfo));
            }
        }

        [Test]
        public void TestMatchingCriteriaFail()
        {
            config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = new DateTime(2020, 10, 1);
            config.GetBindable<double>(OsuSetting.BeatmapDownloadMinimumStarRating).Value = 0.0;
            config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset).Value = 0;
            config.GetBindable<Overlays.BeatmapListing.SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory).Value = Overlays.BeatmapListing.SearchCategory.Leaderboard;

            foreach (var beatmapsetinfo in rulesetList)
            {
                Assert.False(downloader.MatchesDownloadCriteria(beatmapsetinfo));
            }
        }
    }
}
