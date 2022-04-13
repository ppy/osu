// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchStartControl : OsuManualInputManagerTestScene
    {
        private readonly Mock<MultiplayerClient> multiplayerClient = new Mock<MultiplayerClient>();
        private readonly Mock<OnlinePlayBeatmapAvailabilityTracker> availabilityTracker = new Mock<OnlinePlayBeatmapAvailabilityTracker>();

        private readonly Bindable<BeatmapAvailability> beatmapAvailability = new Bindable<BeatmapAvailability>();
        private readonly Bindable<Room> room = new Bindable<Room>();

        private MultiplayerRoom multiplayerRoom;
        private MultiplayerRoomUser localUser;
        private OngoingOperationTracker ongoingOperationTracker;

        private PopoverContainer content;
        private MatchStartControl control;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent)) { Model = { BindTarget = room } };

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs(multiplayerClient.Object);
            Dependencies.CacheAs(ongoingOperationTracker = new OngoingOperationTracker());

            availabilityTracker.SetupGet(a => a.Availability).Returns(beatmapAvailability);
            Dependencies.CacheAs(availabilityTracker.Object);

            multiplayerClient.SetupGet(m => m.LocalUser).Returns(() => localUser);
            multiplayerClient.SetupGet(m => m.Room).Returns(() => multiplayerRoom);

            // By default, the local user is to be the host.
            multiplayerClient.SetupGet(m => m.IsHost).Returns(() => ReferenceEquals(multiplayerRoom.Host, localUser));

            // Assume all state changes are accepted by the server.
            multiplayerClient.Setup(m => m.ChangeState(It.IsAny<MultiplayerUserState>()))
                             .Callback((MultiplayerUserState r) =>
                             {
                                 Logger.Log($"Changing local user state from {localUser.State} to {r}");
                                 localUser.State = r;
                                 raiseRoomUpdated();
                             });

            multiplayerClient.Setup(m => m.StartMatch())
                             .Callback(() => multiplayerClient.Raise(m => m.LoadRequested -= null));

            multiplayerClient.Setup(m => m.SendMatchRequest(It.IsAny<MatchUserRequest>()))
                             .Callback((MatchUserRequest request) =>
                             {
                                 switch (request)
                                 {
                                     case StartMatchCountdownRequest countdownStart:
                                         setRoomCountdown(countdownStart.Duration);
                                         break;

                                     case StopCountdownRequest _:
                                         multiplayerRoom.Countdown = null;
                                         raiseRoomUpdated();
                                         break;
                                 }
                             });

            Children = new Drawable[]
            {
                ongoingOperationTracker,
                content = new PopoverContainer { RelativeSizeAxes = Axes.Both }
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset state", () =>
            {
                multiplayerClient.Invocations.Clear();

                beatmapAvailability.Value = BeatmapAvailability.LocallyAvailable();

                var playlistItem = new PlaylistItem(Beatmap.Value.BeatmapInfo)
                {
                    RulesetID = Beatmap.Value.BeatmapInfo.Ruleset.OnlineID
                };

                room.Value = new Room
                {
                    Playlist = { playlistItem },
                    CurrentPlaylistItem = { Value = playlistItem }
                };

                localUser = new MultiplayerRoomUser(API.LocalUser.Value.Id) { User = API.LocalUser.Value };

                multiplayerRoom = new MultiplayerRoom(0)
                {
                    Playlist =
                    {
                        new MultiplayerPlaylistItem(playlistItem),
                    },
                    Users = { localUser },
                    Host = localUser,
                };
            });

            AddStep("create control", () =>
            {
                content.Child = control = new MatchStartControl
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(250, 50),
                };
            });
        }

        [Test]
        public void TestStartWithCountdown()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button shown", () => this.ChildrenOfType<MultiplayerCountdownButton>().SingleOrDefault()?.IsPresent == true);
            ClickButtonWhenEnabled<MultiplayerCountdownButton>();
            AddStep("click the first countdown button", () =>
            {
                var popoverButton = this.ChildrenOfType<Popover>().Single().ChildrenOfType<OsuButton>().First();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("check request received", () =>
            {
                multiplayerClient.Verify(m => m.SendMatchRequest(It.Is<StartMatchCountdownRequest>(req =>
                    req.Duration == TimeSpan.FromSeconds(10)
                )), Times.Once);
            });
        }

        [Test]
        public void TestCancelCountdown()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button shown", () => this.ChildrenOfType<MultiplayerCountdownButton>().SingleOrDefault()?.IsPresent == true);

            ClickButtonWhenEnabled<MultiplayerCountdownButton>();
            AddStep("click the first countdown button", () =>
            {
                var popoverButton = this.ChildrenOfType<Popover>().Single().ChildrenOfType<OsuButton>().First();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("check request received", () =>
            {
                multiplayerClient.Verify(m => m.SendMatchRequest(It.Is<StartMatchCountdownRequest>(req =>
                    req.Duration == TimeSpan.FromSeconds(10)
                )), Times.Once);
            });

            ClickButtonWhenEnabled<MultiplayerCountdownButton>();
            AddStep("click the cancel button", () =>
            {
                var popoverButton = this.ChildrenOfType<Popover>().Single().ChildrenOfType<OsuButton>().Last();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("check request received", () =>
            {
                multiplayerClient.Verify(m => m.SendMatchRequest(It.IsAny<StopCountdownRequest>()), Times.Once);
            });
        }

        [Test]
        public void TestReadyAndUnReadyDuringCountdown()
        {
            AddStep("add second user as host", () => addUser(new APIUser { Id = 2, Username = "Another user" }, true));

            AddStep("start countdown", () => setRoomCountdown(TimeSpan.FromMinutes(1)));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => localUser.State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => localUser.State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestCountdownWhileSpectating()
        {
            AddStep("set spectating", () => changeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("local user is spectating", () => multiplayerClient.Object.LocalUser?.State == MultiplayerUserState.Spectating);

            AddAssert("countdown button is visible", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);

            AddStep("add second user", () => addUser(new APIUser { Id = 2, Username = "Another user" }));
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);

            AddStep("set second user ready", () => changeUserState(2, MultiplayerUserState.Ready));
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestBecomeHostDuringCountdownAndReady()
        {
            AddStep("add second user as host", () =>
            {
                addUser(new APIUser { Id = 2, Username = "Another user" }, true);
            });

            AddStep("start countdown", () => multiplayerClient.Object.SendMatchRequest(new StartMatchCountdownRequest { Duration = TimeSpan.FromMinutes(1) }).WaitSafely());
            AddUntilStep("countdown started", () => multiplayerClient.Object.Room?.Countdown != null);

            AddStep("transfer host to local user", () => transferHost(localUser));
            AddUntilStep("local user is host", () => multiplayerClient.Object.Room?.Host?.Equals(multiplayerClient.Object.LocalUser) == true);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => multiplayerClient.Object.LocalUser?.State == MultiplayerUserState.Ready);
            AddAssert("countdown still active", () => multiplayerClient.Object.Room?.Countdown != null);
        }

        [Test]
        public void TestCountdownButtonVisibilityWithAutoStartEnablement()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => multiplayerClient.Object.LocalUser?.State == MultiplayerUserState.Ready);
            AddUntilStep("countdown button visible", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);

            AddStep("enable auto start", () => multiplayerClient.Object.ChangeSettings(new MultiplayerRoomSettings { AutoStartDuration = TimeSpan.FromMinutes(1) }).WaitSafely());

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => multiplayerClient.Object.LocalUser?.State == MultiplayerUserState.Ready);
            AddUntilStep("countdown button not visible", () => !this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);
        }

        [Test]
        public void TestClickingReadyButtonUnReadiesDuringAutoStart()
        {
            AddStep("enable auto start", () => multiplayerClient.Object.ChangeSettings(new MultiplayerRoomSettings { AutoStartDuration = TimeSpan.FromMinutes(1) }).WaitSafely());
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => multiplayerClient.Object.LocalUser?.State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became idle", () => multiplayerClient.Object.LocalUser?.State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestDeletedBeatmapDisableReady()
        {
            OsuButton readyButton = null;

            AddUntilStep("ensure ready button enabled", () =>
            {
                readyButton = control.ChildrenOfType<OsuButton>().Single();
                return readyButton.Enabled.Value;
            });

            AddStep("mark beatmap not available", () => beatmapAvailability.Value = BeatmapAvailability.NotDownloaded());
            AddUntilStep("ready button disabled", () => !readyButton.Enabled.Value);
            AddStep("mark beatmap available", () => beatmapAvailability.Value = BeatmapAvailability.LocallyAvailable());
            AddUntilStep("ready button enabled back", () => readyButton.Enabled.Value);
        }

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            AddStep("add second user as host", () =>
            {
                addUser(new APIUser { Id = 2, Username = "Another user" }, true);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => localUser.State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => localUser.State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                transferHost(multiplayerRoom.Users[0]);

                if (!allReady)
                    addUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => localUser.State == MultiplayerUserState.Ready);

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            AddStep("add host", () =>
            {
                addUser(new APIUser { Id = 2, Username = "Another user" }, true);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddStep("make user host", () => transferHost(localUser));

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                addUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => localUser.State == MultiplayerUserState.Ready);

            AddStep("transfer host", () => transferHost(multiplayerRoom.Users[1]));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle (match not started)", () => localUser.State == MultiplayerUserState.Idle);
            AddUntilStep("ready button enabled", () => control.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestManyUsersChangingState(bool isHost)
        {
            const int users = 10;
            AddStep("setup", () =>
            {
                transferHost(localUser);
                for (int i = 0; i < users; i++)
                    addUser(new APIUser { Id = i, Username = "Another user" }, !isHost && i == 2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddRepeatStep("change user ready state", () =>
            {
                changeUserState(RNG.Next(0, users), RNG.NextBool() ? MultiplayerUserState.Ready : MultiplayerUserState.Idle);
            }, 20);

            AddRepeatStep("ready all users", () =>
            {
                var nextUnready = multiplayerClient.Object.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    changeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, users);
        }

        private void verifyGameplayStartFlow()
        {
            AddUntilStep("user is ready", () => localUser.State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddStep("check start request received", () => multiplayerClient.Verify(m => m.StartMatch(), Times.Once));
            AddUntilStep("user waiting for load", () => localUser.State == MultiplayerUserState.WaitingForLoad);

            AddUntilStep("ready button disabled", () => !control.ChildrenOfType<OsuButton>().Single().Enabled.Value);

            AddStep("finish gameplay", () => changeUserState(localUser.UserID, MultiplayerUserState.Idle));

            AddUntilStep("ready button enabled", () => control.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }

        private void setRoomCountdown(TimeSpan duration)
        {
            multiplayerRoom.Countdown = new MatchStartCountdown { TimeRemaining = duration };
            raiseRoomUpdated();
        }

        private void changeUserState(int userId, MultiplayerUserState newState)
        {
            multiplayerRoom.Users.Single(u => u.UserID == userId).State = newState;
            raiseRoomUpdated();
        }

        private void addUser(APIUser user, bool asHost = false)
        {
            var multiplayerRoomUser = new MultiplayerRoomUser(user.Id) { User = user };

            multiplayerRoom.Users.Add(multiplayerRoomUser);

            if (asHost)
                transferHost(multiplayerRoomUser);

            raiseRoomUpdated();
        }

        private void transferHost(MultiplayerRoomUser user)
        {
            multiplayerRoom.Host = user;
            raiseRoomUpdated();
        }

        private void raiseRoomUpdated() => multiplayerClient.Raise(m => m.RoomUpdated -= null);
    }
}
