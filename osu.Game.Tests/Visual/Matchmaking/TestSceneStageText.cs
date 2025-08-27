// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneStageText : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom()));
            WaitForJoined();

            AddStep("create display", () => Child = new StageText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [TestCase(MatchmakingRoomStatus.RoomStart)]
        [TestCase(MatchmakingRoomStatus.RoundStart)]
        [TestCase(MatchmakingRoomStatus.UserPicks)]
        [TestCase(MatchmakingRoomStatus.SelectBeatmap)]
        [TestCase(MatchmakingRoomStatus.PrepareBeatmap)]
        [TestCase(MatchmakingRoomStatus.PrepareGameplay)]
        [TestCase(MatchmakingRoomStatus.Gameplay)]
        [TestCase(MatchmakingRoomStatus.RoundEnd)]
        [TestCase(MatchmakingRoomStatus.RoomEnd)]
        public void TestStatus(MatchmakingRoomStatus status)
        {
            AddStep("set status", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState { RoomStatus = status }).WaitSafely());
        }
    }
}
