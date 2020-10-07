// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBox : CompositeDrawable
    {
        public Action<float> OnRotation;
        public Action<Vector2, Anchor> OnScale;
        public Action<Direction> OnFlip;

        public Action OperationStarted;
        public Action OperationEnded;

        private bool canRotate;

        /// <summary>
        /// Whether rotation support should be enabled.
        /// </summary>
        public bool CanRotate
        {
            get => canRotate;
            set
            {
                if (canRotate == value) return;

                canRotate = value;
                recreate();
            }
        }

        private bool canScaleX;

        /// <summary>
        /// Whether vertical scale support should be enabled.
        /// </summary>
        public bool CanScaleX
        {
            get => canScaleX;
            set
            {
                if (canScaleX == value) return;

                canScaleX = value;
                recreate();
            }
        }

        private bool canScaleY;

        /// <summary>
        /// Whether horizontal scale support should be enabled.
        /// </summary>
        public bool CanScaleY
        {
            get => canScaleY;
            set
            {
                if (canScaleY == value) return;

                canScaleY = value;
                recreate();
            }
        }

        private FillFlowContainer buttons;

        public const float BORDER_RADIUS = 3;

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            recreate();
        }

        private void recreate()
        {
            if (LoadState < LoadState.Loading)
                return;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    Masking = true,
                    BorderThickness = BORDER_RADIUS,
                    BorderColour = colours.YellowDark,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,

                            AlwaysPresent = true,
                            Alpha = 0
                        },
                    }
                },
                buttons = new FillFlowContainer
                {
                    Y = 20,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre
                }
            };

            if (CanScaleX) addXScaleComponents();
            if (CanScaleX && CanScaleY) addFullScaleComponents();
            if (CanScaleY) addYScaleComponents();
            if (CanRotate) addRotationComponents();
        }

        private void addRotationComponents()
        {
            const float separation = 40;

            addButton(FontAwesome.Solid.Undo, "Rotate 90 degrees counter-clockwise", () => OnRotation?.Invoke(-90));
            addButton(FontAwesome.Solid.Redo, "Rotate 90 degrees clockwise", () => OnRotation?.Invoke(90));

            AddRangeInternal(new Drawable[]
            {
                new Box
                {
                    Depth = float.MaxValue,
                    Colour = colours.YellowLight,
                    Blending = BlendingParameters.Additive,
                    Alpha = 0.3f,
                    Size = new Vector2(BORDER_RADIUS, separation),
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                },
                new SelectionBoxDragHandleButton(FontAwesome.Solid.Redo, "Free rotate")
                {
                    Anchor = Anchor.TopCentre,
                    Y = -separation,
                    HandleDrag = e => OnRotation?.Invoke(e.Delta.X),
                    OperationStarted = operationStarted,
                    OperationEnded = operationEnded
                }
            });
        }

        private void addYScaleComponents()
        {
            addButton(FontAwesome.Solid.ArrowsAltV, "Flip vertically", () => OnFlip?.Invoke(Direction.Vertical));

            addDragHandle(Anchor.TopCentre);
            addDragHandle(Anchor.BottomCentre);
        }

        private void addFullScaleComponents()
        {
            addDragHandle(Anchor.TopLeft);
            addDragHandle(Anchor.TopRight);
            addDragHandle(Anchor.BottomLeft);
            addDragHandle(Anchor.BottomRight);
        }

        private void addXScaleComponents()
        {
            addButton(FontAwesome.Solid.ArrowsAltH, "Flip horizontally", () => OnFlip?.Invoke(Direction.Horizontal));

            addDragHandle(Anchor.CentreLeft);
            addDragHandle(Anchor.CentreRight);
        }

        private void addButton(IconUsage icon, string tooltip, Action action)
        {
            buttons.Add(new SelectionBoxDragHandleButton(icon, tooltip)
            {
                OperationStarted = operationStarted,
                OperationEnded = operationEnded,
                Action = action
            });
        }

        private void addDragHandle(Anchor anchor) => AddInternal(new SelectionBoxDragHandle
        {
            Anchor = anchor,
            HandleDrag = e => OnScale?.Invoke(e.Delta, anchor),
            OperationStarted = operationStarted,
            OperationEnded = operationEnded
        });

        private int activeOperations;

        private void operationEnded()
        {
            if (--activeOperations == 0)
                OperationEnded?.Invoke();
        }

        private void operationStarted()
        {
            if (activeOperations++ == 0)
                OperationStarted?.Invoke();
        }
    }
}
