// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
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
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Screens.OnlinePlay.Multiplayer.Spectate;
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

        private TestMultiplayerComponents multiplayerComponents;

        private TestMultiplayerClient client => multiplayerComponents.Client;
        private TestMultiplayerRoomManager roomManager => multiplayerComponents.RoomManager;

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, API, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            });

            AddStep("load multiplayer", () => LoadScreen(multiplayerComponents = new TestMultiplayerComponents()));
            AddUntilStep("wait for multiplayer to load", () => multiplayerComponents.IsLoaded);
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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
            AddUntilStep("wait for song select", () => InputManager.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault()?.BeatmapSetsLoaded == true);

            // select beatmap
            AddStep("Press select", () => InputManager.Key(Key.Enter));
            AddUntilStep("wait for return to screen", () => InputManager.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault() == null);

            // create room
            AddStep("Press select", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.RoomJoined);
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                }, API.LocalUser.Value);
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
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                }, API.LocalUser.Value);
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<DrawableRoom>().Any());

            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.RoomJoined);

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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                }, API.LocalUser.Value);
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
            AddUntilStep("wait for join", () => client.RoomJoined);
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            pressReadyButton();

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));
            AddUntilStep("user state is idle", () => client.LocalUser?.State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestPlayStartsWithCorrectBeatmapWhileAtSongSelect()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            pressReadyButton();

            AddStep("Enter song select", () =>
            {
                var currentSubScreen = ((Screens.OnlinePlay.Multiplayer.Multiplayer)multiplayerComponents.CurrentScreen).CurrentSubScreen;
                ((MultiplayerMatchSubScreen)currentSubScreen).OpenSongSelection(client.Room?.Settings.PlaylistItemId);
            });

            AddUntilStep("wait for song select", () => this.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault()?.BeatmapSetsLoaded == true);

            AddAssert("Beatmap matches current item", () => Beatmap.Value.BeatmapInfo.OnlineID == client.Room?.Playlist.First().BeatmapID);

            AddStep("Select next beatmap", () => InputManager.Key(Key.Down));

            AddUntilStep("Beatmap doesn't match current item", () => Beatmap.Value.BeatmapInfo.OnlineID != client.Room?.Playlist.First().BeatmapID);

            AddStep("start match externally", () => client.StartMatch());

            AddUntilStep("play started", () => multiplayerComponents.CurrentScreen is Player);

            AddAssert("Beatmap matches current item", () => Beatmap.Value.BeatmapInfo.OnlineID == client.Room?.Playlist.First().BeatmapID);
        }

        [Test]
        public void TestPlayStartsWithCorrectRulesetWhileAtSongSelect()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            pressReadyButton();

            AddStep("Enter song select", () =>
            {
                var currentSubScreen = ((Screens.OnlinePlay.Multiplayer.Multiplayer)multiplayerComponents.CurrentScreen).CurrentSubScreen;
                ((MultiplayerMatchSubScreen)currentSubScreen).OpenSongSelection(client.Room?.Settings.PlaylistItemId);
            });

            AddUntilStep("wait for song select", () => this.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault()?.BeatmapSetsLoaded == true);

            AddAssert("Ruleset matches current item", () => Ruleset.Value.OnlineID == client.Room?.Playlist.First().RulesetID);

            AddStep("Switch ruleset", () => ((MultiplayerMatchSongSelect)multiplayerComponents.MultiplayerScreen.CurrentSubScreen).Ruleset.Value = new CatchRuleset().RulesetInfo);

            AddUntilStep("Ruleset doesn't match current item", () => Ruleset.Value.OnlineID != client.Room?.Playlist.First().RulesetID);

            AddStep("start match externally", () => client.StartMatch());

            AddUntilStep("play started", () => multiplayerComponents.CurrentScreen is Player);

            AddAssert("Ruleset matches current item", () => Ruleset.Value.OnlineID == client.Room?.Playlist.First().RulesetID);
        }

        [Test]
        public void TestPlayStartsWithCorrectModsWhileAtSongSelect()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            pressReadyButton();

            AddStep("Enter song select", () =>
            {
                var currentSubScreen = ((Screens.OnlinePlay.Multiplayer.Multiplayer)multiplayerComponents.CurrentScreen).CurrentSubScreen;
                ((MultiplayerMatchSubScreen)currentSubScreen).OpenSongSelection(client.Room?.Settings.PlaylistItemId);
            });

            AddUntilStep("wait for song select", () => this.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault()?.BeatmapSetsLoaded == true);

            AddAssert("Mods match current item", () => SelectedMods.Value.Select(m => m.Acronym).SequenceEqual(client.Room.AsNonNull().Playlist.First().RequiredMods.Select(m => m.Acronym)));

            AddStep("Switch required mods", () => ((MultiplayerMatchSongSelect)multiplayerComponents.MultiplayerScreen.CurrentSubScreen).Mods.Value = new Mod[] { new OsuModDoubleTime() });

            AddAssert("Mods don't match current item", () => !SelectedMods.Value.Select(m => m.Acronym).SequenceEqual(client.Room.AsNonNull().Playlist.First().RequiredMods.Select(m => m.Acronym)));

            AddStep("start match externally", () => client.StartMatch());

            AddUntilStep("play started", () => multiplayerComponents.CurrentScreen is Player);

            AddAssert("Mods match current item", () => SelectedMods.Value.Select(m => m.Acronym).SequenceEqual(client.Room.AsNonNull().Playlist.First().RequiredMods.Select(m => m.Acronym)));
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();

            AddUntilStep("wait for spectating user state", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("start match externally", () => client.StartMatch());

            AddAssert("play not started", () => multiplayerComponents.IsCurrentScreen());
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();

            AddUntilStep("wait for spectating user state", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("start match externally", () => client.StartMatch());

            AddStep("restore beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            });

            AddUntilStep("play started", () => multiplayerComponents.CurrentScreen is SpectatorScreen);
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        AllowedMods = { new OsuModHidden() }
                    }
                }
            });

            AddStep("open mod overlay", () => this.ChildrenOfType<RoomSubScreen.UserModSelectButton>().Single().TriggerClick());

            AddStep("invoke on back button", () => multiplayerComponents.OnBackButton());

            AddAssert("mod overlay is hidden", () => this.ChildrenOfType<UserModSelectOverlay>().Single().State.Value == Visibility.Hidden);

            AddAssert("dialog overlay is hidden", () => DialogOverlay.State.Value == Visibility.Hidden);

            testLeave("back button", () => multiplayerComponents.OnBackButton());

            // mimics home button and OS window close
            testLeave("forced exit", () => multiplayerComponents.Exit());

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
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            enterGameplay();

            // Gameplay runs in real-time, so we need to incrementally check if gameplay has finished in order to not time out.
            for (double i = 1000; i < TestResources.QUICK_BEATMAP_LENGTH; i += 1000)
            {
                double time = i;
                AddUntilStep($"wait for time > {i}", () => this.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.GameplayClock.CurrentTime > time);
            }

            AddUntilStep("wait for results", () => multiplayerComponents.CurrentScreen is ResultsScreen);
        }

        [Test]
        public void TestRoomSettingsReQueriedWhenJoiningRoom()
        {
            AddStep("create room", () =>
            {
                roomManager.AddServerSideRoom(new Room
                {
                    Name = { Value = "Test Room" },
                    QueueMode = { Value = QueueMode.AllPlayers },
                    Playlist =
                    {
                        new PlaylistItem
                        {
                            Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                }, API.LocalUser.Value);
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<DrawableRoom>().Any());
            AddStep("select room", () => InputManager.Key(Key.Down));

            AddStep("disable polling", () => this.ChildrenOfType<ListingPollingComponent>().Single().TimeBetweenPolls.Value = 0);
            AddStep("change server-side settings", () =>
            {
                roomManager.ServerSideRooms[0].Name.Value = "New name";
                roomManager.ServerSideRooms[0].Playlist.Add(new PlaylistItem
                {
                    ID = 2,
                    Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddStep("join room", () => InputManager.Key(Key.Enter));
            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.RoomJoined);

            AddAssert("local room has correct settings", () =>
            {
                var localRoom = this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().Room;
                return localRoom.Name.Value == roomManager.ServerSideRooms[0].Name.Value
                       && localRoom.Playlist.SequenceEqual(roomManager.ServerSideRooms[0].Playlist);
            });
        }

        [Test]
        public void TestSpectatingStateResetOnBackButtonDuringGameplay()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                QueueMode = { Value = QueueMode.AllPlayers },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("set spectating state", () => client.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("state set to spectating", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("join other user", () => client.AddUser(new APIUser { Id = 1234 }));
            AddStep("set other user ready", () => client.ChangeUserState(1234, MultiplayerUserState.Ready));

            pressReadyButton(1234);
            AddUntilStep("wait for gameplay", () => (multiplayerComponents.CurrentScreen as MultiSpectatorScreen)?.IsLoaded == true);

            AddStep("press back button and exit", () =>
            {
                multiplayerComponents.OnBackButton();
                multiplayerComponents.Exit();
            });

            AddUntilStep("wait for return to match subscreen", () => multiplayerComponents.MultiplayerScreen.IsCurrentScreen());
            AddUntilStep("user state is idle", () => client.LocalUser?.State == MultiplayerUserState.Idle);
        }

        [Test]
        public void TestSpectatingStateNotResetOnBackButtonOutsideOfGameplay()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                QueueMode = { Value = QueueMode.AllPlayers },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("set spectating state", () => client.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("state set to spectating", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("join other user", () => client.AddUser(new APIUser { Id = 1234 }));
            AddStep("set other user ready", () => client.ChangeUserState(1234, MultiplayerUserState.Ready));

            pressReadyButton(1234);
            AddUntilStep("wait for gameplay", () => (multiplayerComponents.CurrentScreen as MultiSpectatorScreen)?.IsLoaded == true);
            AddStep("set other user loaded", () => client.ChangeUserState(1234, MultiplayerUserState.Loaded));
            AddStep("set other user finished play", () => client.ChangeUserState(1234, MultiplayerUserState.FinishedPlay));

            AddStep("press back button and exit", () =>
            {
                multiplayerComponents.OnBackButton();
                multiplayerComponents.Exit();
            });

            AddUntilStep("wait for return to match subscreen", () => multiplayerComponents.MultiplayerScreen.IsCurrentScreen());
            AddWaitStep("wait for possible state change", 5);
            AddUntilStep("user state is spectating", () => client.LocalUser?.State == MultiplayerUserState.Spectating);
        }

        [Test]
        public void TestItemAddedByOtherUserDuringGameplay()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                QueueMode = { Value = QueueMode.AllPlayers },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            enterGameplay();

            AddStep("join other user", () => client.AddUser(new APIUser { Id = 1234 }));
            AddStep("add item as other user", () => client.AddUserPlaylistItem(1234, new MultiplayerPlaylistItem(new PlaylistItem
            {
                BeatmapID = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo.OnlineID
            })));

            AddUntilStep("item arrived in playlist", () => client.Room?.Playlist.Count == 2);

            AddStep("exit gameplay as initial user", () => multiplayerComponents.MultiplayerScreen.MakeCurrent());
            AddUntilStep("queue contains item", () => this.ChildrenOfType<MultiplayerQueueList>().Single().Items.Single().ID == 2);
        }

        [Test]
        public void TestItemAddedAndDeletedByOtherUserDuringGameplay()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                QueueMode = { Value = QueueMode.AllPlayers },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            enterGameplay();

            AddStep("join other user", () => client.AddUser(new APIUser { Id = 1234 }));
            AddStep("add item as other user", () => client.AddUserPlaylistItem(1234, new MultiplayerPlaylistItem(new PlaylistItem
            {
                BeatmapID = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo.OnlineID
            })));

            AddUntilStep("item arrived in playlist", () => client.Room?.Playlist.Count == 2);

            AddStep("delete item as other user", () => client.RemoveUserPlaylistItem(1234, 2));
            AddUntilStep("item removed from playlist", () => client.Room?.Playlist.Count == 1);

            AddStep("exit gameplay as initial user", () => multiplayerComponents.MultiplayerScreen.MakeCurrent());
            AddUntilStep("queue is empty", () => this.ChildrenOfType<MultiplayerQueueList>().Single().Items.Count == 0);
        }

        [Test]
        public void TestGameplayStartsWhileInSpectatorScreen()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddStep("join other user and make host", () =>
            {
                client.AddUser(new APIUser { Id = 1234 });
                client.TransferHost(1234);
            });

            AddStep("set local user spectating", () => client.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("wait for spectating state", () => client.LocalUser?.State == MultiplayerUserState.Spectating);

            runGameplay();

            AddStep("exit gameplay for other user", () => client.ChangeUserState(1234, MultiplayerUserState.Idle));
            AddUntilStep("wait for room to be idle", () => client.Room?.State == MultiplayerRoomState.Open);

            runGameplay();

            void runGameplay()
            {
                AddStep("start match by other user", () =>
                {
                    client.ChangeUserState(1234, MultiplayerUserState.Ready);
                    client.StartMatch();
                });

                AddUntilStep("wait for loading", () => client.Room?.State == MultiplayerRoomState.WaitingForLoad);
                AddStep("set player loaded", () => client.ChangeUserState(1234, MultiplayerUserState.Loaded));
                AddUntilStep("wait for gameplay to start", () => client.Room?.State == MultiplayerRoomState.Playing);
                AddUntilStep("wait for local user to enter spectator", () => multiplayerComponents.CurrentScreen is MultiSpectatorScreen);
            }
        }

        private void enterGameplay()
        {
            pressReadyButton();
            pressReadyButton();
            AddUntilStep("wait for player", () => multiplayerComponents.CurrentScreen is Player);
        }

        private ReadyButton readyButton => this.ChildrenOfType<ReadyButton>().Single();

        private void pressReadyButton(int? playingUserId = null)
        {
            // Can't use ClickButtonWhenEnabled<> due to needing to store the state after the button is enabled.

            AddUntilStep("wait for ready button to be enabled", () => readyButton.Enabled.Value);

            MultiplayerUserState lastState = MultiplayerUserState.Idle;
            MultiplayerRoomUser user = null;

            AddStep("click ready button", () =>
            {
                user = playingUserId == null ? client.LocalUser : client.Room?.Users.Single(u => u.UserID == playingUserId);
                lastState = user?.State ?? MultiplayerUserState.Idle;

                InputManager.MoveMouseTo(readyButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for state change", () => user?.State != lastState);
        }

        private void createRoom(Func<Room> room)
        {
            AddUntilStep("wait for lounge", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().Single().Open(room()));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => client.RoomJoined);
        }
    }
}
