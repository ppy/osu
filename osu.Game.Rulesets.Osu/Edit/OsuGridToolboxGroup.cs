﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components.RadioButtons;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuGridToolboxGroup : EditorToolboxGroup, IKeyBindingHandler<GlobalAction>
    {
        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [Resolved]
        private IExpandingContainer? expandingContainer { get; set; }

        /// <summary>
        /// X position of the grid's origin.
        /// </summary>
        public BindableFloat StartPositionX { get; } = new BindableFloat(OsuPlayfield.BASE_SIZE.X / 2)
        {
            MinValue = 0f,
            MaxValue = OsuPlayfield.BASE_SIZE.X,
            Precision = 0.01f,
        };

        /// <summary>
        /// Y position of the grid's origin.
        /// </summary>
        public BindableFloat StartPositionY { get; } = new BindableFloat(OsuPlayfield.BASE_SIZE.Y / 2)
        {
            MinValue = 0f,
            MaxValue = OsuPlayfield.BASE_SIZE.Y,
            Precision = 0.01f,
        };

        /// <summary>
        /// The spacing between grid lines.
        /// </summary>
        public BindableFloat Spacing { get; } = new BindableFloat(4f)
        {
            MinValue = 4f,
            MaxValue = 128f,
            Precision = 0.01f,
        };

        /// <summary>
        /// Rotation of the grid lines in degrees.
        /// </summary>
        public BindableFloat GridLinesRotation { get; } = new BindableFloat(0f)
        {
            MinValue = -180f,
            MaxValue = 180f,
            Precision = 0.01f,
        };

        /// <summary>
        /// Read-only bindable representing the grid's origin.
        /// Equivalent to <code>new Vector2(StartPositionX, StartPositionY)</code>
        /// </summary>
        public Bindable<Vector2> StartPosition { get; } = new Bindable<Vector2>(OsuPlayfield.BASE_SIZE / 2);

        /// <summary>
        /// Read-only bindable representing the grid's spacing in both the X and Y dimension.
        /// Equivalent to <code>new Vector2(Spacing)</code>
        /// </summary>
        public Bindable<Vector2> SpacingVector { get; } = new Bindable<Vector2>();

        public Bindable<PositionSnapGridType> GridType { get; } = new Bindable<PositionSnapGridType>();

        private ExpandableSlider<float> startPositionXSlider = null!;
        private ExpandableSlider<float> startPositionYSlider = null!;
        private ExpandableSlider<float> spacingSlider = null!;
        private ExpandableSlider<float> gridLinesRotationSlider = null!;
        private EditorRadioButtonCollection gridTypeButtons = null!;

        public OsuGridToolboxGroup()
            : base("grid")
        {
        }

        private const float max_automatic_spacing = 64;

        public void SetGridFromPoints(Vector2 point1, Vector2 point2)
        {
            StartPositionX.Value = point1.X;
            StartPositionY.Value = point1.Y;

            // Get the angle between the two points and normalize to the valid range.
            if (!GridLinesRotation.Disabled)
            {
                float period = GridLinesRotation.MaxValue - GridLinesRotation.MinValue;
                GridLinesRotation.Value = normalizeRotation(MathHelper.RadiansToDegrees(MathF.Atan2(point2.Y - point1.Y, point2.X - point1.X)), period);
            }

            // Divide the distance so that there is a good density of grid lines.
            // This matches the maximum grid size of the grid size cycling hotkey.
            float dist = Vector2.Distance(point1, point2);
            while (dist >= max_automatic_spacing)
                dist /= 2;
            Spacing.Value = dist;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                startPositionXSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionX,
                    KeyboardStep = 1,
                },
                startPositionYSlider = new ExpandableSlider<float>
                {
                    Current = StartPositionY,
                    KeyboardStep = 1,
                },
                spacingSlider = new ExpandableSlider<float>
                {
                    Current = Spacing,
                    KeyboardStep = 1,
                },
                gridLinesRotationSlider = new ExpandableSlider<float>
                {
                    Current = GridLinesRotation,
                    KeyboardStep = 1,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(0f, 10f),
                    Children = new Drawable[]
                    {
                        gridTypeButtons = new EditorRadioButtonCollection
                        {
                            RelativeSizeAxes = Axes.X,
                            Items = new[]
                            {
                                new RadioButton("Square",
                                    () => GridType.Value = PositionSnapGridType.Square,
                                    () => new SpriteIcon { Icon = FontAwesome.Regular.Square }),
                                new RadioButton("Triangle",
                                    () => GridType.Value = PositionSnapGridType.Triangle,
                                    () => new OutlineTriangle(true, 20)),
                                new RadioButton("Circle",
                                    () => GridType.Value = PositionSnapGridType.Circle,
                                    () => new SpriteIcon { Icon = FontAwesome.Regular.Circle }),
                            }
                        },
                    }
                },
            };

            Spacing.Value = editorBeatmap.GridSize;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            gridTypeButtons.Items.First().Select();

            StartPositionX.BindValueChanged(x =>
            {
                startPositionXSlider.ContractedLabelText = $"X: {x.NewValue:#,0.##}";
                startPositionXSlider.ExpandedLabelText = $"X Offset: {x.NewValue:#,0.##}";
                StartPosition.Value = new Vector2(x.NewValue, StartPosition.Value.Y);
            }, true);

            StartPositionY.BindValueChanged(y =>
            {
                startPositionYSlider.ContractedLabelText = $"Y: {y.NewValue:#,0.##}";
                startPositionYSlider.ExpandedLabelText = $"Y Offset: {y.NewValue:#,0.##}";
                StartPosition.Value = new Vector2(StartPosition.Value.X, y.NewValue);
            }, true);

            StartPosition.BindValueChanged(pos =>
            {
                StartPositionX.Value = pos.NewValue.X;
                StartPositionY.Value = pos.NewValue.Y;
            });

            Spacing.BindValueChanged(spacing =>
            {
                spacingSlider.ContractedLabelText = $"S: {spacing.NewValue:#,0.##}";
                spacingSlider.ExpandedLabelText = $"Spacing: {spacing.NewValue:#,0.##}";
                SpacingVector.Value = new Vector2(spacing.NewValue);
                editorBeatmap.GridSize = (int)spacing.NewValue;
            }, true);

            GridLinesRotation.BindValueChanged(rotation =>
            {
                gridLinesRotationSlider.ContractedLabelText = $"R: {rotation.NewValue:#,0.##}";
                gridLinesRotationSlider.ExpandedLabelText = $"Rotation: {rotation.NewValue:#,0.##}";
            }, true);

            GridType.BindValueChanged(v =>
            {
                GridLinesRotation.Disabled = v.NewValue == PositionSnapGridType.Circle;

                gridTypeButtons.Items[(int)v.NewValue].Select();

                switch (v.NewValue)
                {
                    case PositionSnapGridType.Square:
                        GridLinesRotation.Value = normalizeRotation(GridLinesRotation.Value, 90);
                        GridLinesRotation.MinValue = -45;
                        GridLinesRotation.MaxValue = 45;
                        break;

                    case PositionSnapGridType.Triangle:
                        GridLinesRotation.Value = normalizeRotation(GridLinesRotation.Value, 60);
                        GridLinesRotation.MinValue = -30;
                        GridLinesRotation.MaxValue = 30;
                        break;
                }
            }, true);

            expandingContainer?.Expanded.BindValueChanged(v =>
            {
                gridTypeButtons.FadeTo(v.NewValue ? 1f : 0f, 500, Easing.OutQuint);
                gridTypeButtons.BypassAutoSizeAxes = !v.NewValue ? Axes.Y : Axes.None;
            }, true);
        }

        private float normalizeRotation(float rotation, float period)
        {
            return ((rotation + 360 + period * 0.5f) % period) - period * 0.5f;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            switch (e.Action)
            {
                case GlobalAction.EditorCycleGridSpacing:
                    Spacing.Value = Spacing.Value * 2 >= max_automatic_spacing ? Spacing.Value / 8 : Spacing.Value * 2;
                    return true;

                case GlobalAction.EditorCycleGridType:
                    GridType.Value = (PositionSnapGridType)(((int)GridType.Value + 1) % Enum.GetValues<PositionSnapGridType>().Length);
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public partial class OutlineTriangle : BufferedContainer
        {
            public OutlineTriangle(bool outlineOnly, float size)
                : base(cachedFrameBuffer: true)
            {
                Size = new Vector2(size);

                InternalChildren = new Drawable[]
                {
                    new EquilateralTriangle { RelativeSizeAxes = Axes.Both },
                };

                if (outlineOnly)
                {
                    AddInternal(new EquilateralTriangle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.Centre,
                        RelativePositionAxes = Axes.Y,
                        Y = 0.48f,
                        Colour = Color4.Black,
                        Size = new Vector2(size - 7),
                        Blending = BlendingParameters.None,
                    });
                }

                Blending = BlendingParameters.Additive;
            }
        }
    }

    public enum PositionSnapGridType
    {
        Square,
        Triangle,
        Circle,
    }
}
