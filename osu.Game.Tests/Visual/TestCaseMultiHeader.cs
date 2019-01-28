// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Screens;
using osu.Game.Screens.Multi;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMultiHeader : OsuTestCase
    {
        public TestCaseMultiHeader()
        {
            int index = 0;

            OsuScreen currentScreen = new TestMultiplayerSubScreen(index);

            Children = new Drawable[]
            {
                currentScreen,
                new Header(currentScreen)
            };

            AddStep("push multi screen", () => currentScreen.Push(currentScreen = new TestMultiplayerSubScreen(++index)));
        }

        private class TestMultiplayerSubScreen : OsuScreen, IMultiplayerSubScreen
        {
            private readonly int index;

            public string ShortTitle => $"Screen {index}";

            public TestMultiplayerSubScreen(int index)
            {
                this.index = index;
            }

            public override string ToString() => ShortTitle;
        }
    }
}
