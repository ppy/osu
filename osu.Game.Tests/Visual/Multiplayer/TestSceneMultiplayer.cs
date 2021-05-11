// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Beatmaps;
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
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, Beatmap.Default));
            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();

            importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
        }

        [Test]
        public void TestMatchStartWhileSpectatingWithNoBeatmap()
        {
            loadMultiplayer();

            AddStep("open room", () =>
            {
                multiplayerScreen.OpenNewRoom(new Room
                {
                    Name = { Value = "Test Room" },
                    Playlist =
                    {
                        new PlaylistItem
                        {
                            Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        }
                    }
                });
            });

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => client.Room != null);

            AddStep("join other user (ready, host)", () =>
            {
                client.AddUser(new User { Id = MultiplayerTestScene.PLAYER_1_ID, Username = "Other" });
                client.TransferHost(MultiplayerTestScene.PLAYER_1_ID);
                client.ChangeUserState(MultiplayerTestScene.PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            AddStep("change playlist", () =>
            {
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddStep("click spectate button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerSpectateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("start match externally", () => client.StartMatch());

            AddAssert("screen not changed", () => multiplayerScreen.IsCurrentScreen());
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
            [Cached(typeof(StatefulMultiplayerClient))]
            public readonly TestMultiplayerClient Client;

            public TestMultiplayer()
            {
                Client = new TestMultiplayerClient((TestMultiplayerRoomManager)RoomManager);
            }

            protected override RoomManager CreateRoomManager() => new TestMultiplayerRoomManager();
        }
    }
}
