// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Matchmaking;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneStageBubble : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("add bubble", () => Child = new StageBubble(MatchmakingStage.RoundWarmupTime, "Next Round")
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 100
            });
        }

        [Test]
        public void TestStartStopCountdown()
        {
            MultiplayerCountdown countdown = null!;

            AddStep("start countdown", () => MultiplayerClient.StartCountdown(countdown = new MatchmakingStageCountdown
            {
                Stage = MatchmakingStage.RoundWarmupTime,
                TimeRemaining = TimeSpan.FromSeconds(5)
            }).WaitSafely());

            AddWaitStep("wait a bit", 10);

            AddStep("stop countdown", () => MultiplayerClient.StopCountdown(countdown).WaitSafely());
        }
    }
}
