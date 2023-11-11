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
    public partial class OsuSliderDrawingSettingsProvider : Drawable, ISliderDrawingSettingsProvider
    {
        public BindableFloat Tolerance { get; } = new BindableFloat(0.1f)
        {
            MinValue = 0.05f,
            MaxValue = 1f,
            Precision = 0.01f
        };

        private BindableInt sliderTolerance = new BindableInt(10)
        {
            MinValue = 5,
            MaxValue = 100
        };

        private ExpandableSlider<int> toleranceSlider = null!;

        private EditorToolboxGroup? toolboxGroup;

        public OsuSliderDrawingSettingsProvider()
        {
            sliderTolerance.BindValueChanged(v =>
            {
                float newValue = v.NewValue / 100f;
                if (!Precision.AlmostEquals(newValue, Tolerance.Value, 1e-7f))
                    Tolerance.Value = newValue;
            });
            Tolerance.BindValueChanged(v =>
            {
                int newValue = (int)Math.Round(v.NewValue * 100f);
                if (sliderTolerance.Value != newValue)
                    sliderTolerance.Value = newValue;
            });
        }

        public void AttachToToolbox(ExpandingToolboxContainer toolboxContainer)
        {
            toolboxContainer.Add(toolboxGroup = new EditorToolboxGroup("drawing")
            {
                Children = new Drawable[]
                {
                    toleranceSlider = new ExpandableSlider<int>
                    {
                        Current = sliderTolerance
                    }
                }
            });

            sliderTolerance.BindValueChanged(e =>
            {
                toleranceSlider.ContractedLabelText = $"Tolerance: {e.NewValue:N0}";
                toleranceSlider.ExpandedLabelText = $"Tolerance: {e.NewValue:N0}";
            }, true);
        }
    }
}
