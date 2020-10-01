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
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class ComposeSelectionBox : CompositeDrawable
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
                canScaleY = value;
                recreate();
            }
        }

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
            };

            if (CanRotate)
            {
                const float separation = 40;

                AddRangeInternal(new Drawable[]
                {
                    new Box
                    {
                        Colour = colours.YellowLight,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0.3f,
                        Size = new Vector2(BORDER_RADIUS, separation),
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.BottomCentre,
                    },
                    new RotationDragHandle
                    {
                        Anchor = Anchor.TopCentre,
                        Y = -separation,
                        HandleDrag = e => OnRotation?.Invoke(e),
                        OperationStarted = operationStarted,
                        OperationEnded = operationEnded
                    }
                });
            }

            if (CanScaleY)
            {
                AddRangeInternal(new[]
                {
                    createDragHandle(Anchor.TopCentre),
                    createDragHandle(Anchor.BottomCentre),
                });
            }

            if (CanScaleX)
            {
                AddRangeInternal(new[]
                {
                    createDragHandle(Anchor.CentreLeft),
                    createDragHandle(Anchor.CentreRight),
                });
            }

            if (CanScaleX && CanScaleY)
            {
                AddRangeInternal(new[]
                {
                    createDragHandle(Anchor.TopLeft),
                    createDragHandle(Anchor.TopRight),
                    createDragHandle(Anchor.BottomLeft),
                    createDragHandle(Anchor.BottomRight),
                });
            }

            ScaleDragHandle createDragHandle(Anchor anchor) =>
                new ScaleDragHandle(anchor)
                {
                    HandleDrag = e => OnScale?.Invoke(e, anchor),
                    OperationStarted = operationStarted,
                    OperationEnded = operationEnded
                };
        }

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

        private class ScaleDragHandle : DragHandle
        {
            public ScaleDragHandle(Anchor anchor)
            {
                Anchor = anchor;
            }
        }

        private class RotationDragHandle : DragHandle
        {
            private SpriteIcon icon;

            [BackgroundDependencyLoader]
            private void load()
            {
                Size *= 2;

                AddInternal(icon = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f),
                    Icon = FontAwesome.Solid.Redo,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            protected override void UpdateHoverState()
            {
                base.UpdateHoverState();
                icon.Colour = !HandlingMouse && IsHovered ? Color4.White : Color4.Black;
            }
        }

        private class DragHandle : Container
        {
            public Action OperationStarted;
            public Action OperationEnded;

            public Action<DragEvent> HandleDrag { get; set; }

            private Circle circle;

            [Resolved]
            private OsuColour colours { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(10);
                Origin = Anchor.Centre;

                InternalChildren = new Drawable[]
                {
                    circle = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    },
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                UpdateHoverState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                UpdateHoverState();
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                UpdateHoverState();
            }

            protected bool HandlingMouse;

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                HandlingMouse = true;
                UpdateHoverState();
                return true;
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                OperationStarted?.Invoke();
                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                HandleDrag?.Invoke(e);
                base.OnDrag(e);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                HandlingMouse = false;
                OperationEnded?.Invoke();

                UpdateHoverState();
                base.OnDragEnd(e);
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                HandlingMouse = false;
                UpdateHoverState();
                base.OnMouseUp(e);
            }

            protected virtual void UpdateHoverState()
            {
                circle.Colour = HandlingMouse ? colours.GrayF : (IsHovered ? colours.Red : colours.YellowDark);
                this.ScaleTo(HandlingMouse || IsHovered ? 1.5f : 1, 100, Easing.OutQuint);
            }
        }
    }
}
