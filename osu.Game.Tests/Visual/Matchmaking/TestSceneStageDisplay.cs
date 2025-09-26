// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneStageDisplay : MultiplayerTestScene
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("add display", () => Child = new StageDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
            });
        }

        [Test]
        public void TestChangeStage()
        {
            addStage(MatchmakingStage.WaitingForClientsJoin);

            for (int i = 1; i <= 5; i++)
            {
                addStage(MatchmakingStage.RoundWarmupTime);
                addStage(MatchmakingStage.UserBeatmapSelect);
                addStage(MatchmakingStage.ServerBeatmapFinalised);
                addStage(MatchmakingStage.WaitingForClientsBeatmapDownload);
                addStage(MatchmakingStage.GameplayWarmupTime);
                addStage(MatchmakingStage.Gameplay);
                addStage(MatchmakingStage.ResultsDisplaying);
            }

            addStage(MatchmakingStage.Ended);
        }

        private void addStage(MatchmakingStage stage)
        {
            AddStep($"{stage}", () => MultiplayerClient.MatchmakingChangeStage(stage).WaitSafely());
            AddWaitStep("wait a bit", 10);
        }
    }
}
