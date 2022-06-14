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
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    [Cached]
    public class SelectionBox : CompositeDrawable
    {
        public const float BORDER_RADIUS = 3;

        public Func<float, bool> OnRotation;
        public Func<Vector2, Anchor, bool> OnScale;
        public Func<Direction, bool, bool> OnFlip;
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
        /// Whether horizontal scaling support should be enabled.
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
        /// Whether vertical scaling support should be enabled.
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

        private bool canFlipX;

        /// <summary>
        /// Whether horizontal flipping support should be enabled.
        /// </summary>
        public bool CanFlipX
        {
            get => canFlipX;
            set
            {
                if (canFlipX == value) return;

                canFlipX = value;
                recreate();
            }
        }

        private bool canFlipY;

        /// <summary>
        /// Whether vertical flipping support should be enabled.
        /// </summary>
        public bool CanFlipY
        {
            get => canFlipY;
            set
            {
                if (canFlipY == value) return;

                canFlipY = value;
                recreate();
            }
        }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                if (value == text)
                    return;

                text = value;
                if (selectionDetailsText != null)
                    selectionDetailsText.Text = value;
            }
        }

        private SelectionBoxDragHandleContainer dragHandles;
        private FillFlowContainer buttons;

        private OsuSpriteText selectionDetailsText;

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load() => recreate();

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || !e.ControlPressed)
                return false;

            bool runOperationFromHotkey(Func<bool> operation)
            {
                operationStarted();
                bool result = operation?.Invoke() ?? false;
                operationEnded();

                return result;
            }

            switch (e.Key)
            {
                case Key.G:
                    return CanReverse && runOperationFromHotkey(OnReverse);
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
                    Name = "info text",
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.YellowDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        selectionDetailsText = new OsuSpriteText
                        {
                            Padding = new MarginPadding(2),
                            Colour = colours.Gray0,
                            Font = OsuFont.Default.With(size: 11),
                            Text = text,
                        }
                    }
                },
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
                dragHandles = new SelectionBoxDragHandleContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    // ensures that the centres of all drag handles line up with the middle of the selection box border.
                    Padding = new MarginPadding(BORDER_RADIUS / 2)
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
            if (CanFlipX) addXFlipComponents();
            if (CanFlipY) addYFlipComponents();
            if (CanRotate) addRotationComponents();
            if (CanReverse) addButton(FontAwesome.Solid.Backward, "Reverse pattern (Ctrl-G)", () => OnReverse?.Invoke());
        }

        private void addRotationComponents()
        {
            addButton(FontAwesome.Solid.Undo, "Rotate 90 degrees counter-clockwise", () => OnRotation?.Invoke(-90));
            addButton(FontAwesome.Solid.Redo, "Rotate 90 degrees clockwise", () => OnRotation?.Invoke(90));

            addRotateHandle(Anchor.TopLeft);
            addRotateHandle(Anchor.TopRight);
            addRotateHandle(Anchor.BottomLeft);
            addRotateHandle(Anchor.BottomRight);
        }

        private void addYScaleComponents()
        {
            addScaleHandle(Anchor.TopCentre);
            addScaleHandle(Anchor.BottomCentre);
        }

        private void addFullScaleComponents()
        {
            addScaleHandle(Anchor.TopLeft);
            addScaleHandle(Anchor.TopRight);
            addScaleHandle(Anchor.BottomLeft);
            addScaleHandle(Anchor.BottomRight);
        }

        private void addXScaleComponents()
        {
            addScaleHandle(Anchor.CentreLeft);
            addScaleHandle(Anchor.CentreRight);
        }

        private void addXFlipComponents()
        {
            addButton(FontAwesome.Solid.ArrowsAltH, "Flip horizontally", () => OnFlip?.Invoke(Direction.Horizontal, false));
        }

        private void addYFlipComponents()
        {
            addButton(FontAwesome.Solid.ArrowsAltV, "Flip vertically", () => OnFlip?.Invoke(Direction.Vertical, false));
        }

        private void addButton(IconUsage icon, string tooltip, Action action)
        {
            var button = new SelectionBoxButton(icon, tooltip)
            {
                Action = action
            };

            button.OperationStarted += operationStarted;
            button.OperationEnded += operationEnded;
            buttons.Add(button);
        }

        private void addScaleHandle(Anchor anchor)
        {
            var handle = new SelectionBoxScaleHandle
            {
                Anchor = anchor,
                HandleScale = (delta, a) => OnScale?.Invoke(delta, a)
            };

            handle.OperationStarted += operationStarted;
            handle.OperationEnded += operationEnded;
            dragHandles.AddScaleHandle(handle);
        }

        private void addRotateHandle(Anchor anchor)
        {
            var handle = new SelectionBoxRotationHandle
            {
                Anchor = anchor,
                HandleRotate = angle => OnRotation?.Invoke(angle)
            };

            handle.OperationStarted += operationStarted;
            handle.OperationEnded += operationEnded;
            dragHandles.AddRotationHandle(handle);
        }

        private int activeOperations;

        private float convertDragEventToAngleOfRotation(DragEvent e)
        {
            // Adjust coordinate system to the center of SelectionBox
            float startAngle = MathF.Atan2(e.LastMousePosition.Y - DrawHeight / 2, e.LastMousePosition.X - DrawWidth / 2);
            float endAngle = MathF.Atan2(e.MousePosition.Y - DrawHeight / 2, e.MousePosition.X - DrawWidth / 2);

            return (endAngle - startAngle) * 180 / MathF.PI;
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
