// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneGameplayWarmupScreen : MultiplayerTestScene
    {
        private RankedPlayScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () =>
            {
                var beatmap = new TestBeatmap(Ruleset.Value).BeatmapInfo;
                beatmap.StarRating = 2;

                var room = CreateDefaultRoom(MatchType.RankedPlay);
                room.Playlist =
                [
                    new PlaylistItem(beatmap)
                    {
                        RulesetID = Ruleset.Value.OnlineID
                    }
                ];

                JoinRoom(room);
            });

            WaitForJoined();
            AddStep("add other user", () => MultiplayerClient.AddUser(new MultiplayerRoomUser(2)));

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
            AddUntilStep("screen loaded", () => screen.IsLoaded);
            AddStep("play card", () => MultiplayerClient.PlayCard(new RankedPlayCardItem()));

            AddStep("set warmup state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.GameplayWarmup).WaitSafely());
        }
    }
}
