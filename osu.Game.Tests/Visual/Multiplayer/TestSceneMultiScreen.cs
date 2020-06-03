// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public class TestSceneMultiScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        public TestSceneMultiScreen()
        {
            Screens.Multi.Multiplayer multi = new Screens.Multi.Multiplayer();

            AddStep(@"show", () => LoadScreen(multi));
        }
    }
}
