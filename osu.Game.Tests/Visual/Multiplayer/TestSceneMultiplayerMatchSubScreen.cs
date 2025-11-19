// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Taiko;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Menu;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerMatchSubScreen : MultiplayerTestScene
    {
        private MultiplayerMatchSubScreen screen = null!;
        private RulesetStore rulesets = null!;
        private BeatmapManager beatmaps = null!;
        private BeatmapSetInfo importedSet = null!;
        private Room room = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
            Dependencies.CacheAs<BeatmapStore>(new RealmDetachedBeatmapStore());

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            importedSet = beatmaps.GetAllUsableBeatmapSets().First();

            Realm.Write(r =>
            {
                foreach (var beatmapInfo in r.All<BeatmapInfo>())
                    beatmapInfo.OnlineMD5Hash = beatmapInfo.MD5Hash;
            });
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddUntilStep("wait for mod select removed", () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Count(), () => Is.Zero);

            AddStep("load match", () =>
            {
                room = new Room { Name = "Test Room" };
                LoadScreen(screen = new TestMultiplayerMatchSubScreen(room));
            });

            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        public void TestCreatedRoom()
        {
            AddStep("add playlist item", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);
        }

        [Test]
        public void TestTaikoOnlyMod()
        {
            AddStep("add playlist item", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new TaikoRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new TaikoRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod(new TaikoModSwap()) }
                    }
                ];
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
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
            });

            AddAssert("create button enabled", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single().Enabled.Value);
        }

        [Test]
        public void TestStartMatchWhileSpectating()
        {
            AddStep("set playlist", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for room join", () => RoomJoined);

            AddStep("join other user", void () => MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID }));
            AddUntilStep("wait for user populated", () => MultiplayerClient.ClientRoom!.Users.Single(u => u.UserID == PLAYER_1_ID).User, () => Is.Not.Null);
            AddStep("other user ready", () => MultiplayerClient.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready));

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();

            AddUntilStep("wait for spectating user state", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Spectating);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("match started", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.WaitingForLoad);
        }

        [Test]
        public void TestFreeModSelectionHasAllowedMods()
        {
            AddStep("add playlist item with allowed mod", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod(new OsuModDoubleTime()) }
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            ClickButtonWhenEnabled<UserModSelectButton>();

            AddUntilStep("mod select contents loaded",
                () => this.ChildrenOfType<ModColumn>().Any() && this.ChildrenOfType<ModColumn>().All(col => col.IsLoaded && col.ItemsLoaded));
            AddUntilStep("mod select contains only double time mod",
                () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single()
                          .ChildrenOfType<ModPanel>()
                          .SingleOrDefault(panel => panel.Visible)?.Mod is OsuModDoubleTime);
        }

        [Test]
        public void TestModSelectKeyWithAllowedMods()
        {
            AddStep("add playlist item with allowed mod", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        AllowedMods = new[] { new APIMod(new OsuModDoubleTime()) }
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            AddStep("press toggle mod select key", () => InputManager.Key(Key.F1));

            AddUntilStep("mod select shown", () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single().State.Value == Visibility.Visible);
        }

        [Test]
        public void TestModSelectKeyWithNoAllowedMods()
        {
            AddStep("add playlist item with no allowed mods", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    }
                ];
            });
            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            AddStep("press toggle mod select key", () => InputManager.Key(Key.F1));

            AddWaitStep("wait some", 3);
            AddAssert("mod select not shown", () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single().State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestNextPlaylistItemSelectedAfterCompletion()
        {
            AddStep("add two playlist items", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    },
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
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
                var lastItem = this.ChildrenOfType<MultiplayerQueueList>()
                                   .Single()
                                   .ChildrenOfType<DrawableRoomPlaylistItem>()
                                   .Single(p => p.Item.ID == MultiplayerClient.ServerAPIRoom?.Playlist.Last().ID);
                return lastItem.IsSelectedItem;
            });
        }

        [Test]
        public void TestModSelectOverlay()
        {
            AddStep("add playlist item", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        RequiredMods = new[]
                        {
                            new APIMod(new OsuModDoubleTime { SpeedChange = { Value = 2.0 } }),
                            new APIMod(new OsuModStrictTracking()),
                        },
                        AllowedMods = new[]
                        {
                            new APIMod(new OsuModFlashlight()),
                        }
                    }
                ];
            });
            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            ClickButtonWhenEnabled<UserModSelectButton>();
            AddUntilStep("mod select shows unranked", () => this.ChildrenOfType<RankingInformationDisplay>().Single().Ranked.Value == false);
            AddAssert("score multiplier = 1.20", () => this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value, () => Is.EqualTo(1.2).Within(0.01));

            AddStep("select flashlight", () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single().ChildrenOfType<ModPanel>().Single(m => m.Mod is ModFlashlight).TriggerClick());
            AddAssert("score multiplier = 1.35", () => this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value, () => Is.EqualTo(1.35).Within(0.01));

            AddStep("change flashlight setting", () => ((OsuModFlashlight)this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single().SelectedMods.Value.Single()).FollowDelay.Value = 1200);
            AddAssert("score multiplier = 1.20", () => this.ChildrenOfType<RankingInformationDisplay>().Single().ModMultiplier.Value, () => Is.EqualTo(1.2).Within(0.01));
        }

        [Test]
        public void TestChangeSettingsButtonVisibleForHost()
        {
            AddStep("add playlist item", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
            });
            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => RoomJoined);

            AddUntilStep("button visible", () => this.ChildrenOfType<MultiplayerRoomPanel>().Single().ChangeSettingsButton.Alpha, () => Is.GreaterThan(0));
            AddStep("join other user", void () => MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID }));
            AddStep("make other user host", () => MultiplayerClient.TransferHost(PLAYER_1_ID));
            AddAssert("button hidden", () => this.ChildrenOfType<MultiplayerRoomPanel>().Single().ChangeSettingsButton.Alpha, () => Is.EqualTo(0));
        }

        [Test]
        public void TestUserModSelectUpdatesWhenNotVisible()
        {
            AddStep("add playlist item", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        AllowedMods = [new APIMod(new OsuModFlashlight())]
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();
            AddUntilStep("wait for join", () => RoomJoined);

            // 1. Open the mod select overlay and enable flashlight

            ClickButtonWhenEnabled<UserModSelectButton>();
            AddUntilStep("mod select contents loaded", () => this.ChildrenOfType<ModColumn>().Any() && this.ChildrenOfType<ModColumn>().All(col => col.IsLoaded && col.ItemsLoaded));
            AddStep("click flashlight panel", () =>
            {
                ModPanel panel = this.ChildrenOfType<ModPanel>().Single(p => p.Mod is OsuModFlashlight);
                InputManager.MoveMouseTo(panel);
                InputManager.Click(MouseButton.Left);
            });
            AddUntilStep("flashlight mod enabled", () => MultiplayerClient.ClientRoom!.Users[0].Mods.Any());

            // 2. Close the mod select overlay, edit the playlist to disable allowed mods, and then edit it again to re-enable allowed mods.

            AddStep("close mod select overlay", () => this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single().Hide());
            AddUntilStep("mod select overlay not present", () => !this.ChildrenOfType<MultiplayerUserModSelectOverlay>().Single().IsPresent);
            AddStep("disable allowed mods", () => MultiplayerClient.EditPlaylistItem(new MultiplayerPlaylistItem(new PlaylistItem(MultiplayerClient.ServerRoom!.Playlist[0])
            {
                AllowedMods = []
            })));
            // This would normally be done as part of the above operation with an actual server.
            AddStep("disable user mods", () => MultiplayerClient.ChangeUserMods(API.LocalUser.Value.OnlineID, Array.Empty<APIMod>()));
            AddUntilStep("flashlight mod disabled", () => !MultiplayerClient.ClientRoom!.Users[0].Mods.Any());
            AddStep("re-enable allowed mods", () => MultiplayerClient.EditPlaylistItem(new MultiplayerPlaylistItem(new PlaylistItem(MultiplayerClient.ServerRoom!.Playlist[0])
            {
                AllowedMods = [new APIMod(new OsuModFlashlight())]
            })));
            AddAssert("flashlight mod still disabled", () => !MultiplayerClient.ClientRoom!.Users[0].Mods.Any());

            // 3. Open the mod select overlay, check that the flashlight mod panel is deactivated.

            ClickButtonWhenEnabled<UserModSelectButton>();
            AddUntilStep("mod select contents loaded", () => this.ChildrenOfType<ModColumn>().Any() && this.ChildrenOfType<ModColumn>().All(col => col.IsLoaded && col.ItemsLoaded));
            AddAssert("flashlight mod still disabled", () => !MultiplayerClient.ClientRoom!.Users[0].Mods.Any());
            AddAssert("flashlight mod panel not activated", () => !this.ChildrenOfType<ModPanel>().Single(p => p.Mod is OsuModFlashlight).Active.Value);
        }

        [Test]
        public void TestStartCountdown()
        {
            AddStep("set playlist", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for room join", () => RoomJoined);

            AddStep("click countdown button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerCountdownButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("start a countdown", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<Popover>().Single().ChildrenOfType<Button>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("countdown started", () => MultiplayerClient.ServerRoom!.ActiveCountdowns.Any());
        }

        [Test]
        public void TestSettingsRemainsOpenOnRoomUpdate()
        {
            AddStep("set playlist", () =>
            {
                room.Playlist =
                [
                    new PlaylistItem(beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First()).BeatmapInfo)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ];
            });

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for room join", () => RoomJoined);

            AddStep("open settings", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay>().Single().Show());
            AddAssert("settings opened", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("trigger room update", () => MultiplayerClient.AddPlaylistItem(MultiplayerClient.ServerRoom!.Playlist[0].Clone()));
            AddAssert("settings still open", () => this.ChildrenOfType<MultiplayerMatchSettingsOverlay>().Single().State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }

        private partial class TestMultiplayerMatchSubScreen : MultiplayerMatchSubScreen
        {
            [Resolved(canBeNull: true)]
            private IDialogOverlay? dialogOverlay { get; set; }

            public TestMultiplayerMatchSubScreen(Room room)
                : base(room)
            {
            }

            public override bool OnExiting(ScreenExitEvent e)
            {
                // For testing purposes allow the screen to exit without confirming on second attempt.
                if (!ExitConfirmed && dialogOverlay?.CurrentDialog is ConfirmDiscardChangesDialog confirmDialog)
                {
                    confirmDialog.PerformAction<PopupDialogDangerousButton>();
                    return true;
                }

                return base.OnExiting(e);
            }
        }
    }
}
