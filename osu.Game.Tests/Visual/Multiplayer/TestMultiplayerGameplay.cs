// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerGameplay : MultiplayerTestScene
    {
        [Test]
        public void TestBasic()
        {
            AddStep("load screen", () =>
                LoadScreen(new MultiplayerPlayer(Client.CurrentMatchPlayingItem.Value, Client.Room?.Users.Select(u => u.UserID).ToArray())));
        }
    }
}
