// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneOpponentPickScreen : RankedPlayTestScene
    {
        private RankedPlayScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("add other user", () => MultiplayerClient.AddUser(new MultiplayerRoomUser(2)));

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
            AddUntilStep("screen loaded", () => screen.IsLoaded);

            var requestHandler = new BeatmapRequestHandler();

            AddStep("setup request handler", () => ((DummyAPIAccess)API).HandleRequest = requestHandler.HandleRequest);

            AddStep("set pick state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = 2).WaitSafely());

            AddStep("reveal cards", () =>
            {
                for (int i = 0; i < 5; i++)
                {
                    int i2 = i;
                    MultiplayerClient.RankedPlayRevealCard(hand => hand[i2], new MultiplayerPlaylistItem
                    {
                        ID = i2,
                        BeatmapID = requestHandler.Beatmaps[i2].OnlineID
                    }).WaitSafely();
                }
            });

            AddWaitStep("wait", 15);

            AddStep("play beatmap", () => MultiplayerClient.PlayUserCard(2, hand => hand[0]).WaitSafely());
            AddStep("reveal card", () => MultiplayerClient.RankedPlayRevealUserCard(2, hand => hand[0], new MultiplayerPlaylistItem
            {
                ID = 0,
                BeatmapID = requestHandler.Beatmaps[0].OnlineID
            }).WaitSafely());
        }
    }
}
