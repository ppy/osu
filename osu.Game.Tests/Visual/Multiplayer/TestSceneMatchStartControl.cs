// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

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
    public partial class TestSceneMatchStartControl : OsuManualInputManagerTestScene
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

        private OsuButton readyButton => control.ChildrenOfType<OsuButton>().Single();

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            new CachedModelDependencyContainer<Room>(base.CreateChildDependencies(parent)) { Model = { BindTarget = room } };

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs(multiplayerClient.Object);
            Dependencies.CacheAs(ongoingOperationTracker = new OngoingOperationTracker());
            Dependencies.CacheAs(availabilityTracker.Object);

            availabilityTracker.SetupGet(a => a.Availability).Returns(beatmapAvailability);

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
                             .Callback(() =>
                             {
                                 multiplayerClient.Raise(m => m.LoadRequested -= null);

                                 // immediately "end" gameplay, as we don't care about that part of the process.
                                 changeUserState(localUser.UserID, MultiplayerUserState.Idle);
                             });

            multiplayerClient.Setup(m => m.SendMatchRequest(It.IsAny<MatchUserRequest>()))
                             .Callback((MatchUserRequest request) =>
                             {
                                 switch (request)
                                 {
                                     case StartMatchCountdownRequest countdownStart:
                                         setRoomCountdown(countdownStart.Duration);
                                         break;

                                     case StopCountdownRequest:
                                         clearRoomCountdown();
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
                        TestMultiplayerClient.CreateMultiplayerPlaylistItem(playlistItem),
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
            checkLocalUserState(MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Idle);
        }

        [Test]
        public void TestCountdownWhileSpectating()
        {
            AddStep("set spectating", () => changeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            checkLocalUserState(MultiplayerUserState.Spectating);

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
            AddUntilStep("countdown started", () => multiplayerRoom.ActiveCountdowns.Any());

            AddStep("transfer host to local user", () => transferHost(localUser));
            AddUntilStep("local user is host", () => multiplayerRoom.Host?.Equals(multiplayerClient.Object.LocalUser) == true);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Ready);
            AddAssert("countdown still active", () => multiplayerRoom.ActiveCountdowns.Any());
        }

        [Test]
        public void TestCountdownButtonVisibilityWithAutoStart()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Ready);
            AddUntilStep("countdown button visible", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);

            AddStep("enable auto start", () => changeRoomSettings(new MultiplayerRoomSettings { AutoStartDuration = TimeSpan.FromMinutes(1) }));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Ready);
            AddUntilStep("countdown button not visible", () => !this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);
        }

        [Test]
        public void TestClickingReadyButtonUnReadiesDuringAutoStart()
        {
            AddStep("enable auto start", () => changeRoomSettings(new MultiplayerRoomSettings { AutoStartDuration = TimeSpan.FromMinutes(1) }));
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Idle);
        }

        [Test]
        public void TestDeletedBeatmapDisableReady()
        {
            AddUntilStep("ready button enabled", () => readyButton.Enabled.Value);

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
            checkLocalUserState(MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            if (!allReady)
                AddStep("add other user", () => addUser(new APIUser { Id = 2, Username = "Another user" }));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Ready);

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

            AddStep("make local user host", () => transferHost(localUser));

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
            checkLocalUserState(MultiplayerUserState.Ready);

            AddStep("transfer host", () => transferHost(multiplayerRoom.Users[1]));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            checkLocalUserState(MultiplayerUserState.Idle);
            AddUntilStep("ready button enabled", () => readyButton.Enabled.Value);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestManyUsersChangingState(bool isHost)
        {
            const int users = 10;

            AddStep("add many users", () =>
            {
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
                var nextUnready = multiplayerRoom.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    changeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, users);
        }

        [Test]
        public void TestAbortMatch()
        {
            AddStep("setup client", () =>
            {
                multiplayerClient.Setup(m => m.StartMatch())
                                 .Callback(() =>
                                 {
                                     multiplayerClient.Raise(m => m.LoadRequested -= null);
                                     multiplayerClient.Object.Room!.State = MultiplayerRoomState.WaitingForLoad;

                                     // The local user state doesn't really matter, so let's do the same as the base implementation for these tests.
                                     changeUserState(localUser.UserID, MultiplayerUserState.Idle);
                                 });

                multiplayerClient.Setup(m => m.AbortMatch())
                                 .Callback(() =>
                                 {
                                     multiplayerClient.Object.Room!.State = MultiplayerRoomState.Open;
                                     raiseRoomUpdated();
                                 });
            });

            // Ready
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            // Start match
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("countdown button disabled", () => !this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);

            // Abort
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddStep("check abort request received", () => multiplayerClient.Verify(m => m.AbortMatch(), Times.Once));
        }

        private void verifyGameplayStartFlow()
        {
            checkLocalUserState(MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddStep("check start request received", () => multiplayerClient.Verify(m => m.StartMatch(), Times.Once));
        }

        private void checkLocalUserState(MultiplayerUserState state) =>
            AddUntilStep($"local user is {state}", () => localUser.State == state);

        private void setRoomCountdown(TimeSpan duration)
        {
            multiplayerRoom.ActiveCountdowns.Add(new MatchStartCountdown { TimeRemaining = duration });
            raiseRoomUpdated();
        }

        private void clearRoomCountdown()
        {
            multiplayerRoom.ActiveCountdowns.Clear();
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

        private void changeRoomSettings(MultiplayerRoomSettings settings)
        {
            multiplayerRoom.Settings = settings;

            // Changing settings should reset all user ready statuses.
            foreach (var user in multiplayerRoom.Users)
            {
                if (user.State == MultiplayerUserState.Ready)
                    user.State = MultiplayerUserState.Idle;
            }

            raiseRoomUpdated();
        }

        private void raiseRoomUpdated() => multiplayerClient.Raise(m => m.RoomUpdated -= null);
    }
}
