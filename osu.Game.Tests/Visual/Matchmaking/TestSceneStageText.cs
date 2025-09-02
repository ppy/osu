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

        [TestCase(MatchmakingStage.WaitingForClientsJoin)]
        [TestCase(MatchmakingStage.RoundWarmupTime)]
        [TestCase(MatchmakingStage.UserBeatmapSelect)]
        [TestCase(MatchmakingStage.ServerBeatmapFinalised)]
        [TestCase(MatchmakingStage.WaitingForClientsBeatmapDownload)]
        [TestCase(MatchmakingStage.GameplayWarmupTime)]
        [TestCase(MatchmakingStage.Gameplay)]
        [TestCase(MatchmakingStage.ResultsDisplaying)]
        [TestCase(MatchmakingStage.Ended)]
        public void TestStatus(MatchmakingStage status)
        {
            AddStep("set status", () => MultiplayerClient.ChangeMatchRoomState(new MatchmakingRoomState { Stage = status }).WaitSafely());
        }
    }
}
