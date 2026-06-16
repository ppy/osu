// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
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

        [Resolved]
        private SelectionScaleHandler? scaleHandler { get; set; }

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
                recreateButtons();
            }
        }

        private readonly IBindable<bool> canRotate = new BindableBool();

        private readonly IBindable<bool> canScaleX = new BindableBool();

        private readonly IBindable<bool> canScaleY = new BindableBool();

        private readonly IBindable<bool> canScaleDiagonally = new BindableBool();

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
                recreateButtons();
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
                recreateButtons();
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
        private FillFlowContainer<SelectionBoxButton> buttons = null!;

        private OsuSpriteText? selectionDetailsText;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
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
                buttons = new FillFlowContainer<SelectionBoxButton>
                {
                    AutoSizeAxes = Axes.X,
                    Height = 30,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding(button_padding),
                }
            };

            if (rotationHandler != null)
                canRotate.BindTo(rotationHandler.CanRotateAroundSelectionOrigin);

            if (scaleHandler != null)
            {
                canScaleX.BindTo(scaleHandler.CanScaleX);
                canScaleY.BindTo(scaleHandler.CanScaleY);
                canScaleDiagonally.BindTo(scaleHandler.CanScaleDiagonally);
            }

            canScaleX.BindValueChanged(_ => recreateScaleHandles());
            canScaleY.BindValueChanged(_ => recreateScaleHandles());
            canScaleDiagonally.BindValueChanged(_ => recreateScaleHandles(), true);
            canRotate.BindValueChanged(_ =>
            {
                recreateRotationHandles();
                recreateButtons();
            }, true);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || !e.ControlPressed)
                return false;

            switch (e.Key)
            {
                case Key.G:
                    if (!CanReverse || reverseButton == null)
                        return false;

                    reverseButton.TriggerAction();
                    return true;

                case Key.Comma:
                    if (!canRotate.Value || rotateCounterClockwiseButton == null)
                        return false;

                    rotateCounterClockwiseButton.TriggerAction();
                    return true;

                case Key.Period:
                    if (!canRotate.Value || rotateClockwiseButton == null)
                        return false;

                    rotateClockwiseButton.TriggerAction();
                    return true;
            }

            return base.OnKeyDown(e);
        }

        protected override void Update()
        {
            base.Update();

            ensureButtonsOnScreen();
        }

        private void recreateScaleHandles()
        {
            if (LoadState < LoadState.Loading)
                return;

            dragHandles.ClearScaleHandles();

            if (canScaleY.Value)
            {
                addScaleHandle(Anchor.TopCentre);
                addScaleHandle(Anchor.BottomCentre);
            }

            if (canScaleDiagonally.Value)
            {
                addScaleHandle(Anchor.TopLeft);
                addScaleHandle(Anchor.TopRight);
                addScaleHandle(Anchor.BottomLeft);
                addScaleHandle(Anchor.BottomRight);
            }

            if (canScaleX.Value)
            {
                addScaleHandle(Anchor.CentreLeft);
                addScaleHandle(Anchor.CentreRight);
            }
        }

        private void addScaleHandle(Anchor anchor)
        {
            var handle = new SelectionBoxScaleHandle
            {
                Anchor = anchor,
            };

            handle.OperationStarted += operationStarted;
            handle.OperationEnded += operationEnded;
            dragHandles.AddScaleHandle(handle);
        }

        private void recreateRotationHandles()
        {
            if (LoadState < LoadState.Loading)
                return;

            dragHandles.ClearRotationHandles();

            if (canRotate.Value)
            {
                addRotateHandle(Anchor.TopLeft);
                addRotateHandle(Anchor.TopRight);
                addRotateHandle(Anchor.BottomLeft);
                addRotateHandle(Anchor.BottomRight);
            }
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

        private void recreateButtons()
        {
            if (LoadState < LoadState.Loading)
                return;

            clearButtons();

            if (canRotate.Value)
            {
                rotateCounterClockwiseButton = addButton(FontAwesome.Solid.Undo, "Rotate 90 degrees counter-clockwise (Ctrl-<)", () => rotationHandler?.Rotate(-90));
                rotateClockwiseButton = addButton(FontAwesome.Solid.Redo, "Rotate 90 degrees clockwise (Ctrl->)", () => rotationHandler?.Rotate(90));
            }

            if (CanFlipX)
                addButton(FontAwesome.Solid.ArrowsAltH, "Flip horizontally", () => OnFlip?.Invoke(Direction.Horizontal, false));

            if (CanFlipY)
                addButton(FontAwesome.Solid.ArrowsAltV, "Flip vertically", () => OnFlip?.Invoke(Direction.Vertical, false));

            if (CanReverse)
                reverseButton = addButton(FontAwesome.Solid.Backward, "Reverse pattern (Ctrl-G)", () => OnReverse?.Invoke());
        }

        private SelectionBoxButton addButton(IconUsage icon, string tooltip, Action action)
        {
            var button = new SelectionBoxButton(icon, tooltip)
            {
                Action = action
            };

            button.Clicked += freezeButtonPosition;
            button.HoverLost += unfreezeButtonPosition;

            button.OperationStarted += operationStarted;
            button.OperationEnded += operationEnded;

            buttons.Add(button);

            return button;
        }

        private void clearButtons()
        {
            foreach (var button in buttons)
            {
                button.Clicked -= freezeButtonPosition;
                button.HoverLost -= unfreezeButtonPosition;

                button.OperationStarted -= operationStarted;
                button.OperationEnded -= operationEnded;
            }

            unfreezeButtonPosition();
            buttons.Clear();
        }

        /// <remarks>
        /// This method should be called when a selection needs to be flipped
        /// because of an ongoing scale handle drag that would otherwise cause width or height to go negative.
        /// </remarks>
        public void PerformFlipFromScaleHandles(Axes axes)
        {
            if (axes.HasFlag(Axes.X))
            {
                dragHandles.FlipScaleHandles(Direction.Horizontal);
                OnFlip?.Invoke(Direction.Horizontal, false);
            }

            if (axes.HasFlag(Axes.Y))
            {
                dragHandles.FlipScaleHandles(Direction.Vertical);
                OnFlip?.Invoke(Direction.Vertical, false);
            }
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

        private Vector2? frozenButtonsPosition;

        private void freezeButtonPosition()
        {
            frozenButtonsPosition = buttons.ScreenSpaceDrawQuad.TopLeft;
        }

        private void unfreezeButtonPosition()
        {
            if (frozenButtonsPosition != null)
            {
                frozenButtonsPosition = null;
                ensureButtonsOnScreen(true);
            }
        }

        private void ensureButtonsOnScreen(bool animated = false)
        {
            if (frozenButtonsPosition != null)
            {
                buttons.Anchor = Anchor.TopLeft;
                buttons.Origin = Anchor.TopLeft;

                buttons.Position = ToLocalSpace(frozenButtonsPosition.Value) - new Vector2(button_padding);
                return;
            }

            if (!animated && buttons.Transforms.Any())
                return;

            var thisQuad = ScreenSpaceDrawQuad;

            // Shrink the parent quad to give a bit of padding so the buttons don't stick *right* on the border.
            // AABBFloat assumes no rotation. one would hope the whole editor is not being rotated.
            var parentQuad = Parent!.ScreenSpaceDrawQuad.AABBFloat.Shrink(ToLocalSpace(thisQuad.TopLeft + new Vector2(button_padding * 2)));

            float topExcess = thisQuad.TopLeft.Y - parentQuad.TopLeft.Y;
            float bottomExcess = parentQuad.BottomLeft.Y - thisQuad.BottomLeft.Y;
            float leftExcess = thisQuad.TopLeft.X - parentQuad.TopLeft.X;
            float rightExcess = parentQuad.TopRight.X - thisQuad.TopRight.X;

            float minHeight = buttons.ScreenSpaceDrawQuad.Height;

            Anchor targetAnchor;
            Anchor targetOrigin;
            Vector2 targetPosition = Vector2.Zero;

            if (topExcess < minHeight && bottomExcess < minHeight)
            {
                targetAnchor = Anchor.BottomCentre;
                targetOrigin = Anchor.BottomCentre;
                targetPosition.Y = Math.Min(0, ToLocalSpace(Parent!.ScreenSpaceDrawQuad.BottomLeft).Y - DrawHeight);
            }
            else if (topExcess > bottomExcess)
            {
                targetAnchor = Anchor.TopCentre;
                targetOrigin = Anchor.BottomCentre;
            }
            else
            {
                targetAnchor = Anchor.BottomCentre;
                targetOrigin = Anchor.TopCentre;
            }

            targetPosition.X += ToLocalSpace(thisQuad.TopLeft - new Vector2(Math.Min(0, leftExcess)) + new Vector2(Math.Min(0, rightExcess))).X;

            if (animated)
            {
                var originalPosition = ToLocalSpace(buttons.ScreenSpaceDrawQuad.TopLeft);

                buttons.Origin = targetOrigin;
                buttons.Anchor = targetAnchor;
                buttons.Position = targetPosition;

                var newPosition = ToLocalSpace(buttons.ScreenSpaceDrawQuad.TopLeft);

                var delta = newPosition - originalPosition;

                buttons.Position -= delta;

                buttons.MoveTo(targetPosition, 300, Easing.OutQuint);
            }
            else
            {
                buttons.Anchor = targetAnchor;
                buttons.Origin = targetOrigin;
                buttons.Position = targetPosition;
            }
        }
    }
}
