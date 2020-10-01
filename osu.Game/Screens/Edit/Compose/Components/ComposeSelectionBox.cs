// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
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

            buttons.Insert(-1, new DragHandleButton(FontAwesome.Solid.Undo, "Rotate 90 degrees counter-clockwise")
            {
                OperationStarted = operationStarted,
                OperationEnded = operationEnded,
                Action = () => OnRotation?.Invoke(-90)
            });

            buttons.Add(new DragHandleButton(FontAwesome.Solid.Redo, "Rotate 90 degrees clockwise")
            {
                OperationStarted = operationStarted,
                OperationEnded = operationEnded,
                Action = () => OnRotation?.Invoke(90)
            });

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
                new DragHandleButton(FontAwesome.Solid.Redo, "Free rotate")
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
            buttons.Add(new DragHandleButton(FontAwesome.Solid.ArrowsAltV, "Flip vertically")
            {
                OperationStarted = operationStarted,
                OperationEnded = operationEnded,
                Action = () => OnFlip?.Invoke(Direction.Vertical)
            });

            AddRangeInternal(new[]
            {
                createDragHandle(Anchor.TopCentre),
                createDragHandle(Anchor.BottomCentre),
            });
        }

        private void addFullScaleComponents()
        {
            AddRangeInternal(new[]
            {
                createDragHandle(Anchor.TopLeft),
                createDragHandle(Anchor.TopRight),
                createDragHandle(Anchor.BottomLeft),
                createDragHandle(Anchor.BottomRight),
            });
        }

        private void addXScaleComponents()
        {
            buttons.Add(new DragHandleButton(FontAwesome.Solid.ArrowsAltH, "Flip horizontally")
            {
                OperationStarted = operationStarted,
                OperationEnded = operationEnded,
                Action = () => OnFlip?.Invoke(Direction.Horizontal)
            });

            AddRangeInternal(new[]
            {
                createDragHandle(Anchor.CentreLeft),
                createDragHandle(Anchor.CentreRight),
            });
        }

        private DragHandle createDragHandle(Anchor anchor) =>
            new DragHandle
            {
                Anchor = anchor,
                HandleDrag = e => OnScale?.Invoke(e.Delta, anchor),
                OperationStarted = operationStarted,
                OperationEnded = operationEnded
            };

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

        private sealed class DragHandleButton : DragHandle, IHasTooltip
        {
            private SpriteIcon icon;

            private readonly IconUsage iconUsage;

            public Action Action;

            public DragHandleButton(IconUsage iconUsage, string tooltip)
            {
                this.iconUsage = iconUsage;

                TooltipText = tooltip;

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size *= 2;
                AddInternal(icon = new SpriteIcon
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.5f),
                    Icon = iconUsage,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                });
            }

            protected override bool OnClick(ClickEvent e)
            {
                OperationStarted?.Invoke();
                Action?.Invoke();
                OperationEnded?.Invoke();
                return true;
            }

            protected override void UpdateHoverState()
            {
                base.UpdateHoverState();
                icon.Colour = !HandlingMouse && IsHovered ? Color4.White : Color4.Black;
            }

            public string TooltipText { get; }
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
