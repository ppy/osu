// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Screens.Play;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    public class TestSceneSpectator : ScreenTestScene
    {
        private readonly User user = new User { Id = 1234, Username = "Test user" };

        [Test]
        public void TestSpectating()
        {
            AddStep("load screen", () => LoadScreen(new Spectator(user)));
        }
    }
}
