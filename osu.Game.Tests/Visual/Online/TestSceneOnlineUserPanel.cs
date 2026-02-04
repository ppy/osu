// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.CurrentlyOnline;
using osu.Game.Rulesets;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneOnlineUserPanel : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        [Resolved]
        private IRulesetStore rulesetStore { get; set; } = null!;

        private TestMetadataClient metadataClient = null!;
        private OnlineUserListPanel panel = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(MetadataClient), metadataClient = new TestMetadataClient())
                ],
                Children = new Drawable[]
                {
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
                            new OnlineUserGridPanel(new APIUser
                            {
                                Username = @"flyte",
                                Id = 3103765,
                                CountryCode = CountryCode.JP,
                                CoverUrl = @"https://assets.ppy.sh/user-cover-presets/1/df28696b58541a9e67f6755918951d542d93bdf1da41720fcca2fd2c1ea8cf51.jpeg",
                                WasRecentlyOnline = true
                            }),
                            new OnlineUserGridPanel(new APIUser
                            {
                                Username = @"peppy",
                                Id = 2,
                                CountryCode = CountryCode.AU,
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                                IsSupporter = true,
                                SupportLevel = 3,
                            }),
                            new OnlineUserListPanel(new APIUser
                            {
                                Username = @"flyte",
                                Id = 3103765,
                                CountryCode = CountryCode.JP,
                                CoverUrl = @"https://assets.ppy.sh/user-cover-presets/1/df28696b58541a9e67f6755918951d542d93bdf1da41720fcca2fd2c1ea8cf51.jpeg",
                                WasRecentlyOnline = true
                            }),
                            panel = new OnlineUserListPanel(new APIUser
                            {
                                Username = @"peppy",
                                Id = 2,
                                CountryCode = CountryCode.AU,
                                CoverUrl = @"https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                                LastVisit = DateTimeOffset.Now
                            }),
                        }
                    }
                }
            };

            metadataClient.BeginWatchingUserPresence();
        });

        [Test]
        public void TestUserActivity()
        {
            AddStep("idle", () => setPresence(UserStatus.Online, null));
            AddStep("in game", () => setPresence(UserStatus.Online, new UserActivity.InSoloGame(new BeatmapInfo(), rulesetStore.GetRuleset(0)!)));
        }

        private void setPresence(UserStatus status, UserActivity? activity, int? userId = null)
        {
            if (status == UserStatus.Offline)
                metadataClient.UserPresenceUpdated(userId ?? panel.User.OnlineID, null);
            else
                metadataClient.UserPresenceUpdated(userId ?? panel.User.OnlineID, new UserPresence { Status = status, Activity = activity });
        }
    }
}
