// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Online.Metadata;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Tests.Visual.Metadata;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Components
{
    public partial class TestSceneFriendPresenceNotifier : OsuManualInputManagerTestScene
    {
        private ChannelManager channelManager = null!;
        private NotificationOverlay notificationOverlay = null!;
        private ChatOverlay chatOverlay = null!;
        private TestMetadataClient metadataClient = null!;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies =
                [
                    (typeof(ChannelManager), channelManager = new ChannelManager(API)),
                    (typeof(INotificationOverlay), notificationOverlay = new NotificationOverlay()),
                    (typeof(ChatOverlay), chatOverlay = new ChatOverlay()),
                    (typeof(MetadataClient), metadataClient = new TestMetadataClient()),
                ],
                Children = new Drawable[]
                {
                    channelManager,
                    notificationOverlay,
                    chatOverlay,
                    metadataClient,
                    new FriendPresenceNotifier()
                }
            };

            for (int i = 1; i <= 100; i++)
                ((DummyAPIAccess)API).LocalUserState.Friends.Add(new APIRelation { TargetID = i, TargetUser = new APIUser { Username = $"Friend {i}" } });
        });

        [Test]
        public void TestNotifications()
        {
            AddStep("bring friend 1 online", () => metadataClient.FriendPresenceUpdated(1, new UserPresence { Status = UserStatus.Online }));
            AddUntilStep("wait for notification", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(1));
            AddStep("bring friend 1 offline", () => metadataClient.FriendPresenceUpdated(1, null));
            AddUntilStep("wait for notification", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(2));
        }

        [Test]
        public void TestSingleUserNotificationOpensChat()
        {
            AddStep("bring friend 1 online", () => metadataClient.FriendPresenceUpdated(1, new UserPresence { Status = UserStatus.Online }));
            AddUntilStep("wait for notification", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(1));

            AddStep("click notification", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<Notification>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("chat overlay opened", () => chatOverlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddUntilStep("user channel selected",
                () => channelManager.CurrentChannel.Value.Name,
                () => Is.EqualTo(((DummyAPIAccess)API).LocalUserState.Friends[0].TargetUser!.Username));
        }

        [Test]
        public void TestMultipleUserNotificationDoesNotOpenChat()
        {
            AddStep("bring friends 1 & 2 online", () =>
            {
                metadataClient.FriendPresenceUpdated(1, new UserPresence { Status = UserStatus.Online });
                metadataClient.FriendPresenceUpdated(2, new UserPresence { Status = UserStatus.Online });
            });

            AddUntilStep("wait for notification", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(1));

            AddStep("click notification", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<Notification>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("chat overlay not opened", () => chatOverlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestNonFriendsDoNotNotify()
        {
            AddStep("bring non-friend 1000 online", () => metadataClient.UserPresenceUpdated(1000, new UserPresence { Status = UserStatus.Online }));
            AddWaitStep("wait for possible notification", 10);
            AddAssert("no notification", () => notificationOverlay.AllNotifications.Count(), () => Is.Zero);
        }

        [Test]
        public void TestPostManyDebounced()
        {
            AddStep("bring friends 1-10 online", () =>
            {
                for (int i = 1; i <= 10; i++)
                    metadataClient.FriendPresenceUpdated(i, new UserPresence { Status = UserStatus.Online });
            });

            AddUntilStep("wait for notification", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(1));

            AddStep("bring friends 1-10 offline", () =>
            {
                for (int i = 1; i <= 10; i++)
                    metadataClient.FriendPresenceUpdated(i, null);
            });

            AddUntilStep("wait for notification", () => notificationOverlay.AllNotifications.Count(), () => Is.EqualTo(2));
        }
    }
}
