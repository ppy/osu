// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class SelectionBox : CompositeDrawable
    {
        public Func<float, bool> OnRotation;
        public Func<Vector2, Anchor, bool> OnScale;
        public Func<Direction, bool> OnFlip;
        public Func<bool> OnReverse;

        public Action OperationStarted;
        public Action OperationEnded;

        private bool canReverse;

        /// <summary>
        /// Whether pattern reversing support should be enabled.
        /// </summary>
        public bool CanReverse
        {
            get => canReverse;
            set
            {
                if (canReverse == value) return;

                canReverse = value;
                recreate();
            }
        }

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

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || !e.ControlPressed)
                return false;

            switch (e.Key)
            {
                case Key.G:
                    return CanReverse && OnReverse?.Invoke() == true;

                case Key.H:
                    return CanScaleX && OnFlip?.Invoke(Direction.Horizontal) == true;

                case Key.J:
                    return CanScaleY && OnFlip?.Invoke(Direction.Vertical) == true;
            }

            return base.OnKeyDown(e);
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
            if (CanReverse) addButton(FontAwesome.Solid.Backward, "Reverse pattern (Ctrl-G)", () => OnReverse?.Invoke());
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
                    HandleDrag = e => OnRotation?.Invoke(convertDragEventToAngleOfRotation(e)),
                    OperationStarted = operationStarted,
                    OperationEnded = operationEnded
                }
            });
        }

        private void addYScaleComponents()
        {
            addButton(FontAwesome.Solid.ArrowsAltV, "Flip vertically (Ctrl-J)", () => OnFlip?.Invoke(Direction.Vertical));

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
            addButton(FontAwesome.Solid.ArrowsAltH, "Flip horizontally (Ctrl-H)", () => OnFlip?.Invoke(Direction.Horizontal));

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
            Y = getAdjustmentToCenterCircleOnBorder(anchor).Y,
            X = getAdjustmentToCenterCircleOnBorder(anchor).X,
            HandleDrag = e => OnScale?.Invoke(e.Delta, anchor),
            OperationStarted = operationStarted,
            OperationEnded = operationEnded
        });

        private int activeOperations;

        private float convertDragEventToAngleOfRotation(DragEvent e)
        {
            // Adjust coordinate system to the center of SelectionBox
            float startAngle = MathF.Atan2(e.LastMousePosition.Y - DrawHeight / 2, e.LastMousePosition.X - DrawWidth / 2);
            float endAngle = MathF.Atan2(e.MousePosition.Y - DrawHeight / 2, e.MousePosition.X - DrawWidth / 2);

            return (endAngle - startAngle) * 180 / MathF.PI;
        }

        /// <summary>
        /// Adjust Drag circle to be centered on the center of the border instead of on the edge.
        /// </summary>
        /// <param name="anchor">The part of the rectangle to be adjusted.</param>
        /// <returns>A 2d vector on how much to adjust the drag circle</returns>
        private Vector2 getAdjustmentToCenterCircleOnBorder(Anchor anchor)
        {
            Vector2 adjustment = Vector2.Zero;

            switch (anchor)
            {
                case Anchor.TopLeft:
                case Anchor.CentreLeft:
                case Anchor.BottomLeft:
                    adjustment.X = BORDER_RADIUS / 2;
                    break;

                case Anchor.TopRight:
                case Anchor.CentreRight:
                case Anchor.BottomRight:
                    adjustment.X = -BORDER_RADIUS / 2;
                    break;
            }

            switch (anchor)
            {
                case Anchor.TopLeft:
                case Anchor.TopCentre:
                case Anchor.TopRight:
                    adjustment.Y = BORDER_RADIUS / 2;
                    break;

                case Anchor.BottomLeft:
                case Anchor.BottomCentre:
                case Anchor.BottomRight:
                    adjustment.Y = -BORDER_RADIUS / 2;
                    break;
            }

            return adjustment;
        }

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
