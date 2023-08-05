// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using JetBrains.Annotations;
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
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
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
using osu.Game.Screens.OnlinePlay.Multiplayer.Participants;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerMatchSubScreen : MultiplayerTestScene
    {
        private MultiplayerMatchSubScreen screen;

        private BeatmapManager beatmaps;
        private BeatmapSetInfo importedSet;

        public TestSceneMultiplayerMatchSubScreen()
            : base(false)
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();

            importedSet = beatmaps.GetAllUsableBeatmapSets().First();
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("load match", () =>
            {
                SelectedRoom.Value = new Room { Name = { Value = "Test Room" } };
                LoadScreen(screen = new TestMultiplayerMatchSubScreen(SelectedRoom.Value));
            });

            AddUntilStep("wait for load", () => screen.IsCurrentScreen());
        }

        [Test]
        [FlakyTest]
        /*
         * Fail rate around 1.5%
         *
         * TearDown : System.AggregateException : One or more errors occurred. (Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index'))
  ----> System.ArgumentOutOfRangeException : Index was out of range. Must be non-negative and less than the size of the collection. (Parameter 'index')
         * --TearDown
         *    at System.Threading.Tasks.Task.ThrowIfExceptional(Boolean includeTaskCanceledExceptions)
         *   at System.Threading.Tasks.Task.Wait(Int32 millisecondsTimeout, CancellationToken cancellationToken)
         *   at osu.Framework.Extensions.TaskExtensions.WaitSafely(Task task)
         *   at osu.Framework.Testing.TestScene.checkForErrors()
         *   at osu.Framework.Testing.TestScene.RunTestsFromNUnit()
         *--ArgumentOutOfRangeException
         *   at osu.Framework.Bindables.BindableList`1.removeAt(Int32 index, BindableList`1 caller)
         *   at osu.Framework.Bindables.BindableList`1.removeAt(Int32 index, BindableList`1 caller)
         *   at osu.Framework.Bindables.BindableList`1.removeAt(Int32 index, BindableList`1 caller)
         *   at osu.Game.Online.Multiplayer.MultiplayerClient.<>c__DisplayClass106_0.<PlaylistItemChanged>b__0() in C:\BuildAgent\work\ecd860037212ac52\osu.Game\Online\Multiplayer\MultiplayerClient     .cs:line 702
         *   at osu.Framework.Threading.ScheduledDelegate.RunTaskInternal()
         */
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
        [FlakyTest] // See above
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
        [FlakyTest] // See above
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
        [FlakyTest] // See above
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

            AddUntilStep("match started", () => MultiplayerClient.ClientRoom?.State == MultiplayerRoomState.WaitingForLoad);
        }

        [Test]
        [FlakyTest] // See above
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
                () => this.ChildrenOfType<RoomSubScreen>().Single().UserModsSelectOverlay
                          .ChildrenOfType<ModPanel>()
                          .SingleOrDefault(panel => panel.Visible)?.Mod is OsuModDoubleTime);
        }

        [Test]
        [FlakyTest] // See above
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

            AddUntilStep("mod select shown", () => this.ChildrenOfType<RoomSubScreen>().Single().UserModsSelectOverlay.State.Value == Visibility.Visible);
        }

        [Test]
        [FlakyTest] // See above
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
            AddAssert("mod select not shown", () => this.ChildrenOfType<RoomSubScreen>().Single().UserModsSelectOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        [FlakyTest] // See above
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
                var lastItem = this.ChildrenOfType<DrawableRoomPlaylistItem>().Single(p => p.Item.ID == MultiplayerClient.ServerAPIRoom?.Playlist.Last().ID);
                return lastItem.IsSelectedItem;
            });
        }

        private partial class TestMultiplayerMatchSubScreen : MultiplayerMatchSubScreen
        {
            [Resolved(canBeNull: true)]
            [CanBeNull]
            private IDialogOverlay dialogOverlay { get; set; }

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
