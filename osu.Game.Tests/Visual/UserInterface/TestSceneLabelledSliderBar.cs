// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneLabelledSliderBar : OsuTestScene
    {
        [TestCase(false)]
        [TestCase(true)]
        public void TestSliderBar(bool hasDescription) => createSliderBar(hasDescription);

        private void createSliderBar(bool hasDescription = false)
        {
            AddStep("create component", () =>
            {
                LabelledSliderBar<double> component;

                Child = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Child = component = new LabelledSliderBar<double>
                    {
                        Current = new BindableDouble(5)
                        {
                            MinValue = 0,
                            MaxValue = 10,
                            Precision = 1,
                        }
                    }
                };

                component.Label = "a sample component";
                component.Description = hasDescription ? "this text describes the component" : string.Empty;
            });
        }
    }
}
