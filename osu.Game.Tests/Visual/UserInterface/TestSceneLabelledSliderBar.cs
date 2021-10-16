// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

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
                FillFlowContainer flow;

                Child = flow = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 500,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        new LabelledSliderBar<double>
                        {
                            Current = new BindableDouble(5)
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 1,
                            },
                            Label = "a sample component",
                            Description = hasDescription ? "this text describes the component" : string.Empty,
                        },
                    },
                };

                foreach (var colour in Enum.GetValues(typeof(OverlayColourScheme)).OfType<OverlayColourScheme>())
                {
                    flow.Add(new OverlayColourContainer(colour)
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new LabelledSliderBar<double>
                        {
                            Current = new BindableDouble(5)
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 1,
                            },
                            Label = "a sample component",
                            Description = hasDescription ? "this text describes the component" : string.Empty,
                        }
                    });
                }
            });
        }

        private class OverlayColourContainer : Container
        {
            [Cached]
            private OverlayColourProvider colourProvider;

            public OverlayColourContainer(OverlayColourScheme scheme)
            {
                colourProvider = new OverlayColourProvider(scheme);
            }
        }
    }
}
