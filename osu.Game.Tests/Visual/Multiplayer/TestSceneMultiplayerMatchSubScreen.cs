// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
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
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            importedSet = beatmaps.GetAllUsableBeatmapSets().First();
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
            AddStep("add playlist item", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                });
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);
        }

        [Test]
        public void TestTaikoOnlyMod()
        {
            AddStep("add playlist item", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new TaikoRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                    AllowedMods = new[] { new APIMod(new TaikoModSwap()) }
                });
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            AddStep("select swap mod", () => MultiplayerClient.ChangeUserMods(API.LocalUser.Value.OnlineID, new[] { new TaikoModSwap() }));
            AddUntilStep("participant panel has mod", () => this.ChildrenOfType<ParticipantPanel>().Any(p => p.ChildrenOfType<ModIcon>().Any(m => m.Mod is TaikoModSwap)));
        }

        [Test]
        public void TestSettingValidity()
        {
            AddAssert("create button not enabled", () => !this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);

            AddStep("set playlist", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                });
            });

            AddAssert("create button enabled", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestStartMatchWhileSpectating()
        {
            AddStep("set playlist", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                });
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for room join", () => RoomJoined);

            AddStep("join other user (ready)", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID });
                MultiplayerClient.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready);
            });

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();

            AddUntilStep("wait for spectating user state", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("match started", () => MultiplayerClient.Room?.State == MultiplayerRoomState.WaitingForLoad);
        }

        [Test]
        public void TestFreeModSelectionHasAllowedMods()
        {
            AddStep("add playlist item with allowed mod", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    AllowedMods = new[] { new APIMod(new OsuModDoubleTime()) }
                });
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            ClickButtonWhenEnabled<RoomSubScreen.UserModSelectButton>();

            AddUntilStep("mod select contents loaded",
                () => this.ChildrenOfType<ModColumn>().Any() && this.ChildrenOfType<ModColumn>().All(col => col.IsLoaded && col.ItemsLoaded));
            AddUntilStep("mod select contains only double time mod",
                () => this.ChildrenOfType<UserModSelectOverlay>()
                          .SingleOrDefault()?
                          .ChildrenOfType<ModPanel>()
                          .SingleOrDefault(panel => !panel.Filtered.Value)?.Mod is OsuModDoubleTime);
        }

        [Test]
        public void TestModSelectKeyWithAllowedMods()
        {
            AddStep("add playlist item with allowed mod", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    AllowedMods = new[] { new APIMod(new OsuModDoubleTime()) }
                });
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            AddStep("press toggle mod select key", () => InputManager.Key(Key.F1));

            AddUntilStep("mod select shown", () => this.ChildrenOfType<ModSelectOverlay>().Single().State.Value == Visibility.Visible);
        }

        [Test]
        public void TestModSelectKeyWithNoAllowedMods()
        {
            AddStep("add playlist item with no allowed mods", () =>
            {
                SelectedRoom.Value.Playlist.Add(new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                {
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                });
            });
            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            AddStep("press toggle mod select key", () => InputManager.Key(Key.F1));

            AddWaitStep("wait some", 3);
            AddAssert("mod select not shown", () => this.ChildrenOfType<ModSelectOverlay>().Single().State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestNextPlaylistItemSelectedAfterCompletion()
        {
            AddStep("add two playlist items", () =>
            {
                SelectedRoom.Value.Playlist.AddRange(new[]
                {
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    },
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                });
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddStep("change user to loaded", () => MultiplayerClient.ChangeState(MultiplayerUserState.Loaded));
            AddUntilStep("user playing", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Playing);
            AddStep("abort gameplay", () => MultiplayerClient.AbortGameplay());

            AddUntilStep("last playlist item selected", () =>
            {
                var lastItem = this.ChildrenOfType<DrawableRoomPlaylistItem>().Single(p => p.Item.ID == MultiplayerClient.APIRoom?.Playlist.Last().ID);
                return lastItem.IsSelectedItem;
            });
        }
    }
}
