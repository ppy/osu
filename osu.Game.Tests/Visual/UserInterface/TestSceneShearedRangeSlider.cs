// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearedRangeSlider : ThemeComparisonTestScene
    {
        private readonly BindableNumber<double> customStart = new BindableNumber<double>
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = 0.1f
        };

        private readonly BindableNumber<double> customEnd = new BindableNumber<double>(10)
        {
            MinValue = 0,
            MaxValue = 10,
            Precision = 0.1f
        };

        private ShearedRangeSlider shearedRangeSlider = null!;

        public TestSceneShearedRangeSlider()
            : base(false)
        {
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            CreateThemedContent(OverlayColourScheme.Aquamarine);
        }

        protected override Drawable CreateContent() => new Container
        {
            RelativeSizeAxes = Axes.Both,
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                shearedRangeSlider = new ShearedRangeSlider("Test")
                {
                    Width = 600,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(1),
                    LowerBound = customStart,
                    UpperBound = customEnd,
                    NubWidth = 32,
                    DefaultStringLowerBound = "0.0",
                    DefaultStringUpperBound = "∞",
                    MinRange = 0.1f,
                }
            }
        };

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset range", () =>
            {
                customStart.SetDefault();
                customEnd.SetDefault();
            });

            AddAssert("Initial lower bound is correct", () => shearedRangeSlider.LowerBound.Value, () => Is.EqualTo(0).Within(0.1f));
            AddAssert("Initial upper bound is correct", () => shearedRangeSlider.UpperBound.Value, () => Is.EqualTo(10).Within(0.1f));
        }

        [Test]
        public void TestAdjustRange()
        {
            AddStep("Adjust range", () =>
            {
                customStart.Value = 5;
                customEnd.Value = 7.5;
            });

            AddAssert("Adjusted lower bound is correct", () => shearedRangeSlider.LowerBound.Value, () => Is.EqualTo(5).Within(0.1f));
            AddAssert("Adjusted upper bound is correct", () => shearedRangeSlider.UpperBound.Value, () => Is.EqualTo(7.5).Within(0.1f));

            AddStep("Test nub pushing", () =>
            {
                customStart.Value = 9;
            });

            AddAssert("Pushed lower bound is correct", () => shearedRangeSlider.LowerBound.Value, () => Is.EqualTo(9).Within(0.1f));
            AddAssert("Pushed upper bound is correct", () => shearedRangeSlider.UpperBound.Value, () => Is.EqualTo(9.1).Within(0.1f));
        }

        [Test]
        public void TestAdjustRangeClickOutsideNub()
        {
            Vector2 lowerBoundNub = Vector2.Zero;
            Vector2 upperBoundNub = Vector2.Zero;

            AddStep("click 75%", () =>
            {
                // save out original positions so we can use as absolute selection range.
                lowerBoundNub = shearedRangeSlider.ChildrenOfType<ShearedNub>().Last().ScreenSpaceDrawQuad.Centre - OsuGame.SHEAR * 2;
                upperBoundNub = shearedRangeSlider.ChildrenOfType<ShearedNub>().First().ScreenSpaceDrawQuad.Centre - OsuGame.SHEAR * 2;

                InputManager.MoveMouseTo(lowerBoundNub + new Vector2((upperBoundNub.X - lowerBoundNub.X) * 0.75f, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Adjusted lower bound is correct", () => shearedRangeSlider.LowerBound.Value, () => Is.EqualTo(0).Within(0.11f));
            AddAssert("Adjusted upper bound is correct", () => shearedRangeSlider.UpperBound.Value, () => Is.EqualTo(7.5).Within(0.11f));

            AddStep("click 30%", () =>
            {
                InputManager.MoveMouseTo(lowerBoundNub + new Vector2((upperBoundNub.X - lowerBoundNub.X) * 0.3f, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Adjusted lower bound is correct", () => shearedRangeSlider.LowerBound.Value, () => Is.EqualTo(3.0).Within(0.11f));
            AddAssert("Adjusted upper bound is correct", () => shearedRangeSlider.UpperBound.Value, () => Is.EqualTo(7.5).Within(0.11f));

            AddStep("click 0%", () =>
            {
                InputManager.MoveMouseTo(lowerBoundNub);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Adjusted lower bound is correct", () => shearedRangeSlider.LowerBound.Value, () => Is.EqualTo(0).Within(0.11f));
            AddAssert("Adjusted upper bound is correct", () => shearedRangeSlider.UpperBound.Value, () => Is.EqualTo(7.5).Within(0.11f));
        }
    }
}
