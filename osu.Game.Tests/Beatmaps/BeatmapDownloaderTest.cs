// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Rulesets;
using osu.Game.Tests.Visual;
using static osu.Game.Overlays.BeatmapListing.BeatmapListingFilterControl;

namespace osu.Game.Tests.Beatmaps
{
    [TestFixture]
    [HeadlessTest]
    public class BeatmapDownloaderTest : OsuTestScene
    {
        private TestBeatmapDownloader downloader { get; set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

        private static List<BeatmapSetInfo> rulesetList = new List<BeatmapSetInfo>() {
            new BeatmapSetInfo()
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
            },
            new BeatmapSetInfo()
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
            },
            new BeatmapSetInfo()
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
            },
        };

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, BeatmapManager beatmapManager, RulesetStore rulesets)
        {
            Dependencies.CacheAs<BeatmapDownloader>(downloader = new TestBeatmapDownloader(config, beatmapManager, API, rulesets));
        }

        public BeatmapDownloaderTest()
        {
        }

        [Test]
        public void TestMatchingCriteriaSuccess()
        {
            config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = new DateTime(2007, 10, 1);
            config.GetBindable<double>(OsuSetting.BeatmapDownloadMinimumStarRating).Value = 0.0;
            config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset).Value = 0;
            config.GetBindable<SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory).Value = SearchCategory.Leaderboard;

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
            config.GetBindable<SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory).Value = SearchCategory.Leaderboard;

            foreach (var beatmapsetinfo in rulesetList)
            {
                Assert.False(downloader.MatchesDownloadCriteria(beatmapsetinfo));
            }
        }

        [Test]
        public void TestDownloadFunctionSuccess()
        {
            config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = new DateTime(2008, 1, 1);
            config.GetBindable<double>(OsuSetting.BeatmapDownloadMinimumStarRating).Value = 7.0;
            config.GetBindable<int>(OsuSetting.BeatmapDownloadRuleset).Value = 0;
            config.GetBindable<SearchCategory>(OsuSetting.BeatmapDownloadSearchCategory).Value = SearchCategory.Leaderboard;

            Assert.True(downloader.DownloadBeatmaps().Result == string.Empty);
        }

        [Test]
        public void TestDownloadFunctionFail()
        {
            API.Logout();
            Assert.True(downloader.DownloadBeatmaps().Result == BeatmapDownloaderStrings.YouNeedToBeLoggedInToDownloadBeatmaps.ToString());

            API.Login("dummy", "password");
            config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = new DateTime(DateTime.Now.Ticks + TimeSpan.TicksPerDay * 7);
            Assert.True(downloader.DownloadBeatmaps().Result == BeatmapDownloaderStrings.TheLastDownloadedBeatmapTimeIsInTheFuture.ToString());

            config.GetBindable<DateTime>(OsuSetting.BeatmapDownloadLastTime).Value = DateTime.Now.AddSeconds(-30);
            Assert.True(downloader.DownloadBeatmaps().Result == BeatmapDownloaderStrings.PleaseWaitAMinuteBeforeRequestingNewBeatmaps.ToString());
        }

        private class TestBeatmapDownloader : BeatmapDownloader
        {
            public TestBeatmapDownloader(OsuConfigManager config, BeatmapManager beatmapManager, IAPIProvider api, RulesetStore rulesets) : base(config, beatmapManager, api, rulesets)
            {
            }

            protected override void downloadBeatmap(BeatmapSetInfo beatmapSetInfo)
            {
                beatmapManager.Download(beatmapSetInfo);
                beatmapManager.GetExistingDownload(beatmapSetInfo).TriggerSuccess();
            }

            protected override void sendAPIReqeust(int iteration, RulesetInfo ruleset, SearchCategory searchCategory, Cursor cursor = null)
            {
                handleAPIReqeust(SearchResult.ResultsReturned(rulesetList), null, iteration, ruleset, searchCategory);
            }
        }
    }
}
