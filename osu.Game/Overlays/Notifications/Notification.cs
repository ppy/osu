// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Notifications
{
    public abstract partial class Notification : Container
    {
        /// <summary>
        /// Notification was closed, either by user or otherwise.
        /// Importantly, this event may be fired from a non-update thread.
        /// </summary>
        public event Action? Closed;

        public abstract LocalisableString Text { get; set; }

        /// <summary>
        /// Whether this notification should forcefully display itself.
        /// </summary>
        public virtual bool IsImportant => true;

        /// <summary>
        /// Run on user activating the notification. Return true to close.
        /// </summary>
        public Func<bool>? Activated;

        public Action? ForwardToOverlay { get; set; }

        /// <summary>
        /// Should we show at the top of our section on display?
        /// </summary>
        public virtual bool DisplayOnTop => true;

        public virtual string PopInSampleName => "UI/notification-default";
        public virtual string PopOutSampleName => "UI/overlay-pop-out";

        protected const float CORNER_RADIUS = 6;

        protected NotificationLight Light;

        protected Container IconContent;

        public bool WasClosed { get; private set; }

        private readonly FillFlowContainer content;

        protected override Container<Drawable> Content => content;

        protected Container MainContent;

        private readonly DragContainer dragContainer;

        public virtual bool Read { get; set; }

        protected virtual bool AllowFlingDismiss => true;

        public new bool IsDragged => dragContainer.IsDragged;

        protected virtual IconUsage CloseButtonIcon => FontAwesome.Solid.Check;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public override bool PropagatePositionalInputSubTree => base.PropagatePositionalInputSubTree && !WasClosed;

        private bool isInToastTray;

        /// <summary>
        /// Whether this notification is in the <see cref="NotificationOverlayToastTray"/>.
        /// </summary>
        public bool IsInToastTray
        {
            get => isInToastTray;
            set
            {
                isInToastTray = value;

                if (!isInToastTray)
                {
                    dragContainer.ResetPosition();
                    if (!Read)
                        Light.FadeIn(100);
                }
            }
        }

        private readonly Box initialFlash;

        private Box background = null!;

        protected Notification()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                Light = new NotificationLight
                {
                    Alpha = 0,
                    Margin = new MarginPadding { Right = 5 },
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreRight,
                },
                dragContainer = new DragContainer(this)
                {
                    // Use margin instead of FillFlow spacing to fix extra padding appearing when notification shrinks
                    // in height.
                    Padding = new MarginPadding { Vertical = 3f },
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                }.WithChild(MainContent = new Container
                {
                    CornerRadius = CORNER_RADIUS,
                    Masking = true,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    AutoSizeDuration = 400,
                    AutoSizeEasing = Easing.OutQuint,
                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize, minSize: 60)
                            },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    IconContent = new Container
                                    {
                                        Width = 40,
                                        RelativeSizeAxes = Axes.Y,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            content = new FillFlowContainer
                                            {
                                                Masking = true,
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Direction = FillDirection.Vertical,
                                                Spacing = new Vector2(15)
                                            },
                                        }
                                    },
                                    new CloseButton(CloseButtonIcon)
                                    {
                                        Action = () => Close(true),
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                    }
                                }
                            },
                        },
                        initialFlash = new Box
                        {
                            Colour = Color4.White.Opacity(0.8f),
                            RelativeSizeAxes = Axes.Both,
                            Blending = BlendingParameters.Additive,
                        },
                    }
                })
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            MainContent.Add(background = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = colourProvider.Background3,
                Depth = float.MaxValue
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(colourProvider.Background2, 200, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            background.FadeColour(colourProvider.Background3, 200, Easing.OutQuint);
            base.OnHoverLost(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            // right click doesn't trigger OnClick so we need to handle here until that changes.
            if (e.Button != MouseButton.Left)
            {
                Close(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            // Clicking with anything but left button should dismiss but not perform the activation action.
            if (e.Button == MouseButton.Left && Activated?.Invoke() == false)
                return true;

            Close(false);
            return true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeInFromZero(200);

            MainContent.MoveToX(DrawSize.X);
            MainContent.MoveToX(0, 500, Easing.OutQuint);

            initialFlash.FadeOutFromOne(2000, Easing.OutQuart);
        }

        public virtual void Close(bool runFlingAnimation)
        {
            if (WasClosed) return;

            WasClosed = true;

            Closed?.Invoke();

            Schedule(() =>
            {
                if (runFlingAnimation && dragContainer.FlingLeft())
                    this.FadeOut(600, Easing.In);
                else
                    this.FadeOut(100);

                Expire();
            });
        }

        private partial class DragContainer : Container
        {
            private Vector2 velocity;
            private Vector2 lastPosition;

            private readonly Notification notification;

            public DragContainer(Notification notification)
            {
                this.notification = notification;
            }

            public override RectangleF BoundingBox
            {
                get
                {
                    var childBounding = Children.First().BoundingBox;

                    if (X < 0) childBounding *= new Vector2(1, Math.Max(0, 1 + (X / 300)));
                    if (Y > 0) childBounding *= new Vector2(1, Math.Max(0, 1 - (Y / 200)));

                    return childBounding;
                }
            }

            protected override bool OnDragStart(DragStartEvent e) => notification.IsInToastTray;

            protected override void OnDrag(DragEvent e)
            {
                if (!notification.IsInToastTray)
                    return;

                Vector2 change = e.MousePosition - e.MouseDownPosition;

                // Diminish the drag distance as we go further to simulate "rubber band" feeling.
                change *= change.Length <= 0 ? 0 : MathF.Pow(change.Length, 0.8f) / change.Length;

                // Only apply Y change if dragging to the left.
                if (change.X >= 0)
                    change.Y = 0;
                else
                    change.Y *= (float)Interpolation.ApplyEasing(Easing.InOutQuart, Math.Min(1, -change.X / 200));

                this.MoveTo(change);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                if (notification.AllowFlingDismiss && (Rotation < -10 || velocity.X < -0.3f))
                    notification.Close(true);
                else if (X > 30 || velocity.X > 0.3f)
                    notification.ForwardToOverlay?.Invoke();
                else
                    ResetPosition();

                base.OnDragEnd(e);
            }

            private bool flinging;

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                Rotation = Math.Min(0, X * 0.1f);

                if (flinging)
                {
                    velocity.Y += (float)Clock.ElapsedFrameTime * 0.005f;
                    Position += (float)Clock.ElapsedFrameTime * velocity;
                }
                else if (Clock.ElapsedFrameTime > 0)
                {
                    Vector2 change = (Position - lastPosition) / (float)Clock.ElapsedFrameTime;

                    if (velocity.X == 0)
                        velocity = change;
                    else
                    {
                        velocity = new Vector2(
                            (float)Interpolation.DampContinuously(velocity.X, change.X, 40, Clock.ElapsedFrameTime),
                            (float)Interpolation.DampContinuously(velocity.Y, change.Y, 40, Clock.ElapsedFrameTime)
                        );
                    }

                    lastPosition = Position;
                }
            }

            public bool FlingLeft()
            {
                if (!notification.IsInToastTray)
                    return false;

                if (flinging)
                    return true;

                if (velocity.X > -0.3f)
                    velocity.X = -0.3f - 0.5f * RNG.NextSingle();

                flinging = true;
                ClearTransforms();
                return true;
            }

            public void ResetPosition()
            {
                this.MoveTo(Vector2.Zero, 800, Easing.OutElastic);
                this.RotateTo(0, 800, Easing.OutElastic);
            }
        }

        internal partial class CloseButton : OsuClickableContainer
        {
            private SpriteIcon icon = null!;
            private Box background = null!;

            private readonly IconUsage iconUsage;

            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public CloseButton(IconUsage iconUsage)
            {
                this.iconUsage = iconUsage;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Y;
                Width = 28;

                Children = new Drawable[]
                {
                    background = new Box
                    {
                        Colour = OsuColour.Gray(0).Opacity(0.15f),
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    icon = new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = iconUsage,
                        Size = new Vector2(12),
                        Colour = colourProvider.Foreground1,
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeIn(200, Easing.OutQuint);
                icon.FadeColour(colourProvider.Content1, 200, Easing.OutQuint);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeOut(200, Easing.OutQuint);
                icon.FadeColour(colourProvider.Foreground1, 200, Easing.OutQuint);
                base.OnHoverLost(e);
            }
        }

        public partial class NotificationLight : Container
        {
            private bool pulsate;
            private Container pulsateLayer = null!;

            public bool Pulsate
            {
                get => pulsate;
                set
                {
                    if (pulsate == value) return;

                    pulsate = value;

                    pulsateLayer.ClearTransforms();
                    pulsateLayer.Alpha = 1;

                    if (pulsate)
                    {
                        const float length = 1000;
                        pulsateLayer.Loop(length / 2,
                            p => p.FadeTo(0.4f, length, Easing.In).Then().FadeTo(1, length, Easing.Out)
                        );
                    }
                }
            }

            public new SRGBColour Colour
            {
                set
                {
                    base.Colour = value;
                    pulsateLayer.EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = ((Color4)value).Opacity(0.18f),
                        Type = EdgeEffectType.Glow,
                        Radius = 14,
                    };
                }
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Size = new Vector2(6, 15);

                Children = new[]
                {
                    pulsateLayer = new CircularContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                            },
                        }
                    }
                };
            }
        }
    }
}
