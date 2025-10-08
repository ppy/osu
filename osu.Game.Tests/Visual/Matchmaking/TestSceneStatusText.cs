// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.Match;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.Matchmaking
{
    public partial class TestSceneStatusText : MultiplayerTestScene
    {
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.Matchmaking)));
            WaitForJoined();

            AddStep("create display", () => Child = new StageDisplay.StatusText
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestChangeStage()
        {
            foreach (var stage in Enum.GetValues<MatchmakingStage>())
            {
                AddStep($"{stage}", () => MultiplayerClient.MatchmakingChangeStage(stage).WaitSafely());
                AddWaitStep("wait a bit", 10);
            }
        }
    }
}
