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
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerMatchSubScreen : MultiplayerTestScene
    {
        private MultiplayerMatchSubScreen screen;

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;

        public TestSceneMultiplayerMatchSubScreen()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();

            importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            SelectedRoom.Value = new Room { Name = { Value = "Test Room" } };
        });

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load match", () => LoadScreen(screen = new MultiplayerMatchSubScreen(SelectedRoom.Value)));
            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestCreatedRoom()
        {
            AddStep("set playlist", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => Client.Room != null);
        }

        [Test]
        public void TestSettingValidity()
        {
            AddAssert("create button not enabled", () => !this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);

            AddStep("set playlist", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddAssert("create button enabled", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestStartMatchWhileSpectating()
        {
            AddStep("set playlist", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem
                {
                    Beatmap = { Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo },
                    Ruleset = { Value = new OsuRuleset().RulesetInfo },
                });
            });

            AddStep("click create button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for room join", () => Client.Room != null);

            AddStep("join other user (ready)", () =>
            {
                Client.AddUser(new APIUser { Id = PLAYER_1_ID });
                Client.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            AddStep("click spectate button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerSpectateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for spectating user state", () => Client.LocalUser?.State == MultiplayerUserState.Spectating);

            AddUntilStep("wait for ready button to be enabled", () => this.ChildrenOfType<MultiplayerReadyButton>().Single().ChildrenOfType<ReadyButton>().Single().Enabled.Value);

            AddStep("click ready button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerReadyButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("match started", () => Client.Room?.State == MultiplayerRoomState.WaitingForLoad);
        }
    }
}
