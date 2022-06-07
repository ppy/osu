// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable
using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Overlays;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Utility
{
    public class LatencyArea : CompositeDrawable
    {
        [Resolved]
        private OverlayColourProvider overlayColourProvider { get; set; } = null!;

        public Action? ReportUserBest { get; set; }

        private Drawable? background;

        private readonly Key key;

        public readonly int TargetFrameRate;

        public readonly BindableBool IsActiveArea = new BindableBool();

        public LatencyArea(Key key, int targetFrameRate)
        {
            this.key = key;
            TargetFrameRate = targetFrameRate;

            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            InternalChildren = new[]
            {
                background = new Box
                {
                    Colour = overlayColourProvider.Background6,
                    RelativeSizeAxes = Axes.Both,
                },
                new ButtonWithKeyBind(key)
                {
                    Text = "Feels better",
                    Y = 20,
                    Width = 0.8f,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Action = () => ReportUserBest?.Invoke(),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new LatencyMovableBox(IsActiveArea)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        new LatencyCursorContainer(IsActiveArea)
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
            };

            IsActiveArea.BindValueChanged(active =>
            {
                background.FadeColour(active.NewValue ? overlayColourProvider.Background4 : overlayColourProvider.Background6, 200, Easing.OutQuint);
            }, true);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            IsActiveArea.Value = true;
            return base.OnMouseMove(e);
        }

        private double lastFrameTime;

        public override bool UpdateSubTree()
        {
            double elapsed = Clock.CurrentTime - lastFrameTime;
            if (TargetFrameRate > 0 && elapsed < 1000.0 / TargetFrameRate)
                return false;

            lastFrameTime = Clock.CurrentTime;

            return base.UpdateSubTree();
        }

        public class LatencyMovableBox : CompositeDrawable
        {
            private Box box = null!;
            private InputManager inputManager = null!;

            private readonly BindableBool isActive;

            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            public LatencyMovableBox(BindableBool isActive)
            {
                this.isActive = isActive;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                inputManager = GetContainingInputManager();

                InternalChild = box = new Box
                {
                    Size = new Vector2(40),
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(0.5f),
                    Origin = Anchor.Centre,
                    Colour = overlayColourProvider.Colour1,
                };
            }

            protected override bool OnHover(HoverEvent e) => false;

            private double? lastFrameTime;

            protected override void Update()
            {
                base.Update();

                if (!isActive.Value)
                {
                    lastFrameTime = null;
                    return;
                }

                if (lastFrameTime != null)
                {
                    float movementAmount = (float)(Clock.CurrentTime - lastFrameTime) / 400;

                    var buttons = inputManager.CurrentState.Keyboard.Keys;

                    box.Colour = buttons.HasAnyButtonPressed ? overlayColourProvider.Content1 : overlayColourProvider.Colour1;

                    foreach (var key in buttons)
                    {
                        switch (key)
                        {
                            case Key.K:
                            case Key.Up:
                                box.Y = MathHelper.Clamp(box.Y - movementAmount, 0.1f, 0.9f);
                                break;

                            case Key.J:
                            case Key.Down:
                                box.Y = MathHelper.Clamp(box.Y + movementAmount, 0.1f, 0.9f);
                                break;

                            case Key.Z:
                            case Key.Left:
                                box.X = MathHelper.Clamp(box.X - movementAmount, 0.1f, 0.9f);
                                break;

                            case Key.X:
                            case Key.Right:
                                box.X = MathHelper.Clamp(box.X + movementAmount, 0.1f, 0.9f);
                                break;
                        }
                    }
                }

                lastFrameTime = Clock.CurrentTime;
            }
        }

        public class LatencyCursorContainer : CompositeDrawable
        {
            private Circle cursor = null!;
            private InputManager inputManager = null!;

            private readonly BindableBool isActive;

            [Resolved]
            private OverlayColourProvider overlayColourProvider { get; set; } = null!;

            public LatencyCursorContainer(BindableBool isActive)
            {
                this.isActive = isActive;
                Masking = true;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                InternalChild = cursor = new Circle
                {
                    Size = new Vector2(40),
                    Origin = Anchor.Centre,
                    Colour = overlayColourProvider.Colour2,
                };

                inputManager = GetContainingInputManager();
            }

            protected override bool OnHover(HoverEvent e) => false;

            protected override void Update()
            {
                cursor.Colour = inputManager.CurrentState.Mouse.IsPressed(MouseButton.Left) ? overlayColourProvider.Content1 : overlayColourProvider.Colour2;

                if (isActive.Value)
                {
                    cursor.Position = ToLocalSpace(inputManager.CurrentState.Mouse.Position);
                    cursor.Alpha = 1;
                }
                else
                {
                    cursor.Alpha = 0;
                }

                base.Update();
            }
        }
    }
}
