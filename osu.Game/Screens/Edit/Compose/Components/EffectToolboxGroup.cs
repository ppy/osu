// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Timing;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class EffectToolboxGroup : EditorToolboxGroup
    {
        private readonly bool showScrollSpeed;

        private ExpandableScrollSpeedControl? scrollSpeedControl;
        private ExpandableKiaiControl kiaiControl = null!;

        private RoundedButton applyButton = null!;
        private RoundedButton addButton = null!;

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        private EffectControlPoint? displayedPoint;
        private double displayedScrollSpeed;
        private bool displayedKiai;

        public EffectToolboxGroup(bool showScrollSpeed)
            : base("effects")
        {
            this.showScrollSpeed = showScrollSpeed;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Spacing = new Vector2(5);

            if (showScrollSpeed)
                Add(scrollSpeedControl = new ExpandableScrollSpeedControl());

            AddRange(new Drawable[]
            {
                kiaiControl = new ExpandableKiaiControl(),
                applyButton = new ToolboxButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Apply to current effect",
                    Action = applyToActive,
                },
                addButton = new ToolboxButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Add to current time",
                    Action = addAtCurrentTime,
                },
            });
        }

        protected override void Update()
        {
            base.Update();

            double time = editorClock.CurrentTimeAccurate;
            var point = editorBeatmap.ControlPointInfo.EffectPointAt(time);

            if (!ReferenceEquals(point, displayedPoint) || point.ScrollSpeed != displayedScrollSpeed || point.KiaiMode != displayedKiai)
            {
                displayedPoint = point;
                displayedScrollSpeed = point.ScrollSpeed;
                displayedKiai = point.KiaiMode;

                if (scrollSpeedControl != null)
                {
                    scrollSpeedControl.Current.Value = point.ScrollSpeed;
                    scrollSpeedControl.ContractedLabelText = $"Scroll: {point.ScrollSpeed:n2}x";
                }

                kiaiControl.Current.Value = point.KiaiMode;
                kiaiControl.ContractedLabelText = $"Kiai: {(point.KiaiMode ? "On" : "Off")}";
            }

            bool valuesMatch = selectedValuesMatch(point);

            addButton.Enabled.Value = ReferenceEquals(point, EffectControlPoint.DEFAULT) || point.Time != time || !valuesMatch;
            applyButton.Enabled.Value = !ReferenceEquals(point, EffectControlPoint.DEFAULT) && !valuesMatch;
        }

        private void applyToActive()
        {
            var active = editorBeatmap.ControlPointInfo.EffectPointAt(editorClock.CurrentTimeAccurate);

            if (ReferenceEquals(active, EffectControlPoint.DEFAULT))
                return;

            editorBeatmap.BeginChange();

            active.ScrollSpeed = scrollSpeedControl?.Current.Value ?? active.ScrollSpeed;
            active.KiaiMode = kiaiControl.Current.Value;

            editorBeatmap.EndChange();
        }

        private void addAtCurrentTime()
        {
            double time = editorClock.CurrentTimeAccurate;
            var active = editorBeatmap.ControlPointInfo.EffectPointAt(time);

            editorBeatmap.BeginChange();

            editorBeatmap.ControlPointInfo.GroupAt(time, true).Add(new EffectControlPoint
            {
                ScrollSpeed = scrollSpeedControl?.Current.Value ?? active.ScrollSpeed,
                KiaiMode = kiaiControl.Current.Value,
            });

            editorBeatmap.EndChange();
        }

        private bool selectedValuesMatch(EffectControlPoint active)
        {
            double scrollSpeed = scrollSpeedControl?.Current.Value ?? active.ScrollSpeed;

            return active.KiaiMode == kiaiControl.Current.Value && Precision.AlmostEquals(active.ScrollSpeed, scrollSpeed, 0.005);
        }

        private partial class ExpandableScrollSpeedControl : CompositeDrawable, IExpandable
        {
            private readonly OsuSpriteText contractedLabel;
            private readonly SliderVelocityAdjustmentControl adjustmentControl;

            /// <summary>
            /// The label text to display when this slider is in a contracted state.
            /// </summary>
            public LocalisableString ContractedLabelText
            {
                set => contractedLabel.Text = value;
            }

            public Bindable<double> Current => adjustmentControl.Current;

            public BindableBool Expanded { get; } = new BindableBool();

            public override bool HandlePositionalInput => true;

            public ExpandableScrollSpeedControl()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        contractedLabel = new OsuSpriteText(),
                        adjustmentControl = new SliderVelocityAdjustmentControl
                        {
                            Caption = "Scroll speed",
                        },
                    }
                };

                // Effect scroll speed can go lower than the slider velocity control's default.
                ((BindableNumber<double>)adjustmentControl.Current).MinValue = 0.01;
            }

            [Resolved]
            private IExpandingContainer? expandingContainer { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
                {
                    Expanded.Value = containerExpanded.NewValue;
                }, true);

                Expanded.BindValueChanged(v =>
                {
                    contractedLabel.FadeTo(v.NewValue ? 0 : 1);

                    adjustmentControl.FadeTo(v.NewValue ? Current.Disabled ? 0.3f : 1f : 0f, 500, Easing.OutQuint);
                    adjustmentControl.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
                }, true);

                Current.BindDisabledChanged(disabled =>
                {
                    adjustmentControl.Alpha = Expanded.Value ? disabled ? 0.3f : 1 : 0f;
                });
            }
        }

        private partial class ExpandableKiaiControl : CompositeDrawable, IExpandable
        {
            private readonly OsuSpriteText contractedLabel;
            private readonly FormCheckBox checkbox;

            /// <summary>
            /// The label text to display when this checkbox is in a contracted state.
            /// </summary>
            public LocalisableString ContractedLabelText
            {
                set => contractedLabel.Text = value;
            }

            public Bindable<bool> Current => checkbox.Current;

            public BindableBool Expanded { get; } = new BindableBool();

            public override bool HandlePositionalInput => true;

            public ExpandableKiaiControl()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        contractedLabel = new OsuSpriteText(),
                        checkbox = new FormCheckBox
                        {
                            Caption = "Kiai Time",
                        },
                    }
                };
            }

            [Resolved]
            private IExpandingContainer? expandingContainer { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
                {
                    Expanded.Value = containerExpanded.NewValue;
                }, true);

                Expanded.BindValueChanged(v =>
                {
                    contractedLabel.FadeTo(v.NewValue ? 0 : 1);

                    checkbox.FadeTo(v.NewValue ? Current.Disabled ? 0.3f : 1f : 0f, 500, Easing.OutQuint);
                    checkbox.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
                }, true);

                Current.BindDisabledChanged(disabled =>
                {
                    checkbox.Alpha = Expanded.Value ? disabled ? 0.3f : 1 : 0f;
                });
            }
        }

        private partial class ToolboxButton : RoundedButton, IExpandable
        {
            public BindableBool Expanded { get; } = new BindableBool();

            [Resolved]
            private IExpandingContainer? expandingContainer { get; set; }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                expandingContainer?.Expanded.BindValueChanged(containerExpanded =>
                {
                    Expanded.Value = containerExpanded.NewValue;
                }, true);

                Expanded.BindValueChanged(v =>
                {
                    this.FadeTo(v.NewValue ? 1 : 0, 500, Easing.OutQuint);
                    BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
                }, true);
            }
        }
    }
}
