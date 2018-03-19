// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.ReplaySettings;

namespace osu.Game.Screens.Evast.MusicVisualizers
{
    public class LinearVisualizerTestScreen : TestScreen
    {
        private LinearVisualizer visualizer;
        private Settings settings;

        protected override void AddTestObject(Container parent)
        {
            parent.Children = new Drawable[]
            {
                visualizer = new LinearVisualizer()
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    BarWidth = 2,
                },
            };
        }

        protected override void AddSettings(FillFlowContainer parent)
        {
            parent.Children = new Drawable[]
            {
                settings = new Settings(
                    visualizer.ValueMultiplier,
                    visualizer.UpdateDelay,
                    visualizer.Softness,
                    visualizer.BarWidth,
                    visualizer.Spacing,
                    visualizer.BarsAmount,
                    visualizer.IsReversed)
            };
        }

        protected override void Connect()
        {
            settings.MultiplierBindable.ValueChanged += newValue => visualizer.ValueMultiplier = newValue;
            settings.UpdateBindable.ValueChanged += newValue => visualizer.UpdateDelay = newValue;
            settings.SoftnessBindable.ValueChanged += newValue => visualizer.Softness = newValue;
            settings.WidthBindable.ValueChanged += newValue => visualizer.BarWidth = newValue;
            settings.ReverseBindable.ValueChanged += newValue => visualizer.IsReversed = newValue;
            settings.SpacingBindable.ValueChanged += newValue => visualizer.Spacing = newValue;
            settings.AmountBindable.ValueChanged += newValue => visualizer.BarsAmount = newValue;
        }

        private class Settings : ReplayGroup
        {
            protected override string Title => @"settings";

            public readonly BindableFloat MultiplierBindable;
            public readonly BindableInt UpdateBindable;
            public readonly BindableInt SoftnessBindable;
            public readonly BindableFloat WidthBindable;
            public readonly BindableBool ReverseBindable;
            public readonly BindableFloat SpacingBindable;
            public readonly BindableInt AmountBindable;

            public Settings(float multiplier, int updateDelay, int softnessValue, float width, float spacing, int barsAmount, bool reverse)
            {
                Children = new Drawable[]
                {
                    new ReplaySliderBar<float>
                    {
                        LabelText = "Amplitude Multiplier",
                        Bindable = MultiplierBindable = new BindableFloat(multiplier)
                        {
                            Default = multiplier,
                            MinValue = 0,
                            MaxValue = 1000,
                        }
                    },
                    new ReplaySliderBar<int>
                    {
                        LabelText = "Update Delay",
                        Bindable = UpdateBindable = new BindableInt(updateDelay)
                        {
                            Default = updateDelay,
                            MinValue = 1,
                            MaxValue = 100,
                        }
                    },
                    new ReplaySliderBar<int>
                    {
                        LabelText = "Softness Value",
                        Bindable = SoftnessBindable = new BindableInt(softnessValue)
                        {
                            Default = softnessValue,
                            MinValue = 1,
                            MaxValue = 1000,
                        }
                    },
                    new ReplaySliderBar<float>
                    {
                        LabelText = "Bar Width",
                        Bindable = WidthBindable = new BindableFloat(width)
                        {
                            Default = width,
                            MinValue = 1,
                            MaxValue = 50,
                        }
                    },
                    new ReplaySliderBar<float>
                    {
                        LabelText = "Spacing",
                        Bindable = SpacingBindable = new BindableFloat(spacing)
                        {
                            Default = spacing,
                            MinValue = 0,
                            MaxValue = 20,
                        }
                    },
                    new ReplaySliderBar<int>
                    {
                        LabelText = "Bars Amount",
                        Bindable = AmountBindable = new BindableInt(barsAmount)
                        {
                            Default = barsAmount,
                            MinValue = 1,
                            MaxValue = 200,
                        }
                    },
                    new ReplayCheckbox
                    {
                        LabelText = "Reversed",
                        Bindable = ReverseBindable = new BindableBool(reverse) { Default = reverse }
                    }
                };
            }
        }
    }
}
