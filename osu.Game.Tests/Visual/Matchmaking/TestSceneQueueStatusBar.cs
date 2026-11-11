// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Matchmaking;
using osu.Game.Screens.OnlinePlay.Matchmaking.Queue;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneQueueStatusBar : MultiplayerTestScene
    {
        private ScreenQueue.StatusBar statusBar = null!;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create", () => Child = statusBar = new ScreenQueue.StatusBar
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("change state to idle", () =>
            {
                QueueController.LeaveQueue();
                statusBar.Hide();
            });

            AddStep("change state to queueing", () =>
            {
                QueueController.JoinQueue(new MatchmakingPool { Id = 0 });
                statusBar.Show();
            });

            AddStep("change state to found match", () =>
            {
                ((IMatchmakingClient)MultiplayerClient).MatchmakingRoomInvitedWithParams(new MatchmakingRoomInvitationParams());
                statusBar.Show();
            });

            AddStep("change state to waiting for room", () =>
            {
                QueueController.CurrentState.Value = ScreenQueue.MatchmakingScreenState.AcceptedWaitingForRoom;
                statusBar.Show();
            });

            AddStep("change state to in room", () =>
            {
                QueueController.CurrentState.Value = ScreenQueue.MatchmakingScreenState.InRoom;
                statusBar.Show();
            });
        }
    }
}
