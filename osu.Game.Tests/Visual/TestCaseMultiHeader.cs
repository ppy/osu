// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Multiplayer;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMultiHeader : OsuTestCase
    {
        public TestCaseMultiHeader()
        {
            Header header;
            TestScreen startScreen;
            Children = new Drawable[]
            {
                header = new Header(),
                startScreen = new TestScreenOne(),
            };

            header.CurrentScreen = startScreen;
        }

        private abstract class TestScreen : MultiplayerScreen
        {
            protected abstract string ButtonText { get; }
            protected abstract TestScreen CreateNextScreen();

            protected TestScreen()
            {
                Child = new TriangleButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 100,
                    Text = ButtonText,
                    Action = () => Push(CreateNextScreen()),
                };
            }
        }

        private class TestScreenOne : TestScreen
        {
            public override string Title => @"one";
            public override string Name => @"Screen One";
            protected override string ButtonText => @"Two";
            protected override TestScreen CreateNextScreen() => new TestScreenTwo();
        }

        private class TestScreenTwo : TestScreen
        {
            public override string Title => @"two";
            public override string Name => @"Screen Two";
            protected override string ButtonText => @"One";
            protected override TestScreen CreateNextScreen() => new TestScreenOne();
        }
    }
}
