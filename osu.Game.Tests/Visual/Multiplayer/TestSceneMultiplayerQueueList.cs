// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerQueueList : MultiplayerTestScene
    {
        private MultiplayerQueueList playlist = null!;
        private RulesetStore rulesets = null!;
        private BeatmapManager beatmaps = null!;
        private BeatmapSetInfo importedSet = null!;
        private BeatmapInfo importedBeatmap = null!;
        private Room room = null!;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, API, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create room", () => room = CreateDefaultRoom());
            AddStep("join room", () => JoinRoom(room));
            WaitForJoined();

            AddStep("create playlist", () =>
            {
                Child = playlist = new MultiplayerQueueList
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300)
                };

                playlist.Items.ReplaceRange(0, playlist.Items.Count, MultiplayerClient.ClientAPIRoom!.Playlist);

                MultiplayerClient.ClientAPIRoom!.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(Room.Playlist))
                        playlist.Items.ReplaceRange(0, playlist.Items.Count, MultiplayerClient.ClientAPIRoom.Playlist);
                };
            });

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
                importedBeatmap = importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0);
            });

            AddStep("change to all players mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }).WaitSafely());
        }

        [Test]
        public void TestDeleteButtonAlwaysVisibleForHost()
        {
            AddStep("set all players queue mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }).WaitSafely());
            AddUntilStep("wait for queue mode change", () => MultiplayerClient.ClientAPIRoom?.QueueMode == QueueMode.AllPlayers);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(1, true);
            addPlaylistItem(() => 1234);
            assertDeleteButtonVisibility(2, true);
        }

        [Test]
        public void TestDeleteButtonOnlyVisibleForItemOwnerIfNotHost()
        {
            AddStep("set all players queue mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }).WaitSafely());
            AddUntilStep("wait for queue mode change", () => MultiplayerClient.ClientAPIRoom?.QueueMode == QueueMode.AllPlayers);

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 1234 }));
            AddStep("set other user as host", () => MultiplayerClient.TransferHost(1234));

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);
            assertDeleteButtonVisibility(1, true);
            addPlaylistItem(() => 1234);
            assertDeleteButtonVisibility(2, false);

            AddStep("set local user as host", () => MultiplayerClient.TransferHost(API.LocalUser.Value.OnlineID));
            assertDeleteButtonVisibility(1, true);
            assertDeleteButtonVisibility(2, true);
        }

        [Test]
        public void TestSingleItemDoesNotHaveDeleteButton()
        {
            AddStep("set all players queue mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }).WaitSafely());
            AddUntilStep("wait for queue mode change", () => MultiplayerClient.ClientAPIRoom?.QueueMode == QueueMode.AllPlayers);

            assertDeleteButtonVisibility(0, false);
        }

        [Test]
        public void TestCurrentItemHasDeleteButtonIfNotSingle()
        {
            AddStep("set all players queue mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayers }).WaitSafely());
            AddUntilStep("wait for queue mode change", () => MultiplayerClient.ClientAPIRoom?.QueueMode == QueueMode.AllPlayers);

            addPlaylistItem(() => API.LocalUser.Value.OnlineID);

            assertDeleteButtonVisibility(0, true);
            assertDeleteButtonVisibility(1, true);

            AddStep("finish current item", () => MultiplayerClient.FinishCurrentItem().WaitSafely());
            AddUntilStep("wait for next item to be selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == 2);
            AddUntilStep("wait for two items in playlist", () => playlist.ChildrenOfType<DrawableRoomPlaylistItem>().Count() == 2);

            assertDeleteButtonVisibility(0, false);
            assertDeleteButtonVisibility(1, false);
        }

        [Test]
        public void TestChangeExistingItem()
        {
            AddStep("change beatmap", () => MultiplayerClient.EditPlaylistItem(new MultiplayerPlaylistItem
            {
                ID = playlist.Items[0].ID,
                BeatmapID = 1337
            }).WaitSafely());

            AddUntilStep("first playlist item has new beatmap", () => playlist.Items[0].Beatmap.OnlineID, () => Is.EqualTo(1337));
        }

        private void addPlaylistItem(Func<int> userId)
        {
            long itemId = -1;

            AddStep("add playlist item", () =>
            {
                MultiplayerPlaylistItem item = new MultiplayerPlaylistItem(new PlaylistItem(importedBeatmap));

                MultiplayerClient.AddUserPlaylistItem(userId(), item).WaitSafely();

                itemId = item.ID;
            });

            AddUntilStep("item arrived in playlist", () => playlist.ChildrenOfType<RearrangeableListItem<PlaylistItem>>().Any(i => i.Model.ID == itemId));
        }

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddUntilStep($"delete button {index} {(visible ? "is" : "is not")} visible", () =>
            {
                var button = playlist.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAtOrDefault(index);
                return (button?.Alpha > 0) == visible;
            });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }
    }
}
