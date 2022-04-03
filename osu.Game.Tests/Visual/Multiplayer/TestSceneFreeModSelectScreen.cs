// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneFreeModSelectScreen : MultiplayerTestScene
    {
        [Test]
        public void TestFreeModSelect()
        {
            FreeModSelectScreen freeModSelectScreen = null;

            AddStep("create free mod select screen", () => Child = freeModSelectScreen = new FreeModSelectScreen
            {
                State = { Value = Visibility.Visible }
            });
            AddToggleStep("toggle visibility", visible =>
            {
                if (freeModSelectScreen != null)
                    freeModSelectScreen.State.Value = visible ? Visibility.Visible : Visibility.Hidden;
            });
        }
    }
}
