// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseSliderBarPercentage : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(OsuSliderBar<>) };

        private readonly BindableFloat floatValue;
        private readonly BindableDouble doubleValue;

        private readonly TestSliderBar<float> floatSliderBar;
        private readonly TestSliderBar<double> doubleSliderBar;

        public TestCaseSliderBarPercentage()
        {
            floatValue = new BindableFloat
            {
                MinValue = -1,
                MaxValue = 1,
            };

            doubleValue = new BindableDouble
            {
                MinValue = -1,
                MaxValue = 1
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    floatSliderBar = new TestSliderBar<float> { RelativeSizeAxes = Axes.X },
                    doubleSliderBar = new TestSliderBar<double> { RelativeSizeAxes = Axes.X }
                }
            };

            floatSliderBar.Current.BindTo(floatValue);
            doubleSliderBar.Current.BindTo(doubleValue);

            floatValue.ValueChanged += setValue;
            doubleValue.ValueChanged += setValue;

            AddStep("Digits = 0", () => setPercentageDigits(0));
            AddStep("Value = 0", () => setValue(0));
            AddAssert("Check 0%", () => checkExact(0));

            AddStep("Value = 0.5", () => setValue(0.5));
            AddAssert("Check 50%", () => checkExact(0.5m));

            AddStep("Value = 0.54", () => setValue(0.54));
            AddAssert("Check 54%", () => checkExact(0.54m));

            AddStep("Value = 0.544", () => setValue(0.544));
            AddAssert("Check 54%", () => checkExact(0.54m));

            AddStep("Value = 0.548", () => setValue(0.548));
            AddAssert("Check 55%", () => checkExact(0.55m));

            AddStep("Digits = 1", () => setPercentageDigits(1));
            AddAssert("Check 54.8%", () => checkExact(0.548m));

            AddSliderStep("Percentage", -1.0, 1.0, 0.0, setValue);
            AddSliderStep("Digits", 0, 7, 1, setPercentageDigits);
        }

        private bool checkExact(decimal percentage)
        {
            string expectedValue = percentage.ToString("P", floatSliderBar.Format);
            return floatSliderBar.TooltipText == expectedValue && doubleSliderBar.TooltipText == expectedValue;
        }

        private void setValue<T>(T value)
        {
            floatValue.Value = Convert.ToSingle(value);
            doubleValue.Value = Convert.ToDouble(value);
        }

        private void setPercentageDigits(int digits)
        {
            floatSliderBar.Format.PercentDecimalDigits = digits;
            doubleSliderBar.Format.PercentDecimalDigits = digits;

            // Make sure that the text referenced in TestSliderBar is updated
            // This doesn't break any assertions if missing, but breaks the visual display
            floatSliderBar.Current.TriggerChange();
            doubleSliderBar.Current.TriggerChange();
        }

        private class TestSliderBar<T> : OsuSliderBar<T>
            where T : struct, IEquatable<T>
        {
            public TestSliderBar()
            {
                SpriteText valueText;
                AddInternal(valueText = new OsuSpriteText
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreLeft,
                    X = 5,
                    Text = TooltipText
                });

                Current.ValueChanged += v => valueText.Text = TooltipText;
            }
        }
    }
}
