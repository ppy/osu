// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.Multi;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public class TestSceneMultiHeader : OsuTestScene
    {
        public TestSceneMultiHeader()
        {
            int index = 0;

            OsuScreenStack screenStack = new OsuScreenStack(new TestMultiplayerSubScreen(index)) { RelativeSizeAxes = Axes.Both };

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
