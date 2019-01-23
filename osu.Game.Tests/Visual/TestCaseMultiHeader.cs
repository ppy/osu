// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
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

            ScreenStack screenStack = new ScreenStack(new TestMultiplayerSubScreen(index)) { RelativeSizeAxes = Axes.Both };

            Children = new Drawable[]
            {
                screenStack,
                new Header(screenStack)
            };

            AddStep("push multi screen", () => screenStack.CurrentScreen.Push(new TestMultiplayerSubScreen(++index)));
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
