// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneUserPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private IRulesetStore rulesetStore { get; set; } = null!;

        private TestUserStatisticsProvider statisticsProvider = null!;
        private TestMetadataClient metadataClient = null!;
        private TestUserListPanel panel = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(LocalUserStatisticsProvider), statisticsProvider = new TestUserStatisticsProvider()),
                    (typeof(MetadataClient), metadataClient = new TestMetadataClient())
                ],
                Children = new Drawable[]
                {
                    statisticsProvider,
                    metadataClient,
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Spacing = new Vector2(10f),
                        Children = new Drawable[]
                        {
                            new UserBrickPanel(new APIUser
                            {
                                Username = @"flyte",
                                Id = 3103765,
                                CoverUrl = @"https://assets.ppy.sh/user-cover-presets/1/df28696b58541a9e67f6755918951d542d93bdf1da41720fcca2fd2c1ea8cf51.jpeg",
                            }),
                            new UserBrickPanel(new APIUser
                            {
                                Username = @"peppy",
                                Id = 2,
                                Colour = "99EB47",
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                            }),
                            new UserGridPanel(new APIUser
                            {
                                Username = @"flyte",
                                Id = 3103765,
                                CountryCode = CountryCode.JP,
                                CoverUrl = @"https://assets.ppy.sh/user-cover-presets/1/df28696b58541a9e67f6755918951d542d93bdf1da41720fcca2fd2c1ea8cf51.jpeg",
                                IsOnline = true
                            }) { Width = 300 },
                            new UserGridPanel(new APIUser
                            {
                                Username = @"peppy",
                                Id = 2,
                                CountryCode = CountryCode.AU,
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                                IsSupporter = true,
                                SupportLevel = 3,
                            }) { Width = 300 },
                            panel = new TestUserListPanel(new APIUser
                            {
                                Username = @"peppy",
                                Id = 2,
                                CountryCode = CountryCode.AU,
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                                LastVisit = DateTimeOffset.Now
                            }),
                            new UserRankPanel(new APIUser
                            {
                                Username = @"flyte",
                                Id = 3103765,
                                CountryCode = CountryCode.JP,
                                CoverUrl = @"https://assets.ppy.sh/user-cover-presets/1/df28696b58541a9e67f6755918951d542d93bdf1da41720fcca2fd2c1ea8cf51.jpeg",
                                Statistics = new UserStatistics { GlobalRank = 12345, CountryRank = 1234 }
                            }) { Width = 300 },
                            new UserRankPanel(new APIUser
                            {
                                Username = @"peppy",
                                Id = 2,
                                Colour = "99EB47",
                                CountryCode = CountryCode.AU,
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                                Statistics = new UserStatistics { GlobalRank = null, CountryRank = null }
                            }) { Width = 300 },
                            new UserGridPanel(API.LocalUser.Value)
                            {
                                Width = 300
                            }
                        }
                    }
                }
            };

            metadataClient.BeginWatchingUserPresence();
        });

        [Test]
        public void TestUserStatus()
        {
            AddStep("online", () => setPresence(UserStatus.Online, null));
            AddStep("do not disturb", () => setPresence(UserStatus.DoNotDisturb, null));
            AddStep("offline", () => setPresence(UserStatus.Offline, null));
        }

        [Test]
        public void TestUserActivity()
        {
            AddStep("idle", () => setPresence(UserStatus.Online, null));
            AddStep("watching replay", () => setPresence(UserStatus.Online, new UserActivity.WatchingReplay(createScore(@"nats"))));
            AddStep("spectating user", () => setPresence(UserStatus.Online, new UserActivity.SpectatingUser(createScore(@"mrekk"))));
            AddStep("solo (osu!)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(0)));
            AddStep("solo (osu!taiko)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(1)));
            AddStep("solo (osu!catch)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(2)));
            AddStep("solo (osu!mania)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(3)));
            AddStep("choosing", () => setPresence(UserStatus.Online, new UserActivity.ChoosingBeatmap()));
            AddStep("editing beatmap", () => setPresence(UserStatus.Online, new UserActivity.EditingBeatmap(new BeatmapInfo())));
            AddStep("modding beatmap", () => setPresence(UserStatus.Online, new UserActivity.ModdingBeatmap(new BeatmapInfo())));
            AddStep("testing beatmap", () => setPresence(UserStatus.Online, new UserActivity.TestingBeatmap(new BeatmapInfo())));
        }

        [Test]
        public void TestUserActivityChange()
        {
            AddAssert("visit message is visible", () => panel.LastVisitMessage.IsPresent);
            AddStep("set online status", () => setPresence(UserStatus.Online, null));
            AddAssert("visit message is not visible", () => !panel.LastVisitMessage.IsPresent);
            AddStep("set choosing activity", () => setPresence(UserStatus.Online, new UserActivity.ChoosingBeatmap()));
            AddStep("set offline status", () => setPresence(UserStatus.Offline, null));
            AddAssert("visit message is visible", () => panel.LastVisitMessage.IsPresent);
            AddStep("set online status", () => setPresence(UserStatus.Online, null));
            AddAssert("visit message is not visible", () => !panel.LastVisitMessage.IsPresent);
        }

        [Test]
        public void TestUserStatisticsChange()
        {
            AddStep("update statistics", () =>
            {
                statisticsProvider.UpdateStatistics(new UserStatistics
                {
                    GlobalRank = RNG.Next(100000),
                    CountryRank = RNG.Next(100000)
                }, Ruleset.Value);
            });
            AddStep("set statistics to something big", () =>
            {
                statisticsProvider.UpdateStatistics(new UserStatistics
                {
                    GlobalRank = RNG.Next(1_000_000, 100_000_000),
                    CountryRank = RNG.Next(1_000_000, 100_000_000)
                }, Ruleset.Value);
            });
            AddStep("set statistics to empty", () => statisticsProvider.UpdateStatistics(new UserStatistics(), Ruleset.Value));
        }

        [Test]
        public void TestLocalUserActivity()
        {
            AddStep("idle", () => setPresence(UserStatus.Online, null, API.LocalUser.Value.OnlineID));
            AddStep("watching replay", () => setPresence(UserStatus.Online, new UserActivity.WatchingReplay(createScore(@"nats")), API.LocalUser.Value.OnlineID));
            AddStep("spectating user", () => setPresence(UserStatus.Online, new UserActivity.SpectatingUser(createScore(@"mrekk")), API.LocalUser.Value.OnlineID));
            AddStep("solo (osu!)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(0), API.LocalUser.Value.OnlineID));
            AddStep("solo (osu!taiko)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(1), API.LocalUser.Value.OnlineID));
            AddStep("solo (osu!catch)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(2), API.LocalUser.Value.OnlineID));
            AddStep("solo (osu!mania)", () => setPresence(UserStatus.Online, soloGameStatusForRuleset(3), API.LocalUser.Value.OnlineID));
            AddStep("choosing", () => setPresence(UserStatus.Online, new UserActivity.ChoosingBeatmap(), API.LocalUser.Value.OnlineID));
            AddStep("editing beatmap", () => setPresence(UserStatus.Online, new UserActivity.EditingBeatmap(new BeatmapInfo()), API.LocalUser.Value.OnlineID));
            AddStep("modding beatmap", () => setPresence(UserStatus.Online, new UserActivity.ModdingBeatmap(new BeatmapInfo()), API.LocalUser.Value.OnlineID));
            AddStep("testing beatmap", () => setPresence(UserStatus.Online, new UserActivity.TestingBeatmap(new BeatmapInfo()), API.LocalUser.Value.OnlineID));
            AddStep("set offline status", () => setPresence(UserStatus.Offline, null, API.LocalUser.Value.OnlineID));
        }

        private void setPresence(UserStatus status, UserActivity? activity, int? userId = null)
        {
            if (status == UserStatus.Offline)
                metadataClient.UserPresenceUpdated(userId ?? panel.User.OnlineID, null);
            else
                metadataClient.UserPresenceUpdated(userId ?? panel.User.OnlineID, new UserPresence { Status = status, Activity = activity });
        }

        private UserActivity soloGameStatusForRuleset(int rulesetId) => new UserActivity.InSoloGame(new BeatmapInfo(), rulesetStore.GetRuleset(rulesetId)!);

        private ScoreInfo createScore(string name) => new ScoreInfo(new TestBeatmap(Ruleset.Value).BeatmapInfo)
        {
            User = new APIUser
            {
                Username = name,
            }
        };

        private partial class TestUserListPanel : UserListPanel
        {
            public TestUserListPanel(APIUser user)
                : base(user)
            {
            }

            public new TextFlowContainer LastVisitMessage => base.LastVisitMessage;
        }

        public partial class TestUserStatisticsProvider : LocalUserStatisticsProvider
        {
            public new void UpdateStatistics(UserStatistics newStatistics, RulesetInfo ruleset, Action<UserStatisticsUpdate>? callback = null)
                => base.UpdateStatistics(newStatistics, ruleset, callback);
        }
    }
}
