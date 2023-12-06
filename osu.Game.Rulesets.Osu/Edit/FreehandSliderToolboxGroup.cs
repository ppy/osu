// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class FreehandSliderToolboxGroup : EditorToolboxGroup
    {
        public FreehandSliderToolboxGroup()
            : base("slider")
        {
        }

        public BindableFloat Tolerance { get; } = new BindableFloat(2f)
        {
            MinValue = 0.05f,
            MaxValue = 2.0f,
            Precision = 0.01f
        };

        public BindableFloat CornerThreshold { get; } = new BindableFloat(0.4f)
        {
            MinValue = 0.05f,
            MaxValue = 1f,
            Precision = 0.01f
        };

        public BindableFloat CircleThreshold { get; } = new BindableFloat(0.002f)
        {
            MinValue = 0f,
            MaxValue = 0.005f,
            Precision = 0.0001f
        };

        // We map internal ranges to a more standard range of values for display to the user.
        private readonly BindableInt displayTolerance = new BindableInt(100)
        {
            MinValue = 5,
            MaxValue = 100
        };

        private readonly BindableInt displayCornerThreshold = new BindableInt(40)
        {
            MinValue = 5,
            MaxValue = 100
        };

        private readonly BindableInt displayCircleThreshold = new BindableInt(40)
        {
            MinValue = 0,
            MaxValue = 100
        };

        private ExpandableSlider<int> toleranceSlider = null!;
        private ExpandableSlider<int> cornerThresholdSlider = null!;
        private ExpandableSlider<int> circleThresholdSlider = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                toleranceSlider = new ExpandableSlider<int>
                {
                    Current = displayTolerance
                },
                cornerThresholdSlider = new ExpandableSlider<int>
                {
                    Current = displayCornerThreshold
                },
                circleThresholdSlider = new ExpandableSlider<int>
                {
                    Current = displayCircleThreshold
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            displayTolerance.BindValueChanged(tolerance =>
            {
                toleranceSlider.ContractedLabelText = $"C. P. S.: {tolerance.NewValue:N0}";
                toleranceSlider.ExpandedLabelText = $"Control Point Spacing: {tolerance.NewValue:N0}";

                Tolerance.Value = displayToInternalTolerance(tolerance.NewValue);
            }, true);

            displayCornerThreshold.BindValueChanged(threshold =>
            {
                cornerThresholdSlider.ContractedLabelText = $"C. T.: {threshold.NewValue:N0}";
                cornerThresholdSlider.ExpandedLabelText = $"Corner Threshold: {threshold.NewValue:N0}";

                CornerThreshold.Value = displayToInternalCornerThreshold(threshold.NewValue);
            }, true);

            displayCircleThreshold.BindValueChanged(threshold =>
            {
                circleThresholdSlider.ContractedLabelText = $"P. C. T.: {threshold.NewValue:N0}";
                circleThresholdSlider.ExpandedLabelText = $"Perfect Curve Threshold: {threshold.NewValue:N0}";

                CircleThreshold.Value = displayToInternalCircleThreshold(threshold.NewValue);
            }, true);

            Tolerance.BindValueChanged(tolerance =>
                displayTolerance.Value = internalToDisplayTolerance(tolerance.NewValue)
            );
            CornerThreshold.BindValueChanged(threshold =>
                displayCornerThreshold.Value = internalToDisplayCornerThreshold(threshold.NewValue)
            );
            CircleThreshold.BindValueChanged(threshold =>
                displayCircleThreshold.Value = internalToDisplayCircleThreshold(threshold.NewValue)
            );

            float displayToInternalTolerance(float v) => v / 50f;
            int internalToDisplayTolerance(float v) => (int)Math.Round(v * 50f);

            float displayToInternalCornerThreshold(float v) => v / 100f;
            int internalToDisplayCornerThreshold(float v) => (int)Math.Round(v * 100f);

            float displayToInternalCircleThreshold(float v) => v / 20000f;
            int internalToDisplayCircleThreshold(float v) => (int)Math.Round(v * 20000f);
        }
    }
}
