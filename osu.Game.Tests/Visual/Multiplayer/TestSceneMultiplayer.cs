// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
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
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay;
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
using ReadyButton = osu.Game.Screens.OnlinePlay.Components.ReadyButton;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayer : MultiplayerTestScene
    {
        private RulesetStore rulesets = null!;
        private BeatmapManager beatmaps = null!;
        private BeatmapSetInfo importedSet = null!;
        private BeatmapSetInfo importedSet2 = null!;

        private Screens.OnlinePlay.Multiplayer.Multiplayer screen = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            BeatmapStore beatmapStore;

            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, API, audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());
            Dependencies.Cache(Realm);

            Add(beatmapStore);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
                importedSet2 = beatmaps.Import(CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo.BeatmapSet!)!.Value.Detach();

                Realm.Write(r =>
                {
                    foreach (var beatmapInfo in r.All<BeatmapInfo>())
                        beatmapInfo.OnlineMD5Hash = beatmapInfo.MD5Hash;
                });
            });

            AddStep("load multiplayer", () => LoadScreen(screen = new Screens.OnlinePlay.Multiplayer.Multiplayer()));
            AddUntilStep("wait for multiplayer to load", () => screen.IsLoaded);
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
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddRepeatStep("random stuff happens", performRandomAction, 30);

            // ensure we have a handful of players so the ready-up sounds good :9
            AddRepeatStep("player joins", addRandomPlayer, 5);

            // all ready
            AddUntilStep("all players ready", () =>
            {
                var nextUnready = MultiplayerClient.ClientRoom?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    MultiplayerClient.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);

                return MultiplayerClient.ClientRoom?.Users.All(u => u.State == MultiplayerUserState.Ready) == true;
            });

            AddStep("unready all players at once", () =>
            {
                Debug.Assert(MultiplayerClient.ServerRoom != null);

                foreach (var u in MultiplayerClient.ServerRoom.Users) MultiplayerClient.ChangeUserState(u.UserID, MultiplayerUserState.Idle);
            });

            AddStep("ready all players at once", () =>
            {
                Debug.Assert(MultiplayerClient.ServerRoom != null);

                foreach (var u in MultiplayerClient.ServerRoom.Users) MultiplayerClient.ChangeUserState(u.UserID, MultiplayerUserState.Ready);
            });
        }

        private void addRandomPlayer()
        {
            int id = TestResources.GetNextTestID();
            MultiplayerClient.AddUser(new APIUser { Id = id, Username = $"user {id}" });
        }

        private void removeLastUser()
        {
            APIUser? lastUser = MultiplayerClient.ServerRoom?.Users.Last().User;

            if (lastUser == null || lastUser == MultiplayerClient.LocalUser?.User)
                return;

            MultiplayerClient.RemoveUser(lastUser);
        }

        private void kickLastUser()
        {
            APIUser? lastUser = MultiplayerClient.ServerRoom?.Users.Last().User;

            if (lastUser == null || lastUser == MultiplayerClient.LocalUser?.User)
                return;

            MultiplayerClient.KickUser(lastUser.Id);
        }

        private void markNextPlayerReady()
        {
            var nextUnready = MultiplayerClient.ServerRoom?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
            if (nextUnready != null)
                MultiplayerClient.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
        }

        private void markNextPlayerIdle()
        {
            var nextUnready = MultiplayerClient.ServerRoom?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Ready);
            if (nextUnready != null)
                MultiplayerClient.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Idle);
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
            waitForSongSelect();

            // select beatmap
            AddStep("Press select", () => InputManager.Key(Key.Enter));
            AddUntilStep("wait for return to screen", () => InputManager.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault() == null);

            // create room
            AddStep("Press select", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => MultiplayerClient.RoomJoined);
        }

        [Test]
        public void TestCreateRoomWithoutPassword()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddUntilStep("Check participant count correct", () => MultiplayerClient.ClientAPIRoom?.ParticipantCount == 1);
            AddUntilStep("Check participant list contains user", () => MultiplayerClient.ClientAPIRoom?.RecentParticipants.Count(u => u.Id == API.LocalUser.Value.Id) == 1);
        }

        [Test]
        public void TestExitMidJoin()
        {
            AddStep("create room", () =>
            {
                MultiplayerClient.AddServerSideRoom(new Room
                {
                    Name = "Test Room",
                    Playlist =
                    [
                        new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                        }
                    ]
                }, API.LocalUser.Value);
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<RoomPanel>().Any());

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
                MultiplayerClient.AddServerSideRoom(new Room
                {
                    Name = "Test Room",
                    Playlist =
                    [
                        new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                        }
                    ]
                }, API.LocalUser.Value);
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<RoomPanel>().Any());

            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => MultiplayerClient.RoomJoined);

            AddUntilStep("Check participant count correct", () => MultiplayerClient.ClientAPIRoom?.ParticipantCount == 1);
            AddUntilStep("Check participant list contains user", () => MultiplayerClient.ClientAPIRoom?.RecentParticipants.Count(u => u.Id == API.LocalUser.Value.Id) == 1);
        }

        [Test]
        public void TestCreateRoomWithPassword()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Password = "password",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddUntilStep("room has password", () => MultiplayerClient.ClientAPIRoom?.Password == "password");
        }

        [Test]
        public void TestJoinRoomWithPassword()
        {
            AddStep("create room", () =>
            {
                MultiplayerClient.AddServerSideRoom(new Room
                {
                    Name = "Test Room",
                    Password = "password",
                    Playlist =
                    [
                        new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                        }
                    ]
                }, API.LocalUser.Value);
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<RoomPanel>().Any());

            AddStep("select room", () => InputManager.Key(Key.Down));
            AddStep("join room", () => InputManager.Key(Key.Enter));

            LoungeRoomPanel.PasswordEntryPopover? passwordEntryPopover = null;
            AddUntilStep("password prompt appeared", () => (passwordEntryPopover = InputManager.ChildrenOfType<LoungeRoomPanel.PasswordEntryPopover>().FirstOrDefault()) != null);
            AddStep("enter password in text box", () => passwordEntryPopover.ChildrenOfType<TextBox>().First().Text = "password");
            AddStep("press join room button", () => passwordEntryPopover.ChildrenOfType<OsuButton>().First().TriggerClick());

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => MultiplayerClient.RoomJoined);
        }

        [Test]
        public void TestLocalPasswordUpdatedWhenMultiplayerSettingsChange()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Password = "password",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddStep("change password", () => MultiplayerClient.ChangeSettings(password: "password2"));
            AddUntilStep("local password changed", () => MultiplayerClient.ClientAPIRoom?.Password == "password2");
        }

        [Test]
        [FlakyTest]
        /*
         * On a slight investigation, this is occurring due to the ready button
         * not receiving the click input generated by the manual input manager.
         *
         * TearDown : System.TimeoutException : "wait for ready button to be enabled" timed out
         *   --TearDown
         *      at osu.Framework.Testing.Drawables.Steps.UntilStepButton.<>c__DisplayClass11_0.<.ctor>b__0()
         *      at osu.Framework.Testing.Drawables.Steps.StepButton.PerformStep(Boolean userTriggered)
         *      at osu.Framework.Testing.TestScene.runNextStep(Action onCompletion, Action`1 onError, Func`2 stopCondition)
         */
        public void TestUserSetToIdleWhenBeatmapDeleted()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            pressReadyButton();

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));
            AddUntilStep("user state is idle", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Idle);
        }

        [Test]
        [FlakyTest] // See above
        public void TestPlayStartsWithCorrectBeatmapWhileAtSongSelect()
        {
            PlaylistItem? item = null;
            createRoom(() =>
            {
                item = new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                };
                return new Room
                {
                    Name = "Test Room",
                    Playlist = [item]
                };
            });

            pressReadyButton();

            AddStep("Enter song select", () => ((MultiplayerMatchSubScreen)screen.CurrentSubScreen).ShowSongSelect(item));

            waitForSongSelect();

            AddUntilStep("Beatmap matches current item", () => Beatmap.Value.BeatmapInfo.OnlineID == MultiplayerClient.ClientRoom?.Playlist.First().BeatmapID);

            AddStep("Select next beatmap", () => InputManager.Key(Key.Down));

            AddUntilStep("Beatmap doesn't match current item", () => Beatmap.Value.BeatmapInfo.OnlineID != MultiplayerClient.ClientRoom?.Playlist.First().BeatmapID);

            AddStep("start match externally", () => MultiplayerClient.StartMatch().WaitSafely());

            AddUntilStep("play started", () => Stack.CurrentScreen is Player);

            AddUntilStep("Beatmap matches current item", () => Beatmap.Value.BeatmapInfo.OnlineID == MultiplayerClient.ClientRoom?.Playlist.First().BeatmapID);
        }

        [Test]
        [FlakyTest] // See above
        public void TestPlayStartsWithCorrectRulesetWhileAtSongSelect()
        {
            PlaylistItem? item = null;
            createRoom(() =>
            {
                item = new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                };
                return new Room
                {
                    Name = "Test Room",
                    Playlist = [item]
                };
            });

            pressReadyButton();

            AddStep("Enter song select", () => ((MultiplayerMatchSubScreen)screen.CurrentSubScreen).ShowSongSelect(item));

            waitForSongSelect();

            AddUntilStep("Ruleset matches current item", () => Ruleset.Value.OnlineID == MultiplayerClient.ClientRoom?.Playlist.First().RulesetID);

            AddStep("Switch ruleset", () => ((MultiplayerMatchSongSelect)screen.CurrentSubScreen).Ruleset.Value = new CatchRuleset().RulesetInfo);

            AddUntilStep("Ruleset doesn't match current item", () => Ruleset.Value.OnlineID != MultiplayerClient.ClientRoom?.Playlist.First().RulesetID);

            AddStep("start match externally", () => MultiplayerClient.StartMatch().WaitSafely());

            AddUntilStep("play started", () => Stack.CurrentScreen is Player);

            AddUntilStep("Ruleset matches current item", () => Ruleset.Value.OnlineID == MultiplayerClient.ClientRoom?.Playlist.First().RulesetID);
        }

        [Test]
        [FlakyTest] // See above
        public void TestPlayStartsWithCorrectModsWhileAtSongSelect()
        {
            PlaylistItem? item = null;
            createRoom(() =>
            {
                item = new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                };
                return new Room
                {
                    Name = "Test Room",
                    Playlist = [item]
                };
            });

            pressReadyButton();

            AddStep("Enter song select", () => ((MultiplayerMatchSubScreen)screen.CurrentSubScreen).ShowSongSelect(item));

            waitForSongSelect();

            AddUntilStep("Mods match current item",
                () => SelectedMods.Value.Select(m => m.Acronym).SequenceEqual(MultiplayerClient.ClientRoom.AsNonNull().Playlist.First().RequiredMods.Select(m => m.Acronym)));

            AddStep("Switch required mods", () => ((MultiplayerMatchSongSelect)screen.CurrentSubScreen).Mods.Value = new Mod[] { new OsuModDoubleTime() });

            AddUntilStep("Mods don't match current item",
                () => !SelectedMods.Value.Select(m => m.Acronym).SequenceEqual(MultiplayerClient.ClientRoom.AsNonNull().Playlist.First().RequiredMods.Select(m => m.Acronym)));

            AddStep("start match externally", () => MultiplayerClient.StartMatch().WaitSafely());

            AddUntilStep("play started", () => Stack.CurrentScreen is Player);

            AddUntilStep("Mods match current item",
                () => SelectedMods.Value.Select(m => m.Acronym).SequenceEqual(MultiplayerClient.ClientRoom.AsNonNull().Playlist.First().RequiredMods.Select(m => m.Acronym)));
        }

        [Test]
        public void TestLocalPlayDoesNotStartWhileSpectatingWithNoBeatmap()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddStep("join other user (ready, host)", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID, Username = "Other" });
                MultiplayerClient.TransferHost(PLAYER_1_ID);
                MultiplayerClient.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();

            AddUntilStep("wait for spectating user state", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("start match externally", () => MultiplayerClient.StartMatch().WaitSafely());

            AddAssert("play not started", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestLocalPlayStartsWhileSpectatingWhenBeatmapBecomesAvailable()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));

            AddStep("join other user (ready, host)", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID, Username = "Other" });
                MultiplayerClient.TransferHost(PLAYER_1_ID);
                MultiplayerClient.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();

            AddUntilStep("wait for spectating user state", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("start match externally", () => MultiplayerClient.StartMatch().WaitSafely());

            AddStep("restore beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            });

            AddUntilStep("play started", () => Stack.CurrentScreen is SpectatorScreen);
        }

        [Test]
        public void TestSubScreenExitedWhenDisconnectedFromMultiplayerServer()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddStep("disconnect", () => MultiplayerClient.Disconnect());
            AddUntilStep("back in lounge", () => this.ChildrenOfType<LoungeSubScreen>().FirstOrDefault()?.IsCurrentScreen() == true);
        }

        [Test]
        public void TestLeaveNavigation()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod(new OsuModHidden()) }
                    }
                ]
            });

            AddStep("open mod overlay", () => this.ChildrenOfType<UserModSelectButton>().Single().TriggerClick());

            AddStep("invoke on back button", () => screen.OnBackButton());

            AddAssert("mod overlay is hidden", () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().All(o => o.State.Value == Visibility.Hidden));

            AddAssert("dialog overlay is hidden", () => DialogOverlay.State.Value == Visibility.Hidden);

            testLeave("back button", () => screen.OnBackButton());

            // mimics home button and OS window close
            testLeave("forced exit", () => screen.Exit());

            void testLeave(string actionName, Action action)
            {
                AddStep($"leave via {actionName}", action);

                AddAssert("dialog overlay is visible", () => DialogOverlay.State.Value == Visibility.Visible);

                AddStep("close dialog overlay", () => InputManager.Key(Key.Escape));
            }
        }

        [Test]
        [FlakyTest] // See above
        public void TestGameplayFlow()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            enterGameplay();

            // Gameplay runs in real-time, so we need to incrementally check if gameplay has finished in order to not time out.
            for (double i = 1000; i < TestResources.QUICK_BEATMAP_LENGTH; i += 1000)
            {
                double time = i;
                AddUntilStep($"wait for time > {i}", () => this.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.CurrentTime > time);
            }

            AddUntilStep("wait for results", () => Stack.CurrentScreen is ResultsScreen);

            AddAssert("check is fail", () =>
            {
                var scoreInfo = ((ResultsScreen)Stack.CurrentScreen).Score;

                return scoreInfo?.Passed == false && scoreInfo.Rank == ScoreRank.F;
            });
        }

        [Test]
        [Ignore("Failing too often, needs revisiting in some future.")]
        // This test is failing even after 10 retries (see https://github.com/ppy/osu/actions/runs/6700910613/job/18208272419)
        // Something is stopping the ready button from changing states, over multiple runs.
        public void TestGameplayExitFlow()
        {
            Bindable<double>? holdDelay = null;

            AddStep("Set hold delay to zero", () =>
            {
                holdDelay = config.GetBindable<double>(OsuSetting.UIHoldActivationDelay);
                holdDelay.Value = 0;
            });

            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            enterGameplay();

            AddUntilStep("wait for playing", () => this.ChildrenOfType<Player>().FirstOrDefault()?.LocalUserPlaying.Value == true);

            AddStep("attempt exit without hold", () => InputManager.Key(Key.Escape));
            AddAssert("still in gameplay", () => Stack.CurrentScreen is Player);

            AddStep("attempt exit with hold", () => InputManager.PressKey(Key.Escape));
            AddUntilStep("wait for lounge", () => Stack.CurrentScreen is Screens.OnlinePlay.Multiplayer.Multiplayer);

            AddStep("stop holding", () => InputManager.ReleaseKey(Key.Escape));
            AddStep("set hold delay to default", () => holdDelay?.SetDefault());
        }

        [Test]
        [FlakyTest] // See above
        public void TestGameplayDoesntStartWithNonLoadedUser()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            pressReadyButton();

            AddStep("join other user and ready", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 1234 });
                MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Ready);
            });

            AddStep("start match", () =>
            {
                MultiplayerClient.StartMatch();
            });

            AddUntilStep("wait for player", () => Stack.CurrentScreen is Player);

            AddWaitStep("wait some", 20);

            AddAssert("ensure gameplay hasn't started", () => this.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.IsRunning == false);
        }

        [Test]
        public void TestRoomSettingsReQueriedWhenJoiningRoom()
        {
            AddStep("create room", () =>
            {
                MultiplayerClient.AddServerSideRoom(new Room
                {
                    Name = "Test Room",
                    QueueMode = QueueMode.AllPlayers,
                    Playlist =
                    [
                        new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        }
                    ]
                }, API.LocalUser.Value);
            });

            AddStep("refresh rooms", () => this.ChildrenOfType<LoungeSubScreen>().Single().UpdateFilter());
            AddUntilStep("wait for room", () => this.ChildrenOfType<RoomPanel>().Any());
            AddStep("select room", () => InputManager.Key(Key.Down));

            AddStep("disable polling", () => this.ChildrenOfType<LoungeListingPoller>().Single().TimeBetweenPolls.Value = 0);
            AddStep("change server-side settings", () =>
            {
                MultiplayerClient.ServerSideRooms[0].Name = "New name";
                MultiplayerClient.ServerSideRooms[0].Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        ID = 2,
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ];
            });

            AddStep("join room", () => InputManager.Key(Key.Enter));
            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddUntilStep("wait for join", () => MultiplayerClient.RoomJoined);

            AddAssert("local room has correct name", () => this.ChildrenOfType<MultiplayerRoomPanel>().Single().Room.Name, () => Is.EqualTo(MultiplayerClient.ServerSideRooms[0].Name));
            AddAssert("local room has correct playlist", () => this.ChildrenOfType<MultiplayerQueueList>().Single().Items.Single().ID, () => Is.EqualTo(2));
        }

        [Test]
        [FlakyTest] // See above
        public void TestSpectatingStateResetOnBackButtonDuringGameplay()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            AddStep("set spectating state", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("state set to spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 1234 }));
            AddStep("set other user ready", () => MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Ready));

            pressReadyButton(1234);
            AddUntilStep("wait for gameplay", () => (Stack.CurrentScreen as MultiSpectatorScreen)?.IsLoaded == true);

            AddStep("press back button and exit", () =>
            {
                ((MultiSpectatorScreen)Stack.CurrentScreen).OnBackButton();
                ((MultiSpectatorScreen)Stack.CurrentScreen).Exit();
            });

            AddUntilStep("wait for return to match subscreen", () => screen.IsCurrentScreen());
            AddUntilStep("user state is idle", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Idle);
        }

        [Test]
        [FlakyTest] // See above
        public void TestSpectatingStateNotResetOnBackButtonOutsideOfGameplay()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            AddStep("set spectating state", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("state set to spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 1234 }));
            AddStep("set other user ready", () => MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Ready));

            pressReadyButton(1234);
            AddUntilStep("wait for gameplay", () => (Stack.CurrentScreen as MultiSpectatorScreen)?.IsLoaded == true);
            AddStep("set other user loaded", () => MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Loaded));
            AddStep("set other user finished play", () => MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.FinishedPlay));

            AddStep("press back button and exit", () =>
            {
                ((MultiSpectatorScreen)Stack.CurrentScreen).OnBackButton();
                ((MultiSpectatorScreen)Stack.CurrentScreen).Exit();
            });

            AddUntilStep("wait for return to match subscreen", () => screen.IsCurrentScreen());
            AddWaitStep("wait for possible state change", 5);
            AddUntilStep("user state is spectating", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);
        }

        [Test]
        [FlakyTest] // See above
        public void TestItemAddedByOtherUserDuringGameplay()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            enterGameplay();
            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 1234 }));
            AddStep("add item as other user", () => MultiplayerClient.AddUserPlaylistItem(1234, new MultiplayerPlaylistItem(
                new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                })).WaitSafely());

            AddUntilStep("item arrived in playlist", () => MultiplayerClient.ClientRoom?.Playlist.Count == 2);

            AddStep("exit gameplay as initial user", () => screen.MakeCurrent());
            AddUntilStep("queue contains item", () => this.ChildrenOfType<MultiplayerQueueList>().Single().Items.Single().ID == 2);
        }

        [Test]
        [FlakyTest] // See above
        public void TestItemAddedAndDeletedByOtherUserDuringGameplay()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ]
            });

            enterGameplay();

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 1234 }));
            AddStep("add item as other user", () => MultiplayerClient.AddUserPlaylistItem(1234, new MultiplayerPlaylistItem(
                new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                })).WaitSafely());

            AddUntilStep("item arrived in playlist", () => MultiplayerClient.ClientRoom?.Playlist.Count == 2);

            AddStep("delete item as other user", () => MultiplayerClient.RemoveUserPlaylistItem(1234, 2).WaitSafely());
            AddUntilStep("item removed from playlist", () => MultiplayerClient.ClientRoom?.Playlist.Count == 1);

            AddStep("exit gameplay as initial user", () => screen.MakeCurrent());
            AddUntilStep("queue is empty", () => this.ChildrenOfType<MultiplayerQueueList>().Single().Items.Count == 0);
        }

        [Test]
        public void TestGameplayStartsWhileInSpectatorScreen()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
            });

            AddStep("join other user and make host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 1234 });
                MultiplayerClient.TransferHost(1234);
            });

            AddStep("set local user spectating", () => MultiplayerClient.ChangeUserState(API.LocalUser.Value.OnlineID, MultiplayerUserState.Spectating));
            AddUntilStep("wait for spectating state", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            runGameplay();

            AddStep("exit gameplay for other user", () => MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Idle));
            AddUntilStep("wait for room to be idle", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.Open);

            runGameplay();

            void runGameplay()
            {
                AddStep("start match by other user", () =>
                {
                    MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Ready);
                    MultiplayerClient.StartMatch().WaitSafely();
                });

                AddUntilStep("wait for loading", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.WaitingForLoad);
                AddStep("set player loaded", () => MultiplayerClient.ChangeUserState(1234, MultiplayerUserState.Loaded));
                AddUntilStep("wait for gameplay to start", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.Playing);
                AddUntilStep("wait for local user to enter spectator", () => Stack.CurrentScreen is MultiSpectatorScreen);
            }
        }

        [Test]
        public void TestGameplayStartsWhileInSongSelectWithDifferentRuleset()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod { Acronym = "HD" } },
                    },
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 1)).BeatmapInfo)
                    {
                        RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod { Acronym = "HD" } },
                    },
                ]
            });

            AddStep("select hidden", () => MultiplayerClient.ChangeUserMods(new[] { new APIMod { Acronym = "HD" } }));
            AddStep("make user ready", () => MultiplayerClient.ChangeState(MultiplayerUserState.Ready));
            AddStep("press edit on second item", () => this.ChildrenOfType<DrawableRoomPlaylistItem>().Single(i => i.Item.RulesetID == 1)
                                                           .ChildrenOfType<DrawableRoomPlaylistItem.PlaylistEditButton>().Single().TriggerClick());

            waitForSongSelect();
            AddAssert("ruleset is taiko", () => Ruleset.Value.OnlineID == 1);

            AddStep("start match", () => MultiplayerClient.StartMatch().WaitSafely());

            AddUntilStep("wait for loading", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.WaitingForLoad);
            AddUntilStep("wait for gameplay to start", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.Playing);
            AddAssert("hidden is selected", () => SelectedMods.Value, () => Has.One.TypeOf(typeof(OsuModHidden)));
        }

        [FlakyTest]
        [Test]
        public void TestGlobalBeatmapDoesNotChangeAtResults()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod { Acronym = "HD" } },
                    },
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 1)).BeatmapInfo)
                    {
                        RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod { Acronym = "HD" } },
                    },
                ]
            });

            enterGameplay();

            // Gameplay runs in real-time, so we need to incrementally check if gameplay has finished in order to not time out.
            for (double i = 1000; i < TestResources.QUICK_BEATMAP_LENGTH; i += 1000)
            {
                double time = i;
                AddUntilStep($"wait for time > {i}", () => this.ChildrenOfType<GameplayClockContainer>().SingleOrDefault()?.CurrentTime > time);
            }

            AddUntilStep("wait for results", () => Stack.CurrentScreen is ResultsScreen);

            AddAssert("global beatmap still matches first playlist item", () => Beatmap.Value.BeatmapInfo.OnlineID, () => Is.EqualTo(MultiplayerClient.ClientRoom!.Playlist[0].BeatmapID));
            AddStep("return to match", () => Stack.Exit());
            AddAssert("global beatmap matches second playlist item", () => Beatmap.Value.BeatmapInfo.OnlineID, () => Is.EqualTo(MultiplayerClient.ClientRoom!.Playlist[1].BeatmapID));
        }

        /// <summary>
        /// Tests that the local user is not able to change their play style if they haven't downloaded the beatmap (beatmap carousel will be empty).
        /// </summary>
        [Test]
        public void TestCanNotEditDifficultyIfNotDownloaded()
        {
            IBeatmap roomBeatmap = null!;

            createRoom(() =>
            {
                roomBeatmap = CreateBeatmap(new OsuRuleset().RulesetInfo);

                return new Room
                {
                    Name = "Test Room",
                    QueueMode = QueueMode.AllPlayers,
                    Playlist =
                    [
                        new PlaylistItem(CreateAPIBeatmap(roomBeatmap.BeatmapInfo))
                        {
                            RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                            Freestyle = true
                        }
                    ]
                };
            });

            AddAssert("editing disallowed", () => !this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().UserStyleEditingEnabled);
            AddStep("import beatmap", () => beatmaps.Import(roomBeatmap.BeatmapInfo.BeatmapSet!));
            AddUntilStep("editing allowed", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().UserStyleEditingEnabled);
        }

        /// <summary>
        /// Test that the user selection screen is not exited when the beatmap is changed to the same set.
        /// </summary>
        [Test]
        public void TestUserStyleSelectionDoesNotExitWhenBeatmapSetNotChanged()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(importedSet.Beatmaps.First())
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        Freestyle = true
                    }
                ]
            });

            AddStep("open user style selection", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().ShowUserStyleSelect());
            AddUntilStep("style selection screen opened", () => this.ChildrenOfType<MultiplayerMatchFreestyleSelect>().SingleOrDefault()?.IsCurrentScreen() == true);

            AddStep("change beatmap", () =>
            {
                var newItem = MultiplayerClient.ServerRoom!.Playlist[0].Clone();
                var newBeatmap = importedSet.Beatmaps.Last();
                newItem.BeatmapID = newBeatmap.OnlineID;
                newItem.BeatmapChecksum = newBeatmap.MD5Hash;

                MultiplayerClient.EditPlaylistItem(newItem);
            });

            AddWaitStep("wait for potential beatmap change", 2);
            AddAssert("style selection screen still open", () => this.ChildrenOfType<MultiplayerMatchFreestyleSelect>().SingleOrDefault()?.IsCurrentScreen() == true);
        }

        /// <summary>
        /// Tests that the user selection screen is exited when the beatmap is changed to another set.
        /// </summary>
        [Test]
        public void TestUserStyleSelectionExitedWhenBeatmapSetChanged()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                QueueMode = QueueMode.AllPlayers,
                Playlist =
                [
                    new PlaylistItem(importedSet.Beatmaps.First())
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        Freestyle = true
                    }
                ]
            });

            AddStep("open user style selection", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().ShowUserStyleSelect());
            AddUntilStep("style selection screen opened", () => this.ChildrenOfType<MultiplayerMatchFreestyleSelect>().SingleOrDefault()?.IsCurrentScreen() == true);

            AddStep("change beatmap set", () =>
            {
                var newItem = MultiplayerClient.ServerRoom!.Playlist[0].Clone();
                var newBeatmap = importedSet2.Beatmaps.Last();
                newItem.BeatmapID = newBeatmap.OnlineID;
                newItem.BeatmapChecksum = newBeatmap.MD5Hash;

                MultiplayerClient.EditPlaylistItem(newItem);
            });

            AddUntilStep("selected beatmap changed", () => Beatmap.Value.BeatmapInfo.Equals(importedSet2.Beatmaps.First()));
            AddUntilStep("style selection screen closed", () => this.ChildrenOfType<MultiplayerMatchFreestyleSelect>().SingleOrDefault()?.IsCurrentScreen() != true);
        }

        [Test]
        public void TestMaxParticipantsAndSlots()
        {
            createRoom(() => new Room
            {
                Name = "Test Room",
                Password = "password",
                Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0)).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ],
                MaxParticipants = 10
            });

            AddStep("turn max participants off", () => MultiplayerClient.ChangeSettings(maxParticipants: null));
            AddStep("turn max participants back on", () => MultiplayerClient.ChangeSettings(maxParticipants: 8));
        }

        private void enterGameplay()
        {
            pressReadyButton();
            pressReadyButton();
            AddUntilStep("wait for player", () => Stack.CurrentScreen is Player);
        }

        private ReadyButton readyButton => this.ChildrenOfType<ReadyButton>().Single();

        private void pressReadyButton(int? playingUserId = null)
        {
            // Can't use ClickButtonWhenEnabled<> due to needing to store the state after the button is enabled.

            AddUntilStep("wait for ready button to be enabled", () => readyButton.Enabled.Value);

            MultiplayerUserState lastState = MultiplayerUserState.Idle;
            MultiplayerRoomUser? user = null;

            AddStep("click ready button", () =>
            {
                user = playingUserId == null ? MultiplayerClient.LocalUser : MultiplayerClient.ServerRoom?.Users.Single(u => u.UserID == playingUserId);
                lastState = user?.State ?? MultiplayerUserState.Idle;

                InputManager.MoveMouseTo(readyButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for state change", () => user?.State != lastState);
        }

        private void createRoom(Func<Room> room)
        {
            AddUntilStep("wait for lounge", () => this.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => this.ChildrenOfType<LoungeSubScreen>().Single().Open(room()));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => MultiplayerClient.RoomJoined);
        }

        private void waitForSongSelect()
        {
            AddUntilStep("wait for song select", () =>
            {
                var songSelect = InputManager.ChildrenOfType<MultiplayerMatchSongSelect>().FirstOrDefault();
                return songSelect != null && songSelect.IsCurrentScreen() && !songSelect.IsFiltering;
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }
    }
}
