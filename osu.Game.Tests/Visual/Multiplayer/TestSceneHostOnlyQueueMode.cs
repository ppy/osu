// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneHostOnlyQueueMode : QueueModeTestScene
    {
        protected override QueueMode Mode => QueueMode.HostOnly;

        [Test]
        public void TestFirstItemSelectedByDefault()
        {
            AddUntilStep("first item selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestNewItemCreatedAfterGameplayFinished()
        {
            RunGameplay();

            AddUntilStep("playlist contains two items", () => MultiplayerClient.ClientAPIRoom?.Playlist.Count == 2);
            AddUntilStep("first playlist item expired", () => MultiplayerClient.ClientAPIRoom?.Playlist[0].Expired == true);
            AddUntilStep("second playlist item not expired", () => MultiplayerClient.ClientAPIRoom?.Playlist[1].Expired == false);
            AddUntilStep("second playlist item selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[1].ID);
        }

        [Test]
        public void TestSettingsUpdatedWhenChangingQueueMode()
        {
            AddStep("change queue mode", () => MultiplayerClient.ChangeSettings(new MultiplayerRoomSettings
            {
                QueueMode = QueueMode.AllPlayers
            }).WaitSafely());

            AddUntilStep("api room updated", () => MultiplayerClient.ClientAPIRoom?.QueueMode.Value == QueueMode.AllPlayers);
        }

        [Test]
        public void TestItemStillSelectedAfterChangeToSameBeatmap()
        {
            selectNewItem(() => InitialBeatmap);

            AddUntilStep("playlist item still selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestItemStillSelectedAfterChangeToOtherBeatmap()
        {
            selectNewItem(() => OtherBeatmap);

            AddUntilStep("playlist item still selected", () => MultiplayerClient.ClientRoom?.Settings.PlaylistItemId == MultiplayerClient.ClientAPIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestOnlyLastItemChangedAfterGameplayFinished()
        {
            RunGameplay();

            IBeatmapInfo firstBeatmap = null;
            AddStep("get first playlist item beatmap", () => firstBeatmap = MultiplayerClient.ServerAPIRoom?.Playlist[0].Beatmap);

            selectNewItem(() => OtherBeatmap);

            AddUntilStep("first playlist item hasn't changed", () => MultiplayerClient.ServerAPIRoom?.Playlist[0].Beatmap == firstBeatmap);
            AddUntilStep("second playlist item changed", () => MultiplayerClient.ClientAPIRoom?.Playlist[1].Beatmap != firstBeatmap);
        }

        [Test]
        public void TestAddItemsAsHost()
        {
            addItem(() => OtherBeatmap);

            AddUntilStep("playlist contains two items", () => MultiplayerClient.ClientAPIRoom?.Playlist.Count == 2);
        }

        private void selectNewItem(Func<BeatmapInfo> beatmap)
        {
            AddUntilStep("wait for playlist panels to load", () =>
            {
                var queueList = this.ChildrenOfType<MultiplayerQueueList>().Single();
                return queueList.ChildrenOfType<DrawableRoomPlaylistItem>().Count() == queueList.Items.Count;
            });

            AddStep("click edit button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistEditButton>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.BeatmapSetsLoaded);

            BeatmapInfo otherBeatmap = null;
            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(otherBeatmap = beatmap()));

            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
            AddUntilStep("selected item is new beatmap", () => (CurrentSubScreen as MultiplayerMatchSubScreen)?.SelectedItem.Value?.Beatmap.OnlineID == otherBeatmap.OnlineID);
        }

        private void addItem(Func<BeatmapInfo> beatmap)
        {
            AddStep("click add button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSubScreen.AddItemButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.BeatmapSetsLoaded);
            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(beatmap()));
            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
        }
    }
}
