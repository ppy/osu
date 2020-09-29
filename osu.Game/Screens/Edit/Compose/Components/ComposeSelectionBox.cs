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
        public Action<DragEvent> OnRotation;
        public Action<DragEvent, Anchor> OnScaleX;
        public Action<DragEvent, Anchor> OnScaleY;

        private bool canRotate;

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
                        HandleDrag = e => OnRotation?.Invoke(e)
                    }
                });
            }

            if (CanScaleY)
            {
                AddRangeInternal(new[]
                {
                    new DragHandle
                    {
                        Anchor = Anchor.TopCentre,
                        HandleDrag = e => OnScaleY?.Invoke(e, Anchor.TopCentre)
                    },
                    new DragHandle
                    {
                        Anchor = Anchor.BottomCentre,
                        HandleDrag = e => OnScaleY?.Invoke(e, Anchor.BottomCentre)
                    },
                });
            }

            if (CanScaleX)
            {
                AddRangeInternal(new[]
                {
                    new DragHandle
                    {
                        Anchor = Anchor.CentreLeft,
                        HandleDrag = e => OnScaleX?.Invoke(e, Anchor.CentreLeft)
                    },
                    new DragHandle
                    {
                        Anchor = Anchor.CentreRight,
                        HandleDrag = e => OnScaleX?.Invoke(e, Anchor.CentreRight)
                    },
                });
            }

            if (CanScaleX && CanScaleY)
            {
                AddRangeInternal(new[]
                {
                    new DragHandle
                    {
                        Anchor = Anchor.TopLeft,
                        HandleDrag = e =>
                        {
                            OnScaleX?.Invoke(e, Anchor.TopLeft);
                            OnScaleY?.Invoke(e, Anchor.TopLeft);
                        }
                    },
                    new DragHandle
                    {
                        Anchor = Anchor.TopRight,
                        HandleDrag = e =>
                        {
                            OnScaleX?.Invoke(e, Anchor.TopRight);
                            OnScaleY?.Invoke(e, Anchor.TopRight);
                        }
                    },
                    new DragHandle
                    {
                        Anchor = Anchor.BottomLeft,
                        HandleDrag = e =>
                        {
                            OnScaleX?.Invoke(e, Anchor.BottomLeft);
                            OnScaleY?.Invoke(e, Anchor.BottomLeft);
                        }
                    },
                    new DragHandle
                    {
                        Anchor = Anchor.BottomRight,
                        HandleDrag = e =>
                        {
                            OnScaleX?.Invoke(e, Anchor.BottomRight);
                            OnScaleY?.Invoke(e, Anchor.BottomRight);
                        }
                    },
                });
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

            protected override bool OnDragStart(DragStartEvent e) => true;

            protected override void OnDrag(DragEvent e)
            {
                HandleDrag?.Invoke(e);
                base.OnDrag(e);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                HandlingMouse = false;
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
