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
                startScreen = new TestScreenOne(),
            };

            breadcrumbs.OnScreenChanged += s => titleText.Text = $"Changed to {s.ToString()}";
            breadcrumbs.CurrentScreen = startScreen;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Blue;
        }

        private abstract class TestScreen : OsuScreen
        {
            protected abstract string Title { get; }
            protected abstract string NextTitle { get; }
            protected abstract TestScreen CreateNextScreen();

            public override string ToString() => Title;

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
                            Action = () => Push(CreateNextScreen()),
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
