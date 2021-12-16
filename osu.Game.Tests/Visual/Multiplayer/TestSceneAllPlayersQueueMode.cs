// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneAllPlayersQueueMode : QueueModeTestScene
    {
        protected override QueueMode Mode => QueueMode.AllPlayers;

        [Test]
        public void TestFirstItemSelectedByDefault()
        {
            AddAssert("first item selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestItemAddedToTheEndOfQueue()
        {
            addItem(() => OtherBeatmap);
            AddAssert("playlist has 2 items", () => Client.APIRoom?.Playlist.Count == 2);

            addItem(() => InitialBeatmap);
            AddAssert("playlist has 3 items", () => Client.APIRoom?.Playlist.Count == 3);

            AddAssert("first item still selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestSingleItemExpiredAfterGameplay()
        {
            RunGameplay();

            AddAssert("playlist has only one item", () => Client.APIRoom?.Playlist.Count == 1);
            AddAssert("playlist item is expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("last item selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[0].ID);
        }

        [Test]
        public void TestNextItemSelectedAfterGameplayFinish()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            RunGameplay();

            AddAssert("first item expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("next item selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[1].ID);

            RunGameplay();

            AddAssert("second item expired", () => Client.APIRoom?.Playlist[1].Expired == true);
            AddAssert("next item selected", () => Client.Room?.Settings.PlaylistItemId == Client.APIRoom?.Playlist[2].ID);
        }

        [Test]
        public void TestItemsNotClearedWhenSwitchToHostOnlyMode()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            // Move to the "other" beatmap.
            RunGameplay();

            AddStep("change queue mode", () => Client.ChangeSettings(queueMode: QueueMode.HostOnly));
            AddAssert("playlist has 3 items", () => Client.APIRoom?.Playlist.Count == 3);
            AddAssert("item 2 is not expired", () => Client.APIRoom?.Playlist[1].Expired == false);
            AddAssert("current item is the other beatmap", () => Client.Room?.Settings.PlaylistItemId == 2);
        }

        [Test]
        public void TestCorrectItemSelectedAfterNewItemAdded()
        {
            addItem(() => OtherBeatmap);
            AddUntilStep("selected beatmap is initial beatmap", () => Beatmap.Value.BeatmapInfo.OnlineID == InitialBeatmap.OnlineID);
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
