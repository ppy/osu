// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Tests.Visual.Multiplayer;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneEndedScreen : MultiplayerTestScene
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
        }

        [Test]
        public void TestVictory()
        {
            AddStep("set results state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.Ended, s =>
            {
                s.WinningUserId = API.LocalUser.Value.OnlineID;
                s.Users[API.LocalUser.Value.OnlineID].RatingAfter = 1520;
                s.Users[2].RatingAfter = 1480;
            }).WaitSafely());
        }

        [Test]
        public void TestDefeat()
        {
            AddStep("set results state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.Ended, s =>
            {
                s.WinningUserId = 2;
                s.Users[API.LocalUser.Value.OnlineID].RatingAfter = 1480;
                s.Users[2].RatingAfter = 1520;
            }).WaitSafely());
        }

        [Test]
        public void TestDraw()
        {
            AddStep("set results state", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.Ended, s =>
            {
                s.Users[API.LocalUser.Value.OnlineID].RatingAfter = 1480;
                s.Users[2].RatingAfter = 1520;
            }).WaitSafely());
        }
    }
}
