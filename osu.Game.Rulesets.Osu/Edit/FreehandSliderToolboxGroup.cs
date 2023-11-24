﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        public BindableFloat Tolerance { get; } = new BindableFloat(1.5f)
        {
            MinValue = 0.05f,
            MaxValue = 3f,
            Precision = 0.01f
        };

        public BindableFloat CornerThreshold { get; } = new BindableFloat(0.4f)
        {
            MinValue = 0.05f,
            MaxValue = 1f,
            Precision = 0.01f
        };

        // We map internal ranges to a more standard range of values for display to the user.
        private readonly BindableInt displayTolerance = new BindableInt(40)
        {
            MinValue = 5,
            MaxValue = 100
        };

        private readonly BindableInt displayCornerThreshold = new BindableInt(40)
        {
            MinValue = 5,
            MaxValue = 100
        };

        private ExpandableSlider<int> toleranceSlider = null!;
        private ExpandableSlider<int> cornerThresholdSlider = null!;

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

            Tolerance.BindValueChanged(tolerance =>
                displayTolerance.Value = internalToDisplayTolerance(tolerance.NewValue)
            );
            CornerThreshold.BindValueChanged(threshold =>
                displayCornerThreshold.Value = internalToDisplayCornerThreshold(threshold.NewValue)
            );

            float displayToInternalTolerance(float v) => v / 33f;
            int internalToDisplayTolerance(float v) => (int)Math.Round(v * 33f);

            float displayToInternalCornerThreshold(float v) => v / 100f;
            int internalToDisplayCornerThreshold(float v) => (int)Math.Round(v * 100f);
        }
    }
}
