// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneHostOnlyQueueMode : QueueModeTestScene
    {
        protected override QueueMode Mode => QueueMode.HostOnly;

        [Test]
        public void TestFirstItemSelectedByDefault()
        {
            AddAssert("first item selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestItemStillSelectedAfterChangeToSameBeatmap()
        {
            selectNewItem(() => InitialBeatmap);

            AddAssert("playlist item still selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestItemStillSelectedAfterChangeToOtherBeatmap()
        {
            selectNewItem(() => OtherBeatmap);

            AddAssert("playlist item still selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestNewItemCreatedAfterGameplayFinished()
        {
            RunGameplay();

            AddAssert("playlist contains two items", () => Client.APIRoom?.Playlist.Count == 2);
            AddAssert("first playlist item expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("second playlist item not expired", () => Client.APIRoom?.Playlist[1].Expired == false);
            AddAssert("second playlist item selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[1].ID);
        }

        [Test]
        public void TestOnlyLastItemChangedAfterGameplayFinished()
        {
            RunGameplay();

            IBeatmapInfo firstBeatmap = null;
            AddStep("get first playlist item beatmap", () => firstBeatmap = Client.APIRoom?.Playlist[0].Beatmap.Value);

            selectNewItem(() => OtherBeatmap);

            AddAssert("first playlist item hasn't changed", () => Client.APIRoom?.Playlist[0].Beatmap.Value == firstBeatmap);
            AddAssert("second playlist item changed", () => Client.APIRoom?.Playlist[1].Beatmap.Value != firstBeatmap);
        }

        [Test]
        public void TestSettingsUpdatedWhenChangingQueueMode()
        {
            AddStep("change queue mode", () => Client.ChangeSettings(new MultiplayerRoomSettings
            {
                QueueMode = QueueMode.AllPlayers
            }));

            AddUntilStep("api room updated", () => Client.APIRoom?.QueueMode.Value == QueueMode.AllPlayers);
        }

        [Test]
        public void TestAddItemsAsHost()
        {
            addItem(() => OtherBeatmap);

            AddAssert("playlist contains two items", () => Client.APIRoom?.Playlist.Count == 2);
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
            AddUntilStep("selected item is new beatmap", () => (CurrentSubScreen as MultiplayerMatchSubScreen)?.SelectedItem.Value?.BeatmapID == otherBeatmap.OnlineID);
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
