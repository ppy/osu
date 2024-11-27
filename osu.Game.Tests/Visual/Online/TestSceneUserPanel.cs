// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneUserPanel : OsuTestScene
    {
        private readonly Bindable<UserActivity?> activity = new Bindable<UserActivity?>();
        private readonly Bindable<UserStatus?> status = new Bindable<UserStatus?>();

        private UserGridPanel boundPanel1 = null!;
        private TestUserListPanel boundPanel2 = null!;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Cached(typeof(LocalUserStatisticsProvider))]
        private readonly TestUserStatisticsProvider statisticsProvider = new TestUserStatisticsProvider();

        [Resolved]
        private IRulesetStore rulesetStore { get; set; } = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            activity.Value = null;
            status.Value = null;

            Remove(statisticsProvider, false);
            Clear();
            Add(statisticsProvider);

            Add(new FillFlowContainer
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
                        Status = { Value = UserStatus.Online }
                    }) { Width = 300 },
                    boundPanel1 = new UserGridPanel(new APIUser
                    {
                        Username = @"peppy",
                        Id = 2,
                        CountryCode = CountryCode.AU,
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                        IsSupporter = true,
                        SupportLevel = 3,
                    }) { Width = 300 },
                    boundPanel2 = new TestUserListPanel(new APIUser
                    {
                        Username = @"Evast",
                        Id = 8195163,
                        CountryCode = CountryCode.BY,
                        CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                        IsOnline = false,
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
                    }) { Width = 300 }
                }
            });

            boundPanel1.Status.BindTo(status);
            boundPanel1.Activity.BindTo(activity);

            boundPanel2.Status.BindTo(status);
            boundPanel2.Activity.BindTo(activity);
        });

        [Test]
        public void TestUserStatus()
        {
            AddStep("online", () => status.Value = UserStatus.Online);
            AddStep("do not disturb", () => status.Value = UserStatus.DoNotDisturb);
            AddStep("offline", () => status.Value = UserStatus.Offline);
            AddStep("null status", () => status.Value = null);
        }

        [Test]
        public void TestUserActivity()
        {
            AddStep("set online status", () => status.Value = UserStatus.Online);

            AddStep("idle", () => activity.Value = null);
            AddStep("watching replay", () => activity.Value = new UserActivity.WatchingReplay(createScore(@"nats")));
            AddStep("spectating user", () => activity.Value = new UserActivity.SpectatingUser(createScore(@"mrekk")));
            AddStep("solo (osu!)", () => activity.Value = soloGameStatusForRuleset(0));
            AddStep("solo (osu!taiko)", () => activity.Value = soloGameStatusForRuleset(1));
            AddStep("solo (osu!catch)", () => activity.Value = soloGameStatusForRuleset(2));
            AddStep("solo (osu!mania)", () => activity.Value = soloGameStatusForRuleset(3));
            AddStep("choosing", () => activity.Value = new UserActivity.ChoosingBeatmap());
            AddStep("editing beatmap", () => activity.Value = new UserActivity.EditingBeatmap(new BeatmapInfo()));
            AddStep("modding beatmap", () => activity.Value = new UserActivity.ModdingBeatmap(new BeatmapInfo()));
            AddStep("testing beatmap", () => activity.Value = new UserActivity.TestingBeatmap(new BeatmapInfo()));
        }

        [Test]
        public void TestUserActivityChange()
        {
            AddAssert("visit message is visible", () => boundPanel2.LastVisitMessage.IsPresent);
            AddStep("set online status", () => status.Value = UserStatus.Online);
            AddAssert("visit message is not visible", () => !boundPanel2.LastVisitMessage.IsPresent);
            AddStep("set choosing activity", () => activity.Value = new UserActivity.ChoosingBeatmap());
            AddStep("set offline status", () => status.Value = UserStatus.Offline);
            AddAssert("visit message is visible", () => boundPanel2.LastVisitMessage.IsPresent);
            AddStep("set online status", () => status.Value = UserStatus.Online);
            AddAssert("visit message is not visible", () => !boundPanel2.LastVisitMessage.IsPresent);
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
