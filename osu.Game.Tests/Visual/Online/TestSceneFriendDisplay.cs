// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneFriendDisplay : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private TestMetadataClient metadataClient;

        [SetUp]
        public void Setup() => Schedule(() =>
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
                    new BasicScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new FriendDisplay()
                    }
                }
            };
        });

        [Test]
        public void TestAddAndRemoveFriends()
        {
            AddStep("set friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.LocalUserState.Friends.Clear();
                api.LocalUserState.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad();
            assertVisiblePanelCount<UserPanel>(3);

            AddStep("remove one friend", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.LocalUserState.Friends.RemoveAt(0);
            });

            waitForLoad();
            assertVisiblePanelCount<UserPanel>(2);

            AddStep("add one friend", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.LocalUserState.Friends.AddRange(getUsers().Take(1).Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad();
            assertVisiblePanelCount<UserPanel>(3);
        }

        [Test]
        public void TestChangeDisplayStyle()
        {
            AddStep("set friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.LocalUserState.Friends.Clear();
                api.LocalUserState.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad();
            assertVisiblePanelCount<UserGridPanel>(3);

            AddStep("set list style", () => this.ChildrenOfType<UserListToolbar>().Single().DisplayStyle.Value = OverlayPanelDisplayStyle.List);

            waitForLoad();
            assertVisiblePanelCount<UserListPanel>(3);

            AddStep("set brick style", () => this.ChildrenOfType<UserListToolbar>().Single().DisplayStyle.Value = OverlayPanelDisplayStyle.Brick);

            waitForLoad();
            assertVisiblePanelCount<UserBrickPanel>(3);
        }

        [Test]
        public void TestOnlinePresence()
        {
            AddStep("set friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.LocalUserState.Friends.Clear();
                api.LocalUserState.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad();
            assertVisiblePanelCount<UserPanel>(3);

            AddStep("change to online stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.Online);
            assertVisiblePanelCount<UserPanel>(0);

            AddStep("bring a friend online", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                metadataClient.FriendPresenceUpdated(api.LocalUserState.Friends[0].TargetID, new UserPresence { Status = UserStatus.Online });
            });

            assertVisiblePanelCount<UserPanel>(1);

            AddStep("change to offline stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.Offline);
            assertVisiblePanelCount<UserPanel>(2);

            AddStep("bring a friend online", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                metadataClient.FriendPresenceUpdated(api.LocalUserState.Friends[1].TargetID, new UserPresence { Status = UserStatus.Online });
            });

            assertVisiblePanelCount<UserPanel>(1);

            AddStep("change to online stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.Online);
            assertVisiblePanelCount<UserPanel>(2);

            AddStep("take friend offline", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                metadataClient.FriendPresenceUpdated(api.LocalUserState.Friends[1].TargetID, null);
            });
            assertVisiblePanelCount<UserPanel>(1);

            AddStep("change to all stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.All);
            assertVisiblePanelCount<UserPanel>(3);
        }

        [Test]
        public void TestLoadFriendsBeforeDisplay()
        {
            AddStep("set friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.LocalUserState.Friends.Clear();
                api.LocalUserState.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            AddStep("load new display", () =>
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
                        new BasicScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = new FriendDisplay()
                        }
                    }
                };
            });

            waitForLoad();
            assertVisiblePanelCount<UserPanel>(3);
        }

        private void waitForLoad()
            => AddUntilStep("wait for panels to load", () => this.ChildrenOfType<LoadingSpinner>().First().State.Value, () => Is.EqualTo(Visibility.Hidden));

        private void assertVisiblePanelCount<T>(int expectedVisible)
            where T : UserPanel
        {
            AddAssert($"{typeof(T).ReadableName()}s in list", () => this.ChildrenOfType<FriendsList>().Last().ChildrenOfType<UserPanel>().All(p => p is T));
            AddAssert($"{expectedVisible} panels visible", () => this.ChildrenOfType<FriendsList>().Last().ChildrenOfType<FriendsList.FilterableUserPanel>().Count(p => p.IsPresent),
                () => Is.EqualTo(expectedVisible));
        }

        private List<APIUser> getUsers() => new List<APIUser>
        {
            new APIUser
            {
                Username = "flyte",
                Id = 3103765,
                WasRecentlyOnline = true,
                Statistics = new UserStatistics { GlobalRank = 1111 },
                CountryCode = CountryCode.JP,
                CoverUrl = TestResources.COVER_IMAGE_4
            },
            new APIUser
            {
                Username = "peppy",
                Id = 2,
                WasRecentlyOnline = false,
                Statistics = new UserStatistics { GlobalRank = 2222 },
                CountryCode = CountryCode.AU,
                CoverUrl = TestResources.COVER_IMAGE_3,
                IsSupporter = true,
                SupportLevel = 3,
            },
            new APIUser
            {
                Username = "Evast",
                Id = 8195163,
                CountryCode = CountryCode.BY,
                CoverUrl = "https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                WasRecentlyOnline = false,
                LastVisit = DateTimeOffset.Now
            }
        };
    }
}
