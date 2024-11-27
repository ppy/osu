﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLabelledSliderBar : OsuTestScene
    {
        [Test]
        public void TestBasic() => createSliderBar();

        [Test]
        public void TestDescription()
        {
            createSliderBar();
            AddStep("set description", () => this.ChildrenOfType<LabelledSliderBar<double>>().ForEach(l => l.Description = "this text describes the component"));
        }

        [Test]
        public void TestSize()
        {
            createSliderBar();
            AddStep("set zero width", () => this.ChildrenOfType<LabelledSliderBar<double>>().ForEach(l => l.ResizeWidthTo(0, 200, Easing.OutQuint)));
            AddStep("set negative width", () => this.ChildrenOfType<LabelledSliderBar<double>>().ForEach(l => l.ResizeWidthTo(-1, 200, Easing.OutQuint)));
            AddStep("revert back", () => this.ChildrenOfType<LabelledSliderBar<double>>().ForEach(l => l.ResizeWidthTo(1, 200, Easing.OutQuint)));
        }

        [Test]
        public void TestDisable()
        {
            createSliderBar();
            AddStep("set disabled", () => this.ChildrenOfType<LabelledSliderBar<double>>().ForEach(l => l.Current.Disabled = true));
            AddStep("unset disabled", () => this.ChildrenOfType<LabelledSliderBar<double>>().ForEach(l => l.Current.Disabled = false));
        }

        private void createSliderBar()
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
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Current = new BindableDouble(5)
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 1,
                            },
                            Label = "a sample component",
                        },
                    },
                };

                foreach (var colour in Enum.GetValues(typeof(OverlayColourScheme)).OfType<OverlayColourScheme>())
                {
                    flow.Add(new OverlayColourContainer(colour)
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Child = new LabelledSliderBar<double>
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Current = new BindableDouble(5)
                            {
                                MinValue = 0,
                                MaxValue = 10,
                                Precision = 1,
                            },
                            Label = "a sample component",
                        }
                    });
                }
            });
        }

        private partial class OverlayColourContainer : Container
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
