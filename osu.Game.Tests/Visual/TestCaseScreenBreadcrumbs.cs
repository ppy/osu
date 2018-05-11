// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseScreenBreadcrumbs : OsuTestCase
    {
        private readonly ScreenBreadcrumbControl<TestScreen> breadcrumbs;
        private TestScreen currentScreen, changedScreen;

        public TestCaseScreenBreadcrumbs()
        {
            TestScreen startScreen;
            OsuSpriteText titleText;

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        breadcrumbs = new ScreenBreadcrumbControl<TestScreen>
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        titleText = new OsuSpriteText(),
                    },
                },
                currentScreen = startScreen = new TestScreenOne(),
            };

            breadcrumbs.OnScreenChanged += s =>
            {
                titleText.Text = $"Changed to {s.ToString()}";
                changedScreen = s;
            };

            AddStep(@"make start current", () => breadcrumbs.CurrentScreen = startScreen);
            assertCurrent();
            pushNext();
            assertCurrent();
            pushNext();
            assertCurrent();

            AddStep(@"make start current", () =>
            {
                startScreen.MakeCurrent();
                currentScreen = startScreen;
            });

            assertCurrent();
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Blue;
        }

        private void pushNext() => AddStep(@"push next screen", () => currentScreen = currentScreen.PushNext());
        private void assertCurrent() => AddAssert(@"assert the current screen is correct", () => currentScreen == changedScreen);

        private abstract class TestScreen : OsuScreen
        {
            protected abstract string Title { get; }
            protected abstract string NextTitle { get; }
            protected abstract TestScreen CreateNextScreen();

            public override string ToString() => Title;

            public TestScreen PushNext()
            {
                TestScreen screen = CreateNextScreen();
                Push(screen);

                return screen;
            }

            protected TestScreen()
            {
                Child = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = Title,
                        },
                        new TriangleButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 100,
                            Text = $"Push {NextTitle}",
                            Action = () => PushNext(),
                        },
                    },
                };
            }
        }

        private class TestScreenOne : TestScreen
        {
            protected override string Title => @"Screen One";
            protected override string NextTitle => @"Two";
            protected override TestScreen CreateNextScreen() => new TestScreenTwo();
        }

        private class TestScreenTwo : TestScreen
        {
            protected override string Title => @"Screen Two";
            protected override string NextTitle => @"One";
            protected override TestScreen CreateNextScreen() => new TestScreenOne();
        }
    }
}
