// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Edit
{
    public partial class GridToolboxGroup : EditorToolboxGroup
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public GridToolboxGroup()
            : base("grid")
        {
        }

        public BindableFloat StartPositionX { get; } = new BindableFloat(256f)
        {
            MinValue = 0f,
            MaxValue = 512f,
            Precision = 1f
        };

        public BindableFloat StartPositionY { get; } = new BindableFloat(192)
        {
            MinValue = 0f,
            MaxValue = 384f,
            Precision = 1f
        };

        public BindableFloat Spacing { get; } = new BindableFloat(4f)
        {
            MinValue = 4f,
            MaxValue = 128f,
            Precision = 1f
        };

        public BindableFloat GridLinesRotation { get; } = new BindableFloat(0f)
        {
            MinValue = -180f,
            MaxValue = 180f,
            Precision = 1f
        };

        private ExpandableSlider<float> startPositionXSlider = null!;
        private ExpandableSlider<float> startPositionYSlider = null!;
        private ExpandableSlider<float> spacingSlider = null!;
        private ExpandableSlider<float> gridLinesRotationSlider = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                startPositionXSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionX
                },
                startPositionYSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionY
                },
                spacingSlider = new ExpandableSlider<float>
                {
                    Current = Spacing
                },
                gridLinesRotationSlider = new ExpandableSlider<float>
                {
                    Current = GridLinesRotation
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            StartPositionX.BindValueChanged(x =>
            {
                startPositionXSlider.ContractedLabelText = $"X: {x.NewValue:N0}";
                startPositionXSlider.ExpandedLabelText = $"X Offset: {x.NewValue:N0}";
            }, true);

            StartPositionY.BindValueChanged(y =>
            {
                startPositionYSlider.ContractedLabelText = $"Y: {y.NewValue:N0}";
                startPositionYSlider.ExpandedLabelText = $"Y Offset: {y.NewValue:N0}";
            }, true);

            Spacing.BindValueChanged(spacing =>
            {
                spacingSlider.ContractedLabelText = $"S: {spacing.NewValue:N0}";
                spacingSlider.ExpandedLabelText = $"Spacing: {spacing.NewValue:N0}";
            }, true);

            GridLinesRotation.BindValueChanged(rotation =>
            {
                gridLinesRotationSlider.ContractedLabelText = $"R: {rotation.NewValue:N0}";
                gridLinesRotationSlider.ExpandedLabelText = $"Rotation: {rotation.NewValue:N0}";
            }, true);
        }
    }
}
