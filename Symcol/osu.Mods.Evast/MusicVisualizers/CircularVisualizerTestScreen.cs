// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Mods.Evast.MusicVisualizers
{
    public class CircularVisualizerTestScreen : TestScreen
    {
        private CircularVisualizer visualizer;
        private Settings settings;

        protected override void AddTestObject(Container parent)
        {
            parent.Children = new Drawable[]
            {
                visualizer = new CircularVisualizer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarWidth = 2,
                    CircleSize = 250,
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
                    visualizer.Smoothness,
                    visualizer.BarWidth,
                    visualizer.CircleSize,
                    visualizer.BarsAmount,
                    visualizer.DegreeValue,
                    visualizer.IsReversed)
            };
        }

        protected override void Connect()
        {
            settings.MultiplierBindable.ValueChanged += newValue => visualizer.ValueMultiplier = newValue;
            settings.UpdateBindable.ValueChanged += newValue => visualizer.UpdateDelay = newValue;
            settings.SmoothnessBindable.ValueChanged += newValue => visualizer.Smoothness = newValue;
            settings.WidthBindable.ValueChanged += newValue => visualizer.BarWidth = newValue;
            settings.ReverseBindable.ValueChanged += newValue => visualizer.IsReversed = newValue;
            settings.SizeBindable.ValueChanged += newValue => visualizer.CircleSize = newValue;
            settings.AmountBindable.ValueChanged += newValue => visualizer.BarsAmount = newValue;
            settings.DegreeBindable.ValueChanged += newValue => visualizer.DegreeValue = newValue;
        }

        private class Settings : PlayerSettingsGroup
        {
            protected override string Title => @"settings";

            public readonly BindableFloat MultiplierBindable;
            public readonly BindableInt UpdateBindable;
            public readonly BindableInt SmoothnessBindable;
            public readonly BindableFloat WidthBindable;
            public readonly BindableBool ReverseBindable;
            public readonly BindableFloat SizeBindable;
            public readonly BindableInt AmountBindable;
            public readonly BindableFloat DegreeBindable;

            public Settings(float multiplier, int updateDelay, int smoothnessValue, float width, float circleSize, int barsAmount, float degreeValue, bool reverse)
            {
                Children = new Drawable[]
                {
                    new PlayerSliderBar<float>
                    {
                        LabelText = "Amplitude Multiplier",
                        Bindable = MultiplierBindable = new BindableFloat(multiplier)
                        {
                            Default = multiplier,
                            MinValue = 0,
                            MaxValue = 1000,
                        }
                    },
                    new PlayerSliderBar<int>
                    {
                        LabelText = "Update Delay",
                        Bindable = UpdateBindable = new BindableInt(updateDelay)
                        {
                            Default = updateDelay,
                            MinValue = 1,
                            MaxValue = 100,
                        }
                    },
                    new PlayerSliderBar<int>
                    {
                        LabelText = "Smoothness Value",
                        Bindable = SmoothnessBindable = new BindableInt(smoothnessValue)
                        {
                            Default = smoothnessValue,
                            MinValue = 1,
                            MaxValue = 1000,
                        }
                    },
                    new PlayerSliderBar<float>
                    {
                        LabelText = "Bar Width",
                        Bindable = WidthBindable = new BindableFloat(width)
                        {
                            Default = width,
                            MinValue = 1,
                            MaxValue = 50,
                        }
                    },
                    new PlayerSliderBar<float>
                    {
                        LabelText = "Circle Size",
                        Bindable = SizeBindable = new BindableFloat(circleSize)
                        {
                            Default = circleSize,
                            MinValue = 0,
                            MaxValue = 500,
                        }
                    },
                    new PlayerSliderBar<int>
                    {
                        LabelText = "Bars Amount",
                        Bindable = AmountBindable = new BindableInt(barsAmount)
                        {
                            Default = barsAmount,
                            MinValue = 1,
                            MaxValue = 200,
                        }
                    },
                    new PlayerSliderBar<float>
                    {
                        LabelText = "Degree Value",
                        Bindable = DegreeBindable = new BindableFloat(degreeValue)
                        {
                            Default = degreeValue,
                            MinValue = 0,
                            MaxValue = 360,
                        }
                    },
                    new PlayerCheckbox
                    {
                        LabelText = "Reversed",
                        Bindable = ReverseBindable = new BindableBool(reverse) { Default = reverse }
                    }
                };
            }
        }
    }
}
