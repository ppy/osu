// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSliderDrawingSettingsProvider : Drawable
    {
        public BindableFloat Tolerance { get; } = new BindableFloat(1.5f)
        {
            MinValue = 0.05f,
            MaxValue = 3f,
            Precision = 0.01f
        };

        private readonly BindableInt sliderTolerance = new BindableInt(50)
        {
            MinValue = 5,
            MaxValue = 100
        };

        public BindableFloat CornerThreshold { get; } = new BindableFloat(0.4f)
        {
            MinValue = 0.05f,
            MaxValue = 1f,
            Precision = 0.01f
        };

        private readonly BindableInt sliderCornerThreshold = new BindableInt(40)
        {
            MinValue = 5,
            MaxValue = 100
        };

        private ExpandableSlider<int> toleranceSlider = null!;
        private ExpandableSlider<int> cornerThresholdSlider = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sliderTolerance.BindValueChanged(v =>
            {
                float newValue = v.NewValue / 33f;
                if (!Precision.AlmostEquals(newValue, Tolerance.Value))
                    Tolerance.Value = newValue;
            });
            Tolerance.BindValueChanged(v =>
            {
                int newValue = (int)Math.Round(v.NewValue * 33f);
                if (sliderTolerance.Value != newValue)
                    sliderTolerance.Value = newValue;
            });
            sliderCornerThreshold.BindValueChanged(v =>
            {
                float newValue = v.NewValue / 100f;
                if (!Precision.AlmostEquals(newValue, CornerThreshold.Value))
                    CornerThreshold.Value = newValue;
            });
            CornerThreshold.BindValueChanged(v =>
            {
                int newValue = (int)Math.Round(v.NewValue * 100f);
                if (sliderCornerThreshold.Value != newValue)
                    sliderCornerThreshold.Value = newValue;
            });
        }

        public void AttachToToolbox(ExpandingToolboxContainer toolboxContainer)
        {
            toolboxContainer.Add(new EditorToolboxGroup("slider")
            {
                Children = new Drawable[]
                {
                    toleranceSlider = new ExpandableSlider<int>
                    {
                        Current = sliderTolerance
                    },
                    cornerThresholdSlider = new ExpandableSlider<int>
                    {
                        Current = sliderCornerThreshold
                    }
                }
            });

            sliderTolerance.BindValueChanged(e =>
            {
                toleranceSlider.ContractedLabelText = $"C. P. S.: {e.NewValue:N0}";
                toleranceSlider.ExpandedLabelText = $"Control Point Spacing: {e.NewValue:N0}";
            }, true);

            sliderCornerThreshold.BindValueChanged(e =>
            {
                cornerThresholdSlider.ContractedLabelText = $"C. T.: {e.NewValue:N0}";
                cornerThresholdSlider.ExpandedLabelText = $"Corner Threshold: {e.NewValue:N0}";
            }, true);
        }
    }
}
