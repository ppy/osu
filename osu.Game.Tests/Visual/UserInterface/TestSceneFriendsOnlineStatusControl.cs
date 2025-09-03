// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Dashboard.Friends;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneFriendsOnlineStatusControl : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TestMetadataClient metadataClient = null!;

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
                    new FriendOnlineStreamControl
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                }
            };
        });

        [Test]
        public void TestChangeFriends()
        {
            AddStep("set 10 friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.Clear();
                api.Friends.AddRange(Enumerable.Range(1, 10).Select(i => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = i,
                    TargetUser = new APIUser { Id = i },
                }));
            });

            AddStep("set 20 friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.Clear();
                api.Friends.AddRange(Enumerable.Range(1, 20).Select(i => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = i,
                    TargetUser = new APIUser { Id = i },
                }));
            });
        }

        [Test]
        public void TestChangeOnlineStates()
        {
            AddStep("set 10 friends", () =>
            {
                DummyAPIAccess api = (DummyAPIAccess)API;
                api.Friends.Clear();
                api.Friends.AddRange(Enumerable.Range(1, 10).Select(i => new APIRelation
                {
                    RelationType = RelationType.Friend,
                    TargetID = i,
                    TargetUser = new APIUser { Id = i },
                }));
            });

            AddStep("make users 1-5 online", () =>
            {
                for (int i = 1; i <= 5; i++)
                    metadataClient.FriendPresenceUpdated(i, new UserPresence { Status = UserStatus.Online });
            });

            AddStep("make users 1-5 DnD", () =>
            {
                for (int i = 1; i <= 5; i++)
                    metadataClient.FriendPresenceUpdated(i, new UserPresence { Status = UserStatus.DoNotDisturb });
            });

            AddStep("make users 1-5 offline", () =>
            {
                for (int i = 1; i <= 5; i++)
                    metadataClient.FriendPresenceUpdated(i, null);
            });
        }
    }
}
