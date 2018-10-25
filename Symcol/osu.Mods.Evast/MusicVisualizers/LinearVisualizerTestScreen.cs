// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens.Play.PlayerSettings;

namespace osu.Mods.Evast.MusicVisualizers
{
    public class LinearVisualizerTestScreen : TestScreen
    {
        private LinearVisualizer visualizer;
        private Settings settings;

        protected override void AddTestObject(Container parent)
        {
            parent.Children = new Drawable[]
            {
                visualizer = new LinearVisualizer
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
                    visualizer.Smoothness,
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
            settings.SmoothnessBindable.ValueChanged += newValue => visualizer.Smoothness = newValue;
            settings.WidthBindable.ValueChanged += newValue => visualizer.BarWidth = newValue;
            settings.ReverseBindable.ValueChanged += newValue => visualizer.IsReversed = newValue;
            settings.SpacingBindable.ValueChanged += newValue => visualizer.Spacing = newValue;
            settings.AmountBindable.ValueChanged += newValue => visualizer.BarsAmount = newValue;
        }

        private class Settings : PlayerSettingsGroup
        {
            protected override string Title => @"settings";

            public readonly BindableFloat MultiplierBindable;
            public readonly BindableInt UpdateBindable;
            public readonly BindableInt SmoothnessBindable;
            public readonly BindableFloat WidthBindable;
            public readonly BindableBool ReverseBindable;
            public readonly BindableFloat SpacingBindable;
            public readonly BindableInt AmountBindable;

            public Settings(float multiplier, int updateDelay, int smoothnessValue, float width, float spacing, int barsAmount, bool reverse)
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
                        LabelText = "Spacing",
                        Bindable = SpacingBindable = new BindableFloat(spacing)
                        {
                            Default = spacing,
                            MinValue = 0,
                            MaxValue = 20,
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
