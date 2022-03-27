// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Countdown;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMatchStartControl : MultiplayerTestScene
    {
        private MatchStartControl control;
        private BeatmapSetInfo importedSet;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            AvailabilityTracker.SelectedItem.BindTo(selectedItem);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
            importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First());

            selectedItem.Value = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                RulesetID = Beatmap.Value.BeatmapInfo.Ruleset.OnlineID
            };

            Child = new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = control = new MatchStartControl
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(250, 50),
                }
            };
        });

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

            AddStep("finish countdown", () => MultiplayerClient.SkipToEndOfCountdown());
            AddUntilStep("match started", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.WaitingForLoad);
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

            ClickButtonWhenEnabled<MultiplayerCountdownButton>();
            AddStep("click the cancel button", () =>
            {
                var popoverButton = this.ChildrenOfType<Popover>().Single().ChildrenOfType<OsuButton>().Last();
                InputManager.MoveMouseTo(popoverButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("finish countdown", () => MultiplayerClient.SkipToEndOfCountdown());
            AddUntilStep("match not started", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
        }

        [Test]
        public void TestReadyAndUnReadyDuringCountdown()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            AddStep("start with countdown", () => MultiplayerClient.SendMatchRequest(new StartMatchCountdownRequest { Duration = TimeSpan.FromMinutes(2) }).WaitSafely());

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestCountdownWhileSpectating()
        {
            AddStep("set spectating", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("local user is spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddAssert("countdown button is visible", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);

            AddStep("add second user", () => MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" }));
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);

            AddStep("set second user ready", () => MultiplayerClient.ChangeUserState(2, MultiplayerUserState.Ready));
            AddAssert("countdown button enabled", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestBecomeHostDuringCountdownAndReady()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            AddStep("start countdown", () => MultiplayerClient.SendMatchRequest(new StartMatchCountdownRequest { Duration = TimeSpan.FromMinutes(1) }).WaitSafely());
            AddUntilStep("countdown started", () => MultiplayerClient.Room?.Countdown != null);

            AddStep("transfer host to local user", () => MultiplayerClient.TransferHost(API.LocalUser.Value.OnlineID));
            AddUntilStep("local user is host", () => MultiplayerClient.Room?.Host?.Equals(MultiplayerClient.LocalUser) == true);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            AddAssert("countdown still active", () => MultiplayerClient.Room?.Countdown != null);
        }

        [Test]
        public void TestCountdownButtonVisibilityWithAutoStartEnablement()
        {
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            AddUntilStep("countdown button visible", () => this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);

            AddStep("enable auto start", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { AutoStartDuration = TimeSpan.FromMinutes(1) }).WaitSafely());

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            AddUntilStep("countdown button not visible", () => !this.ChildrenOfType<MultiplayerCountdownButton>().Single().IsPresent);
        }

        [Test]
        public void TestClickingReadyButtonUnReadiesDuringAutoStart()
        {
            AddStep("enable auto start", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { AutoStartDuration = TimeSpan.FromMinutes(1) }).WaitSafely());
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("local user became idle", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Idle);
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

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));
            AddUntilStep("ready button disabled", () => !readyButton.Enabled.Value);
            AddStep("undelete beatmap", () => beatmaps.Undelete(importedSet));
            AddUntilStep("ready button enabled back", () => readyButton.Enabled.Value);
        }

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            AddStep("add host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddStep("make user host", () => MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0));

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            AddStep("transfer host", () => MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[1].UserID ?? 0));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle (match not started)", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
            AddUntilStep("ready button enabled", () => control.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestManyUsersChangingState(bool isHost)
        {
            const int users = 10;
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);
                for (int i = 0; i < users; i++)
                    MultiplayerClient.AddUser(new APIUser { Id = i, Username = "Another user" });
            });

            if (!isHost)
                AddStep("transfer host", () => MultiplayerClient.TransferHost(2));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddRepeatStep("change user ready state", () =>
            {
                MultiplayerClient.ChangeUserState(RNG.Next(0, users), RNG.NextBool() ? MultiplayerUserState.Ready : MultiplayerUserState.Idle);
            }, 20);

            AddRepeatStep("ready all users", () =>
            {
                var nextUnready = MultiplayerClient.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    MultiplayerClient.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, users);
        }

        private void verifyGameplayStartFlow()
        {
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user waiting for load", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);

            AddStep("finish gameplay", () =>
            {
                MultiplayerClient.ChangeUserState(MultiplayerClient.Room?.Users[0].UserID ?? 0, MultiplayerUserState.Loaded);
                MultiplayerClient.ChangeUserState(MultiplayerClient.Room?.Users[0].UserID ?? 0, MultiplayerUserState.FinishedPlay);
            });

            AddUntilStep("ready button enabled", () => control.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }
    }
}
