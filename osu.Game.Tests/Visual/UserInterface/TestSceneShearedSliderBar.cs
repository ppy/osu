// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneShearedSliderBar : ThemeComparisonTestScene
    {
        private TestSliderBar slider = null!;

        protected override Drawable CreateContent() => slider = new TestSliderBar
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Current = new BindableDouble(5)
            {
                Precision = 0.1,
                MinValue = 0,
                MaxValue = 15
            },
            RelativeSizeAxes = Axes.X,
            Width = 0.4f
        };

        [Test]
        public void TestNubDisplay()
        {
            AddSliderStep("nub width", 20, 80, 50, v =>
            {
                if (slider.IsNotNull())
                {
                    slider.Nub.Width = v;
                    slider.RangePadding = v / 2f;
                }
            });
            AddToggleStep("nub shadow", v =>
            {
                if (slider.IsNotNull())
                    slider.NubShadowColour = v ? Color4.Black.Opacity(0.2f) : Color4.Black.Opacity(0f);
            });
        }

        [Test]
        public void TestNubDoubleClickRevertToDefault()
        {
            AddStep("set slider to 1", () => slider.Current.Value = 1);

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(slider.ChildrenOfType<ShearedNub>().Single()));

            AddStep("double click nub", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("slider is default", () => slider.Current.IsDefault);
        }

        [Test]
        public void TestNubDoubleClickOnDisabledSliderDoesNothing()
        {
            AddStep("set slider to 1", () => slider.Current.Value = 1);
            AddStep("disable slider", () => slider.Current.Disabled = true);

            AddStep("move mouse to nub", () => InputManager.MoveMouseTo(slider.ChildrenOfType<ShearedNub>().Single()));

            AddStep("double click nub", () =>
            {
                InputManager.Click(MouseButton.Left);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("slider is still at 1", () => slider.Current.Value, () => Is.EqualTo(1));
            AddStep("enable slider", () => slider.Current.Disabled = false);
        }

        public partial class TestSliderBar : ShearedSliderBar<double>
        {
            public new ShearedNub Nub => base.Nub;
        }
    }
}
