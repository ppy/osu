// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayer : ScreenTestScene
    {
        private TestMultiplayer multiplayerScreen;

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;

        private TestMultiplayerClient client => multiplayerScreen.Client;
        private Room room => client.APIRoom;

        public TestSceneMultiplayer()
        {
            loadMultiplayer();
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
            importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
        });

        [Test]
        public void TestUserSetToIdleWhenBeatmapDeleted()
        {
            loadMultiplayer();

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
            loadMultiplayer();

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

            AddStep("start match externally", () => client.StartMatch());

            AddAssert("play not started", () => multiplayerScreen.IsCurrentScreen());
        }

        [Test]
        public void TestLocalPlayStartsWhileSpectatingWhenBeatmapBecomesAvailable()
        {
            loadMultiplayer();

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

            AddStep("start match externally", () => client.StartMatch());

            AddStep("restore beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
            });

            AddUntilStep("play started", () => !multiplayerScreen.IsCurrentScreen());
        }

        private void createRoom(Func<Room> room)
        {
            AddStep("open room", () =>
            {
                multiplayerScreen.OpenNewRoom(room());
            });

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => client.Room != null);
        }

        private void loadMultiplayer()
        {
            AddStep("show", () =>
            {
                multiplayerScreen = new TestMultiplayer();

                // Needs to be added at a higher level since the multiplayer screen becomes non-current.
                Child = multiplayerScreen.Client;

                LoadScreen(multiplayerScreen);
            });

            AddUntilStep("wait for loaded", () => multiplayerScreen.IsLoaded);
        }

        private class TestMultiplayer : Screens.OnlinePlay.Multiplayer.Multiplayer
        {
            [Cached(typeof(MultiplayerClient))]
            public readonly TestMultiplayerClient Client;

            public TestMultiplayer()
            {
                Client = new TestMultiplayerClient((TestMultiplayerRoomManager)RoomManager);
            }

            protected override RoomManager CreateRoomManager() => new TestMultiplayerRoomManager();
        }
    }
}
