// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerLobbyEvents : ScreenTestScene
    {
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;

        private DependenciesScreen dependenciesScreen;
        private TestMultiplayer multiplayerScreen;
        private TestMultiplayerClient client;

        private TestRequestHandlingMultiplayerRoomManager roomManager => multiplayerScreen.RoomManager;

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
                client = new TestMultiplayerClient(roomManager);

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
        }

        private void playerJoin()
        {
            int randomUser = RNG.Next(200000, 500000);
            client.AddUser(new User { Id = randomUser, Username = $"user {randomUser}" });
        }

        private void playerLeave()
        {
            User lastUser = client.Room?.Users.Last().User;

            if (lastUser == null || lastUser == client.LocalUser?.User)
                return;

            client.RemoveUser(lastUser);
        }

        private void playerKicked()
        {
            User lastUser = client.Room?.Users.Last().User;

            if (lastUser == null || lastUser == client.LocalUser?.User)
                return;

            client.KickUser(lastUser.Id);
        }

        private void playerReady()
        {
            var nextUnready = client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
            if (nextUnready != null)
                client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
        }

        private void playerUnready()
        {
            var nextUnready = client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Ready);
            if (nextUnready != null)
                client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Idle);
        }

        private void randomEvent()
        {
            int eventToPerform = RNG.Next(1, 6);

            switch (eventToPerform)
            {
                case 1:
                    playerJoin();
                    break;

                case 2:
                    playerLeave();
                    break;

                case 3:
                    playerKicked();
                    break;

                case 4:
                    playerReady();
                    break;

                case 5:
                    playerUnready();
                    break;
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

            AddRepeatStep("random stuff happens", randomEvent, 30);

            // ensure we have a handful of players so the ready-up sounds good :9
            AddRepeatStep("player joins", playerJoin, 5);

            // all ready
            AddRepeatStep("players ready up", () =>
            {
                var nextUnready = client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, 40);
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
