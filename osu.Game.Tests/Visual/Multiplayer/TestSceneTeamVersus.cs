// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneTeamVersus : ScreenTestScene
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

            AddUntilStep("wait for dependencies screen", () => Stack.CurrentScreen is DependenciesScreen);
            AddUntilStep("wait for dependencies to start load", () => dependenciesScreen.LoadState > LoadState.NotLoaded);
            AddUntilStep("wait for dependencies to load", () => dependenciesScreen.IsLoaded);

            AddStep("load multiplayer", () => LoadScreen(multiplayerScreen));
            AddUntilStep("wait for multiplayer to load", () => multiplayerScreen.IsLoaded);
            AddUntilStep("wait for lounge to load", () => this.ChildrenOfType<MultiplayerLoungeSubScreen>().FirstOrDefault()?.IsLoaded == true);
        }

        [Test]
        public void TestCreateWithType()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Type = { Value = MatchType.TeamVersus },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddAssert("room type is team vs", () => client.Room?.Settings.MatchType == MatchType.TeamVersus);
            AddAssert("user state arrived", () => client.Room?.Users.FirstOrDefault()?.MatchState is TeamVersusUserState);
        }

        [Test]
        public void TestChangeTeamsViaButton()
        {
            createRoom(() => new Room
            {
                Name = { Value = "Test Room" },
                Type = { Value = MatchType.TeamVersus },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First(b => b.RulesetID == 0)).BeatmapInfo },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            });

            AddAssert("user on team 0", () => (client.Room?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 0);

            AddStep("press button", () =>
            {
                InputManager.MoveMouseTo(multiplayerScreen.ChildrenOfType<TeamDisplay>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("user on team 1", () => (client.Room?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 1);

            AddStep("press button", () => InputManager.Click(MouseButton.Left));
            AddAssert("user on team 0", () => (client.Room?.Users.FirstOrDefault()?.MatchState as TeamVersusUserState)?.TeamID == 0);
        }

        [Test]
        public void TestChangeTypeViaMatchSettings()
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

            AddAssert("room type is head to head", () => client.Room?.Settings.MatchType == MatchType.HeadToHead);

            AddStep("change to team vs", () => client.ChangeSettings(matchType: MatchType.TeamVersus));

            AddAssert("room type is team vs", () => client.Room?.Settings.MatchType == MatchType.TeamVersus);
        }

        private void createRoom(Func<Room> room)
        {
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
