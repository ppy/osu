// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.EnumExtensions;
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
    public partial class SelectionBox : CompositeDrawable
    {
        public const float BORDER_RADIUS = 3;

        private const float button_padding = 5;

        [Resolved]
        private SelectionRotationHandler? rotationHandler { get; set; }

        public Func<Vector2, Anchor, bool>? OnScale;
        public Func<Direction, bool, bool>? OnFlip;
        public Func<bool>? OnReverse;

        public Action? OperationStarted;
        public Action? OperationEnded;

        private SelectionBoxButton? reverseButton;
        private SelectionBoxButton? rotateClockwiseButton;
        private SelectionBoxButton? rotateCounterClockwiseButton;

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

        private readonly IBindable<bool> canRotate = new BindableBool();

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

        private bool canScaleProportionally;

        /// <summary>
        /// Whether vertical scaling support should be enabled.
        /// </summary>
        public bool CanScaleProportionally
        {
            get => canScaleProportionally;
            set
            {
                if (canScaleProportionally == value) return;

                canScaleProportionally = value;
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

        private string text = string.Empty;

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

        private SelectionBoxDragHandleContainer dragHandles = null!;
        private FillFlowContainer buttons = null!;

        private OsuSpriteText? selectionDetailsText;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (rotationHandler != null)
                canRotate.BindTo(rotationHandler.CanRotate);

            canRotate.BindValueChanged(_ => recreate(), true);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || !e.ControlPressed)
                return false;

            switch (e.Key)
            {
                case Key.G:
                    return CanReverse && reverseButton?.TriggerClick() == true;

                case Key.Comma:
                    return canRotate.Value && rotateCounterClockwiseButton?.TriggerClick() == true;

                case Key.Period:
                    return canRotate.Value && rotateClockwiseButton?.TriggerClick() == true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Update()
        {
            base.Update();

            ensureButtonsOnScreen();
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
                    AutoSizeAxes = Axes.X,
                    Height = 30,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding(button_padding),
                }
            };

            if (CanScaleX) addXScaleComponents();
            if (CanScaleProportionally) addFullScaleComponents();
            if (CanScaleY) addYScaleComponents();
            if (CanFlipX) addXFlipComponents();
            if (CanFlipY) addYFlipComponents();
            if (canRotate.Value) addRotationComponents();
            if (CanReverse) reverseButton = addButton(FontAwesome.Solid.Backward, "Reverse pattern (Ctrl-G)", () => OnReverse?.Invoke());
        }

        private void addRotationComponents()
        {
            rotateCounterClockwiseButton = addButton(FontAwesome.Solid.Undo, "Rotate 90 degrees counter-clockwise (Ctrl-<)", () => rotationHandler?.Rotate(-90));
            rotateClockwiseButton = addButton(FontAwesome.Solid.Redo, "Rotate 90 degrees clockwise (Ctrl->)", () => rotationHandler?.Rotate(90));

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

        private SelectionBoxButton addButton(IconUsage icon, string tooltip, Action action)
        {
            var button = new SelectionBoxButton(icon, tooltip)
            {
                Action = action
            };

            button.OperationStarted += operationStarted;
            button.OperationEnded += operationEnded;
            buttons.Add(button);

            return button;
        }

        /// <remarks>
        /// This method should be called when a selection needs to be flipped
        /// because of an ongoing scale handle drag that would otherwise cause width or height to go negative.
        /// </remarks>
        public void PerformFlipFromScaleHandles(Axes axes)
        {
            if (axes.HasFlagFast(Axes.X))
            {
                dragHandles.FlipScaleHandles(Direction.Horizontal);
                OnFlip?.Invoke(Direction.Horizontal, false);
            }

            if (axes.HasFlagFast(Axes.Y))
            {
                dragHandles.FlipScaleHandles(Direction.Vertical);
                OnFlip?.Invoke(Direction.Vertical, false);
            }
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

        private void ensureButtonsOnScreen()
        {
            buttons.Position = Vector2.Zero;

            var thisQuad = ScreenSpaceDrawQuad;

            // Shrink the parent quad to give a bit of padding so the buttons don't stick *right* on the border.
            // AABBFloat assumes no rotation. one would hope the whole editor is not being rotated.
            var parentQuad = Parent!.ScreenSpaceDrawQuad.AABBFloat.Shrink(ToLocalSpace(thisQuad.TopLeft + new Vector2(button_padding * 2)));

            float topExcess = thisQuad.TopLeft.Y - parentQuad.TopLeft.Y;
            float bottomExcess = parentQuad.BottomLeft.Y - thisQuad.BottomLeft.Y;
            float leftExcess = thisQuad.TopLeft.X - parentQuad.TopLeft.X;
            float rightExcess = parentQuad.TopRight.X - thisQuad.TopRight.X;

            float minHeight = buttons.ScreenSpaceDrawQuad.Height;

            if (topExcess < minHeight && bottomExcess < minHeight)
            {
                buttons.Anchor = Anchor.BottomCentre;
                buttons.Origin = Anchor.BottomCentre;
                buttons.Y = Math.Min(0, ToLocalSpace(Parent!.ScreenSpaceDrawQuad.BottomLeft).Y - DrawHeight);
            }
            else if (topExcess > bottomExcess)
            {
                buttons.Anchor = Anchor.TopCentre;
                buttons.Origin = Anchor.BottomCentre;
            }
            else
            {
                buttons.Anchor = Anchor.BottomCentre;
                buttons.Origin = Anchor.TopCentre;
            }

            buttons.X += ToLocalSpace(thisQuad.TopLeft - new Vector2(Math.Min(0, leftExcess)) + new Vector2(Math.Min(0, rightExcess))).X;
        }
    }
}
