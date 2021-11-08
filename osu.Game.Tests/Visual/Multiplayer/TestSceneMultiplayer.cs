// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Spectate;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayer : ScreenTestScene
    {
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;

        private TestMultiplayerScreenStack multiplayerScreenStack;

        private TestMultiplayerClient client => multiplayerScreenStack.Client;
        private TestMultiplayerRoomManager roomManager => multiplayerScreenStack.RoomManager;

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
            });

            AddStep("load multiplayer", () => LoadScreen(multiplayerScreenStack = new TestMultiplayerScreenStack()));
            AddUntilStep("wait for multiplayer to load", () => multiplayerScreenStack.IsLoaded);
            AddUntilStep("wait for lounge to load", () => this.ChildrenOfType<MultiplayerLoungeSubScreen>().FirstOrDefault()?.IsLoaded == true);
        }

        [Test]
        public void TestEmpty()
        {
            // used to test the flow of multiplayer from visual tests.
            AddStep("empty step", () => { });
        }

        [Test]
        public void TestLobbyEvents()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddRepeatStep("random stuff happens", performRandomAction, 30);

            // ensure we have a handful of players so the ready-up sounds good :9
            AddRepeatStep("player joins", addRandomPlayer, 5);

            // all ready
            AddUntilStep("all players ready", () =>
            {
                var nextUnready = client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);

                return client.Room?.Users.All(u => u.State == MultiplayerUserState.Ready) == true;
            });

            AddStep("unready all players at once", () =>
            {
                Debug.Assert(client.Room != null);

                foreach (var u in client.Room.Users) client.ChangeUserState(u.UserID, MultiplayerUserState.Idle);
            });

            AddStep("ready all players at once", () =>
            {
                Debug.Assert(client.Room != null);

                foreach (var u in client.Room.Users) client.ChangeUserState(u.UserID, MultiplayerUserState.Ready);
            });
        }

        private void addRandomPlayer()
        {
            int randomUser = RNG.Next(200000, 500000);
            client.AddUser(new APIUser { Id = randomUser, Username = $"user {randomUser}" });
        }

        private void removeLastUser()
        {
            APIUser lastUser = client.Room?.Users.Last().User;

            if (lastUser == null || lastUser == client.LocalUser?.User)
                return;

            client.RemoveUser(lastUser);
        }

        private void kickLastUser()
        {
            APIUser lastUser = client.Room?.Users.Last().User;

            if (lastUser == null || lastUser == client.LocalUser?.User)
                return;

            client.KickUser(lastUser.Id);
        }

        private void markNextPlayerReady()
        {
            var nextUnready = client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
            if (nextUnready != null)
                client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
        }

        private void markNextPlayerIdle()
        {
            var nextUnready = client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Ready);
            if (nextUnready != null)
                client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Idle);
        }

        private void performRandomAction()
        {
            int eventToPerform = RNG.Next(1, 6);

            switch (eventToPerform)
            {
                case 1:
                    addRandomPlayer();
                    break;

                case 2:
                    removeLastUser();
                    break;

                case 3:
                    kickLastUser();
                    break;

                case 4:
                    markNextPlayerReady();
                    break;

                case 5:
                    markNextPlayerIdle();
                    break;
            }
        }

        [Test]
        public void TestCreateRoomViaKeyboard()
        {
            // create room dialog
            AddStep("Press new document", () => InputManager.Keys(PlatformAction.DocumentNew));
            AddUntilStep("wait for settings", () => InputManager.ChildrenOfType<MultiplayerMatchSettingsOverlay>().FirstOrDefault() != null);

            // edit playlist item
            AddStep("Press select", () => InputManager.Key(Key.Enter));
            AddUntilStep("wait for song select", () => InputManager.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault() != null);

            // select beatmap
            AddStep("Press select", () => InputManager.Key(Key.Enter));
            AddUntilStep("wait for return to screen", () => InputManager.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault() == null);

            // create room
            AddStep("Press select", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.Room != null);
        }

        [Test]
        public void TestCreateRoomWithoutPassword()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddAssert("Check participant count correct", () => client.APIRoom?.ParticipantCount.Value == 1);
            AddAssert("Check participant list contains user", () => client.APIRoom?.RecentParticipants.Count(u => u.Id == API.LocalUser.Value.Id) == 1);
        }

        [Test]
        public void TestExitMidJoin()
        {
            AddStep("create room", () =>
            {
                roomManager.AddServerSideRoom(new Room
                {
                    Name = { Value = "Test Room" },
                    Playlist =
                    {
                        new PlaylistItem
                        {
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                });
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<DrawableRoom>().Any());

            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room and immediately exit select", () =>
            {
                InputManager.Key(Key.Enter);
                Schedule(() => Stack.CurrentScreen.Exit());
            });
        }

        [Test]
        public void TestJoinRoomWithoutPassword()
        {
            AddStep("create room", () =>
            {
                roomManager.AddServerSideRoom(new Room
                {
                    Name = { Value = "Test Room" },
                    Playlist =
                    {
                        new PlaylistItem
                        {
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                });
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<DrawableRoom>().Any());

            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.Room != null);

            AddAssert("Check participant count correct", () => client.APIRoom?.ParticipantCount.Value == 1);
            AddAssert("Check participant list contains user", () => client.APIRoom?.RecentParticipants.Count(u => u.Id == API.LocalUser.Value.Id) == 1);
        }

        [Test]
        public void TestCreateRoomWithPassword()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Password = { Value = "password" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddAssert("room has password", () => client.APIRoom?.Password.Value == "password");
        }

        [Test]
        public void TestJoinRoomWithPassword()
        {
            AddStep("create room", () =>
            {
                roomManager.AddServerSideRoom(new Room
                {
                    Name = { Value = "Test Room" },
                    Password = { Value = "password" },
                    Playlist =
                    {
                        new PlaylistItem
                        {
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                });
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<DrawableRoom>().Any());

            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            DrawableLoungeRoom.PasswordEntryPopover passwordEntryPopover = null;
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<DrawableLoungeRoom.PasswordEntryPopover>().FirstOrDefault()) != null);
            AddStep("enter password in text box", () => passwordEntryPopover.ChildrenOfType<TextBox>().First().Text = "password");
            AddStep("press join room button", () => passwordEntryPopover.ChildrenOfType<OsuButton>().First().TriggerClick());

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.Room != null);
        }

        [Test]
        public void TestLocalPasswordUpdatedWhenMultiplayerSettingsChange()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Password = { Value = "password" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("change password", () => client.ChangeSettings(password: "password2"));
            AddUntilStep("local password changed", () => client.APIRoom?.Password.Value == "password2");
        }

        [Test]
        public void TestUserSetToIdleWhenBeatmapDeleted()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("set user ready", () => client.ChangeState(MultiplayerUserState.Ready));
            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));

            AddUntilStep("user state is idle", () => client.LocalUser?.State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestLocalPlayDoesNotStartWhileSpectatingWithNoBeatmap()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("join other user (ready, host)", () =>
            {
                client.AddUser(new APIUser { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Other" });
                client.TransferHost(MultiplayerTestScene.PLAYER_1_ID);
                client.ChangeUserState(MultiplayerTestScene.PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));

            AddStep("click spectate button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerSpectateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for spectating user state", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("start match externally", () => client.StartMatch());

            AddAssert("play not started", () => multiplayerScreenStack.IsCurrentScreen());
        }

        [Test]
        public void TestLocalPlayStartsWhileSpectatingWhenBeatmapBecomesAvailable()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));

            AddStep("join other user (ready, host)", () =>
            {
                client.AddUser(new APIUser { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Other" });
                client.TransferHost(MultiplayerTestScene.PLAYER_1_ID);
                client.ChangeUserState(MultiplayerTestScene.PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            AddStep("click spectate button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerSpectateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for spectating user state", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("start match externally", () => client.StartMatch());

            AddStep("restore beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
            });

            AddUntilStep("play started", () => multiplayerScreenStack.CurrentScreen is SpectatorScreen);
        }

        [Test]
        public void TestSubScreenExitedWhenDisconnectedFromMultiplayerServer()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("disconnect", () => client.Disconnect());
            AddUntilStep("back in lounge", () => this.ChildrenOfType<LoungeSubScreen>().FirstOrDefault()?.IsCurrentScreen() == true);
        }

        [Test]
        public void TestLeaveNavigation()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        AllowedMods = { new OsuModHidden() }
                    }
                }
            });

            AddStep("open mod overlay", () => this.ChildrenOfType<RoomSubScreen.UserModSelectButton>().Single().TriggerClick());

            AddStep("invoke on back button", () => multiplayerScreenStack.OnBackButton());

            AddAssert("mod overlay is hidden", () => this.ChildrenOfType<UserModSelectOverlay>().Single().State.Value == Visibility.Hidden);

            AddAssert("dialog overlay is hidden", () => DialogOverlay.State.Value == Visibility.Hidden);

            testLeave("back button", () => multiplayerScreenStack.OnBackButton());

            // mimics home button and OS window close
            testLeave("forced exit", () => multiplayerScreenStack.Exit());

            void testLeave(string actionName, Action action)
            {
                AddStep($"leave via {actionName}", action);

                AddAssert("dialog overlay is visible", () => DialogOverlay.State.Value == Visibility.Visible);

                AddStep("close dialog overlay", () => InputManager.Key(Key.Escape));
            }
        }

        [Test]
        public void TestGameplayFlow()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddUntilStep("wait for ready button to be enabled", () => readyButton.ChildrenOfType<OsuButton>().Single().Enabled.Value);

            AddStep("click ready button", () =>
            {
                InputManager.MoveMouseTo(readyButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for player to be ready", () => client.Room?.Users[0].State == MultiplayerUserState.Ready);
            AddUntilStep("wait for ready button to be enabled", () => readyButton.ChildrenOfType<OsuButton>().Single().Enabled.Value);

            AddStep("click start button", () => InputManager.Click(MouseButton.Left));

            AddUntilStep("wait for player", () => multiplayerScreenStack.CurrentScreen is Player);

            // Gameplay runs in real-time, so we need to incrementally check if gameplay has finished in order to not time out.
            for (double i = 1000; i < TestResources.QUICK_BEATMAP_LENGTH; i += 1000)
            {
                double time = i;
                AddUntilStep($"wait for time > {i}", () => this.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.GameplayClock.CurrentTime > time);
            }

            AddUntilStep("wait for results", () => multiplayerScreenStack.CurrentScreen is ResultsScreen);
        }

        private MultiplayerReadyButton readyButton => this.ChildrenOfType<MultiplayerReadyButton>().Single();

        private void createRoom(Func<Room> room)
        {
            AddUntilStep("wait for lounge", () => multiplayerScreenStack.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => multiplayerScreenStack.ChildrenOfType<LoungeSubScreen>().Single().Open(room()));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => client.Room != null);
        }
    }
}
