// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Edit.Screens.Setup.Components;
using osu.Game.Screens.Edit.Screens.Setup.Components.LabelledComponents;
using System;
using System.Collections.Generic;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseLabelledSwitchButton : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledSwitchButton),
            typeof(OsuSetupSwitchButton),
        };

        private int count = -1;

        [BackgroundDependencyLoader]
        private void load()
        {
            LabelledSwitchButton labelledSwitchButton;
            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Left = 150, Right = 150 },
                Child = labelledSwitchButton = new LabelledSwitchButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Secret Feature",
                }
            };

            labelledSwitchButton.Current.ValueChanged += a =>
            {
                count += a ? 1 : 0;
                labelledSwitchButton.BottomLabelText = a ? $"Thanks for {(count > 0 ? "re-" : "")}enabling this useful secret feature{(count > 0 ? $" for the {count}{getOrderedNumberSuffix(count)} time" : "")}. Unfortunately, we cannot tell you what this does as it is secret."
                                                         : "Why did you disable this? :(";
            };

            AddStep("Set value to true", () => labelledSwitchButton.Current.Value = true);
            AddStep("Set value to false", () => labelledSwitchButton.Current.Value = false);
        }

        private string getOrderedNumberSuffix(int n)
        {
            if (n % 100 / 10 == 1)
                return "th";
            else
            {
                switch (n % 10)
                {
                    case 1:
                        return "st";
                    case 2:
                        return "nd";
                    case 3:
                        return "rd";
                    default:
                        return "th";
                }
            }
        }
    }
}
