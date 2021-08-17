// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayer : ScreenTestScene
    {
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;

        private DependenciesScreen dependenciesScreen;
        private TestMultiplayer multiplayerScreen;
        private TestMultiplayerClient client;

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

            AddStep("create multiplayer screen", () => multiplayerScreen = new TestMultiplayer());

            AddStep("load dependencies", () =>
            {
                client = new TestMultiplayerClient(multiplayerScreen.RoomManager);

                // The screen gets suspended so it stops receiving updates.
                Child = client;

                LoadScreen(dependenciesScreen = new DependenciesScreen(client));
            });

            AddUntilStep("wait for dependencies to load", () => dependenciesScreen.IsLoaded);

            AddStep("load multiplayer", () => LoadScreen(multiplayerScreen));
            AddUntilStep("wait for multiplayer to load", () => multiplayerScreen.IsLoaded);
            AddUntilStep("wait for lounge to load", () => this.ChildrenOfType<MultiplayerLoungeSubScreen>().FirstOrDefault()?.IsLoaded == true);
        }

        [Test]
        public void TestEmpty()
        {
            // used to test the flow of multiplayer from visual tests.
            AddStep("empty step", () => { });
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
        }

        [Test]
        public void TestExitMidJoin()
        {
            Room room = null;

            AddStep("create room", () =>
            {
                room = new Room
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
                };
            });

            AddStep("refresh rooms", () => multiplayerScreen.RoomManager.Filter.Value = new FilterCriteria());
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room and immediately exit", () =>
            {
                multiplayerScreen.ChildrenOfType<LoungeSubScreen>().Single().Open(room);
                Schedule(() => Stack.CurrentScreen.Exit());
            });
        }

        [Test]
        public void TestJoinRoomWithoutPassword()
        {
            AddStep("create room", () =>
            {
                multiplayerScreen.RoomManager.AddRoom(new Room
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

            AddStep("refresh rooms", () => multiplayerScreen.RoomManager.Filter.Value = new FilterCriteria());
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => client.Room != null);
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
                multiplayerScreen.RoomManager.AddRoom(new Room
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

            AddStep("refresh rooms", () => multiplayerScreen.RoomManager.Filter.Value = new FilterCriteria());
            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            DrawableRoom.PasswordEntryPopover passwordEntryPopover = null;
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<DrawableRoom.PasswordEntryPopover>().FirstOrDefault()) != null);
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
                client.AddUser(new User { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Other" });
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

            AddAssert("play not started", () => multiplayerScreen.IsCurrentScreen());
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
                client.AddUser(new User { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Other" });
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

            AddUntilStep("play started", () => !multiplayerScreen.IsCurrentScreen());
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

            AddStep("invoke on back button", () => multiplayerScreen.OnBackButton());

            AddAssert("mod overlay is hidden", () => this.ChildrenOfType<LocalPlayerModSelectOverlay>().Single().State.Value == Visibility.Hidden);

            AddAssert("dialog overlay is hidden", () => DialogOverlay.State.Value == Visibility.Hidden);

            testLeave("back button", () => multiplayerScreen.OnBackButton());

            // mimics home button and OS window close
            testLeave("forced exit", () => multiplayerScreen.Exit());

            void testLeave(string actionName, Action action)
            {
                AddStep($"leave via {actionName}", action);

                AddAssert("dialog overlay is visible", () => DialogOverlay.State.Value == Visibility.Visible);

                AddStep("close dialog overlay", () => InputManager.Key(Key.Escape));
            }
        }

        private void createRoom(Func<Room> room)
        {
            AddUntilStep("wait for lounge", () => multiplayerScreen.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => multiplayerScreen.ChildrenOfType<LoungeSubScreen>().Single().Open(room()));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => client.Room != null);
        }

        /// <summary>
        /// Used for the sole purpose of adding <see cref="TestMultiplayerClient"/> as a resolvable dependency.
        /// </summary>
        private class DependenciesScreen : OsuScreen
        {
            [Cached(typeof(MultiplayerClient))]
            public readonly TestMultiplayerClient Client;

            public DependenciesScreen(TestMultiplayerClient client)
            {
                Client = client;
            }
        }

        private class TestMultiplayer : Screens.OnlinePlay.Multiplayer.Multiplayer
        {
            public new TestRequestHandlingMultiplayerRoomManager RoomManager { get; private set; }

            protected override RoomManager CreateRoomManager() => RoomManager = new TestRequestHandlingMultiplayerRoomManager();
        }
    }
}
