// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Screens;
using osu.Game.Screens.OnlinePlay;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public partial class TestSceneMultiHeader : OsuTestScene
    {
        public TestSceneMultiHeader()
        {
            int index = 0;

            OsuScreenStack screenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both };

            screenStack.Push(new TestOnlinePlaySubScreen(index));

            Children = new Drawable[]
            {
                screenStack,
                new Header("Multiplayer", screenStack)
            };

            AddStep("push multi screen", () => screenStack.CurrentScreen.Push(new TestOnlinePlaySubScreen(++index)));
        }

        private partial class TestOnlinePlaySubScreen : OsuScreen, IOnlinePlaySubScreen
        {
            private readonly int index;

            public string ShortTitle => $"Screen {index}";

            public TestOnlinePlaySubScreen(int index)
            {
                this.index = index;
            }

            public override string ToString() => ShortTitle;
        }
    }
}
