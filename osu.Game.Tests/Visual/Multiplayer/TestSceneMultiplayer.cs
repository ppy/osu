// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Containers;
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
using osu.Game.Screens.OnlinePlay.Match.Components;
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
        }

        [Test]
        public void TestEmpty()
        {
            // used to test the flow of multiplayer from visual tests.
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

            AddStep("start match externally", () => client.StartMatch());

            AddStep("restore beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
            });

            AddUntilStep("play started", () => !multiplayerScreen.IsCurrentScreen());
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

            AddStep("open mod overlay", () => this.ChildrenOfType<PurpleTriangleButton>().ElementAt(2).Click());

            AddStep("invoke on back button", () => multiplayerScreen.OnBackButton());

            AddAssert("mod overlay is hidden", () => this.ChildrenOfType<LocalPlayerModSelectOverlay>().Single().State.Value == Visibility.Hidden);

            AddAssert("dialog overlay is hidden", () => DialogOverlay.State.Value == Visibility.Hidden);

            testLeave("lounge tab item", () => this.ChildrenOfType<BreadcrumbControl<IScreen>.BreadcrumbTabItem>().First().Click());

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
            public new TestMultiplayerRoomManager RoomManager { get; private set; }

            protected override RoomManager CreateRoomManager() => RoomManager = new TestMultiplayerRoomManager();
        }
    }
}
