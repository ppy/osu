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
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;
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
                api.Friends.Clear();
                api.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad(3);

            AddStep("remove one friend", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.RemoveAt(0);
            });

            waitForLoad(2);

            AddStep("add one friend", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.AddRange(getUsers().Take(1).Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad(3);

            void waitForLoad(int expectedPanels)
            {
                AddUntilStep("wait for friends to load", () => this.ChildrenOfType<FriendsList>().LastOrDefault()?.IsLoaded == true);
                AddAssert($"{expectedPanels} panels in list", () => this.ChildrenOfType<FriendsList>().Last().ChildrenOfType<UserPanel>().Count(), () => Is.EqualTo(expectedPanels));
            }
        }

        [Test]
        public void TestChangeDisplayStyle()
        {
            AddStep("set friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.Clear();
                api.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            waitForLoad<UserGridPanel>();

            AddStep("set list style", () => this.ChildrenOfType<UserListToolbar>().Single().DisplayStyle.Value = OverlayPanelDisplayStyle.List);
            waitForLoad<UserListPanel>();

            AddStep("set brick style", () => this.ChildrenOfType<UserListToolbar>().Single().DisplayStyle.Value = OverlayPanelDisplayStyle.Brick);
            waitForLoad<UserBrickPanel>();

            void waitForLoad<T>()
            {
                AddUntilStep("wait for friends to load", () => this.ChildrenOfType<FriendsList>().LastOrDefault()?.IsLoaded == true);
                AddAssert($"3 {typeof(T).ReadableName()} in list", () => this.ChildrenOfType<FriendsList>().Last().ChildrenOfType<T>().Count(), () => Is.EqualTo(3));
            }
        }

        [Test]
        public void TestOnlinePresence()
        {
            AddStep("set friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.Clear();
                api.Friends.AddRange(getUsers().Select(u => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = u.OnlineID,
                    TargetUser = u
                }));
            });

            AddUntilStep("wait for friends to load", () => this.ChildrenOfType<FriendsList>().LastOrDefault()?.IsLoaded == true);
            assertVisible(3);

            AddStep("change to online stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.Online);
            assertVisible(0);

            AddStep("bring a friend online", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                metadataClient.FriendPresenceUpdated(api.Friends[0].TargetID, new UserPresence { Status = UserStatus.Online });
            });

            assertVisible(1);

            AddStep("change to offline stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.Offline);
            assertVisible(2);

            AddStep("bring a friend online", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                metadataClient.FriendPresenceUpdated(api.Friends[1].TargetID, new UserPresence { Status = UserStatus.Online });
            });

            assertVisible(1);

            AddStep("change to online stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.Online);
            assertVisible(2);

            AddStep("change to all stream", () => this.ChildrenOfType<FriendOnlineStreamControl>().Single().Current.Value = OnlineStatus.All);
            assertVisible(3);

            void assertVisible(int expectedPanels)
            {
                AddAssert($"{expectedPanels} panels visible",
                    () => this.ChildrenOfType<FriendsList.FilterableUserPanel>().Count(p => p.Alpha > 0),
                    () => Is.EqualTo(expectedPanels));
            }
        }

        private List<APIUser> getUsers() => new List<APIUser>
        {
            new APIUser
            {
                Username = "flyte",
                Id = 3103765,
                IsOnline = true,
                Statistics = new UserStatistics { GlobalRank = 1111 },
                CountryCode = CountryCode.JP,
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c6.jpg"
            },
            new APIUser
            {
                Username = "peppy",
                Id = 2,
                IsOnline = false,
                Statistics = new UserStatistics { GlobalRank = 2222 },
                CountryCode = CountryCode.AU,
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
                IsSupporter = true,
                SupportLevel = 3,
            },
            new APIUser
            {
                Username = "Evast",
                Id = 8195163,
                CountryCode = CountryCode.BY,
                CoverUrl = "https://assets.ppy.sh/user-profile-covers/8195163/4a8e2ad5a02a2642b631438cfa6c6bd7e2f9db289be881cb27df18331f64144c.jpeg",
                IsOnline = false,
                LastVisit = DateTimeOffset.Now
            }
        };
    }
}
