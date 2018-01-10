// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    public class TestCaseSliderBarPrecision : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[] { typeof(OsuSliderBar<>) };

        private readonly BindableInt intValue;
        private readonly BindableFloat floatValue;
        private readonly BindableDouble doubleValue;

        private readonly TestSliderBar<int> intSliderBar;
        private readonly TestSliderBar<float> floatSliderBar;
        private readonly TestSliderBar<double> doubleSliderBar;

        public TestCaseSliderBarPrecision()
        {
            intValue = new BindableInt
            {
                MinValue = -1000,
                MaxValue = 1000,
            };

            floatValue = new BindableFloat
            {
                MinValue = -1000,
                MaxValue = 1000,
            };

            doubleValue = new BindableDouble
            {
                MinValue = -1000,
                MaxValue = 1000
            };

            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                Width = 300,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(0, 20),
                Children = new Drawable[]
                {
                    intSliderBar = new TestSliderBar<int> { RelativeSizeAxes = Axes.X },
                    floatSliderBar = new TestSliderBar<float> { RelativeSizeAxes = Axes.X },
                    doubleSliderBar = new TestSliderBar<double> { RelativeSizeAxes = Axes.X }
                }
            };

            intSliderBar.Current.BindTo(intValue);
            floatSliderBar.Current.BindTo(floatValue);
            doubleSliderBar.Current.BindTo(doubleValue);

            intValue.ValueChanged += setValue;
            floatValue.ValueChanged += setValue;
            doubleValue.ValueChanged += setValue;

            AddStep("Value = 0", () => setValue(0));
            AddStep("Digits = 0", () => setDecimalDigits(0));
            AddAssert("Check all 0", () => checkExact("0"));

            AddStep("Digits = 3", () => setDecimalDigits(3));
            AddAssert("Check 0.000", () => checkExact(0.000m));

            AddStep("Value = 0.5", () => setValue(0.5));
            AddAssert("Check 0.500", () => checkExact(0.500m));

            AddStep("Value = 123.4567", () => setValue(123.4567));
            AddAssert("Check 123.457", () => checkExact(123.457m));

            AddStep("Value = 765.4312", () => setValue(765.4312));
            AddAssert("Check 765.431", () => checkExact(765.431m));

            AddStep("Value = -12.3456", () => setValue(-12.3456));
            AddAssert("Check -12.346", () => checkExact(-12.346m));
            AddStep("Digits = 1", () => setDecimalDigits(1));
            AddAssert("Check -12.3", () => checkExact(-12.3m));
            AddStep("Digits = 0", () => setDecimalDigits(0));
            AddAssert("Check -12", () => checkExact(-12m));

            AddStep("Value = -12.8", () => setValue(-12.8));
            AddAssert("Check -13", () => checkExact(-13m));
            AddStep("Digits = 1", () => setDecimalDigits(1));
            AddAssert("Check -12.8", () => checkExact(-12.8m));

            AddSliderStep("Digits", 0, 7, 1, setDecimalDigits);
        }

        /// <summary>
        /// Checks whether all sliderbar tooltips display an exact value.
        /// </summary>
        /// <param name="value">The expected value that should be displayed.</param>
        private bool checkExact(string value)
            => intSliderBar.TooltipText == value
               && floatSliderBar.TooltipText == value
               && doubleSliderBar.TooltipText == value;

        /// <summary>
        /// Checks whether all sliderbar tooltips display an exact value.
        /// </summary>
        /// <param name="value">The expected value that should be displayed.</param>
        private bool checkExact(decimal value)
        {
            var expectedDecimal = value.ToString(intSliderBar.Format);

            return intSliderBar.TooltipText == Convert.ToInt32(value).ToString("N0")
                   && floatSliderBar.TooltipText == expectedDecimal
                   && doubleSliderBar.TooltipText == expectedDecimal;
        }

        /// <summary>
        /// Checks whether all floating-point sliderbar tooltips have a certain number of decimal digits.
        /// </summary>
        /// <param name="decimals">The expected number of decimal digits.</param>
        private bool checkDecimalDigits(int decimals)
            => checkDecimalDigits(decimals, floatSliderBar.TooltipText)
               && checkDecimalDigits(decimals, doubleSliderBar.TooltipText);

        private bool checkDecimalDigits(int decimals, string value)
            => value.Length - value.IndexOf(intSliderBar.Format.NumberDecimalSeparator, StringComparison.InvariantCulture) - 1 == decimals;

        private void setValue<T>(T value)
        {
            intValue.Value = Convert.ToInt32(value);
            floatValue.Value = Convert.ToSingle(value);
            doubleValue.Value = Convert.ToDouble(value);
        }

        private void setDecimalDigits(int digits)
        {
            intSliderBar.Format.NumberDecimalDigits = digits;
            floatSliderBar.Format.NumberDecimalDigits = digits;
            doubleSliderBar.Format.NumberDecimalDigits = digits;

            // Make sure that the text referenced in TestSliderBar is updated
            // This doesn't break any assertions if missing, but breaks the visual display
            intSliderBar.Current.TriggerChange();
            floatSliderBar.Current.TriggerChange();
            doubleSliderBar.Current.TriggerChange();
        }

        private class TestSliderBar<T> : OsuSliderBar<T>
            where T : struct, IEquatable<T>, IComparable, IConvertible
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
