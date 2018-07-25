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
    public class TestCaseLabelledRadioButtonCollection : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(LabelledRadioButtonCollection),
            typeof(OsuSetupRadioButtonCollection),
            typeof(OsuSetupRadioButton),
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            LabelledRadioButtonCollection labelledRadioButtonCollection;
            OsuSetupRadioButton ok;
            OsuSetupRadioButton no;

            Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                Padding = new MarginPadding { Left = 150, Right = 150 },
                Child = labelledRadioButtonCollection = new LabelledRadioButtonCollection
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    LabelText = "Testing collection",
                    BottomLabelText = "This collection contains extraordinary options, such as \"OK!\" and \"No\".",
                    Items = new[]
                    {
                        ok = new OsuSetupRadioButton
                        {
                            LabelText = "OK!"
                        },
                        no = new OsuSetupRadioButton
                        {
                            LabelText = "No"
                        }
                    },
                }
            };

            AddAssert("Check initial selected value of the collection", () => labelledRadioButtonCollection.CurrentSelection == ok);
            AddAssert("Check value of the OK button", () => ok.Current.Value);
            AddAssert("Check value of the No button", () => !no.Current.Value);
            AddStep("Select the No button", () => labelledRadioButtonCollection.CurrentSelection = no);
            AddAssert("Check selected value of the collection", () => labelledRadioButtonCollection.CurrentSelection == no);
            AddAssert("Check value of the OK button", () => !ok.Current.Value);
            AddAssert("Check value of the No button", () => no.Current.Value);
            AddStep("Select the OK button", () => labelledRadioButtonCollection.CurrentSelection = ok);
            AddAssert("Check selected value of the collection", () => labelledRadioButtonCollection.CurrentSelection == ok);
            AddAssert("Check value of the OK button", () => ok.Current.Value);
            AddAssert("Check value of the No button", () => !no.Current.Value);
        }
    }
}
