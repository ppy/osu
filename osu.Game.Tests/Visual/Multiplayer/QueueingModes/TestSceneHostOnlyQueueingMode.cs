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
    public class TestSceneHostOnlyQueueingMode : QueueModeTestScene
    {
        protected override QueueModes Mode => QueueModes.HostOnly;

        [Test]
        public void TestItemStillSelectedAfterChangeToSameBeatmap()
        {
            selectNewItem(() => InitialBeatmap);

            AddAssert("playlist item still selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[0]);
        }

        [Test]
        public void TestItemStillSelectedAfterChangeToOtherBeatmap()
        {
            selectNewItem(() => OtherBeatmap);

            AddAssert("playlist item still selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[0]);
        }

        [Test]
        public void TestNewItemCreatedAfterGameplayFinished()
        {
            RunGameplay();

            AddAssert("playlist contains two items", () => Client.APIRoom?.Playlist.Count == 2);
            AddAssert("first playlist item expired", () => Client.APIRoom?.Playlist[0].Expired == true);
            AddAssert("second playlist item not expired", () => Client.APIRoom?.Playlist[1].Expired == false);
            AddAssert("second playlist item selected", () => Client.CurrentMatchPlayingItem.Value == Client.APIRoom?.Playlist[1]);
        }

        [Test]
        public void TestOnlyLastItemChangedAfterGameplayFinished()
        {
            RunGameplay();

            BeatmapInfo firstBeatmap = null;
            AddStep("get first playlist item beatmap", () => firstBeatmap = Client.APIRoom?.Playlist[0].Beatmap.Value);

            selectNewItem(() => OtherBeatmap);

            AddAssert("first playlist item hasn't changed", () => Client.APIRoom?.Playlist[0].Beatmap.Value == firstBeatmap);
            AddAssert("second playlist item changed", () => Client.APIRoom?.Playlist[1].Beatmap.Value != firstBeatmap);
        }

        private void selectNewItem(Func<BeatmapInfo> beatmap)
        {
            AddStep("click edit button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSubScreen>().Single().AddOrEditPlaylistButton);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for song select", () => CurrentSubScreen is Screens.Select.SongSelect select && select.IsLoaded);

            BeatmapInfo otherBeatmap = null;
            AddStep("select other beatmap", () => ((Screens.Select.SongSelect)CurrentSubScreen).FinaliseSelection(otherBeatmap = beatmap()));

            AddUntilStep("wait for return to match", () => CurrentSubScreen is MultiplayerMatchSubScreen);
            AddUntilStep("selected item is new beatmap", () => Client.CurrentMatchPlayingItem.Value?.Beatmap.Value?.OnlineID == otherBeatmap.OnlineID);
        }
    }
}
