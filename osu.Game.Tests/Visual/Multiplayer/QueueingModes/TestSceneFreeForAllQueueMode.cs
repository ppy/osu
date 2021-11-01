// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer.Queueing;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer.QueueingModes
{
    public class TestSceneFreeForAllQueueMode : QueueModeTestScene
    {
        protected override QueueModes Mode => QueueModes.FreeForAll;

        [Test]
        public void TestFirstItemSelectedByDefault()
        {
            AddAssert("first item selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[0]);
        }

        [Test]
        public void TestItemAddedToTheEndOfQueue()
        {
            addItem(() => OtherBeatmap);
            AddAssert("playlist has 2 items", () => Client.APIRoom?.Playlist.Count == 2);
            AddAssert("last playlist item is different", () => Client.APIRoom?.Playlist[1].Beatmap.Value.OnlineID == OtherBeatmap.OnlineID);

            addItem(() => InitialBeatmap);
            AddAssert("playlist has 3 items", () => Client.APIRoom?.Playlist.Count == 3);
            AddAssert("last playlist item is different", () => Client.APIRoom?.Playlist[2].Beatmap.Value.OnlineID == InitialBeatmap.OnlineID);

            AddAssert("first item still selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[0]);
        }

        [Test]
        public void TestSingleItemExpiredAfterGameplay()
        {
            RunGameplay();

            AddAssert("playlist has only one item", () => Client.APIRoom?.Playlist.Count == 1);
            AddAssert("playlist item is expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("last item selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[0]);
        }

        [Test]
        public void TestNextItemSelectedAfterGameplayFinish()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            RunGameplay();

            AddAssert("first item expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("next item selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[1]);

            RunGameplay();

            AddAssert("second item expired", () => Client.APIRoom?.Playlist[1].Expired == true);
            AddAssert("next item selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[2]);
        }

        [Test]
        public void TestItemsClearedWhenSwitchToHostOnlyMode()
        {
            addItem(() => OtherBeatmap);
            addItem(() => InitialBeatmap);

            // Move to the "other" beatmap.
            RunGameplay();

            AddStep("change queue mode", () => Client.ChangeSettings(queueMode: QueueModes.HostOnly));
            AddAssert("playlist has 1 item", () => Client.APIRoom?.Playlist.Count == 1);
            AddAssert("playlist item is the same as last selected", () => Client.APIRoom?.Playlist[0].Beatmap.Value.OnlineID == OtherBeatmap.OnlineID);
            AddAssert("playlist item is not expired", () => Client.APIRoom?.Playlist[0].Expired == false);
        }

        private void addItem(Func<BeatmapInfo> beatmap)
        {
            AddStep("click edit button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().AddOrEditPlaylistButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.IsLoaded);
            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(beatmap()));
            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
        }
    }
}
