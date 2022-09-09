// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchTouchInputMapper : VisibilityContainer
    {
        public override bool PropagatePositionalInputSubTree => true;
        public override bool PropagateNonPositionalInputSubTree => true;

        private readonly Dictionary<object, TouchCatchAction> trackedActionSources = new Dictionary<object, TouchCatchAction>();

        private KeyBindingContainer<CatchAction> keyBindingContainer = null!;

        private Container mainContent = null!;

        private InputArea leftBox = null!;
        private InputArea rightBox = null!;
        private InputArea leftDashBox = null!;
        private InputArea rightDashBox = null!;

        [BackgroundDependencyLoader]
        private void load(CatchInputManager catchInputManager, OsuColour colours)
        {
            keyBindingContainer = catchInputManager.KeyBindingContainer;

            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                mainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.15f,
                            Children = new Drawable[]
                            {
                                leftBox = new InputArea(TouchCatchAction.MoveLeft, trackedActionSources, colours.Gray3)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                },
                                leftDashBox = new InputArea(TouchCatchAction.DashLeft, trackedActionSources, colours.Gray2)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.15f,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Children = new Drawable[]
                            {
                                rightBox = new InputArea(TouchCatchAction.MoveRight, trackedActionSources, colours.Gray3)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                rightDashBox = new InputArea(TouchCatchAction.DashRight, trackedActionSources, colours.Gray2)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.5f,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                },
                            }
                        },
                    },
                },
            };
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            // Hide whenever the keyboard is used.
            Hide();
            return false;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            return handleDown(e.Button, e.ScreenSpaceMousePosition);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            handleDown(e.Touch.Source, e.ScreenSpaceTouch.Position);
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            // multiple mouse buttons may be pressed and handling the same action.
            foreach (MouseButton button in e.PressedButtons)
                handleMove(button, e.ScreenSpaceMousePosition);
            return true;
        }

        protected override void OnTouchMove(TouchMoveEvent e)
        {
            handleMove(e.Touch.Source, e.ScreenSpaceTouch.Position);
            base.OnTouchMove(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            handleUp(e.Button);
            base.OnMouseUp(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            handleUp(e.Touch.Source);
            base.OnTouchUp(e);
        }

        private void handleMove(object inputSource, Vector2 screenSpaceInputPosition)
        {
            Show();

            trackedActionSources[inputSource] = getTouchCatchActionFromInput(screenSpaceInputPosition);
            updatePressedActions();
        }

        private bool handleDown(object inputSource, Vector2 screenSpaceInputPosition)
        {
            TouchCatchAction action = getTouchCatchActionFromInput(screenSpaceInputPosition);

            if (action == TouchCatchAction.None)
                return false;

            trackedActionSources[inputSource] = action;
            updatePressedActions();

            return true;
        }

        private void handleUp(object source)
        {
            if (trackedActionSources.Remove(source))
                updatePressedActions();
        }

        private void updatePressedActions()
        {
            if (trackedActionSources.ContainsValue(TouchCatchAction.DashLeft) || trackedActionSources.ContainsValue(TouchCatchAction.MoveLeft))
                keyBindingContainer.TriggerPressed(CatchAction.MoveLeft);
            else
                keyBindingContainer.TriggerReleased(CatchAction.MoveLeft);

            if (trackedActionSources.ContainsValue(TouchCatchAction.DashRight) || trackedActionSources.ContainsValue(TouchCatchAction.MoveRight))
                keyBindingContainer.TriggerPressed(CatchAction.MoveRight);
            else
                keyBindingContainer.TriggerReleased(CatchAction.MoveRight);

            if (trackedActionSources.ContainsValue(TouchCatchAction.DashRight) || trackedActionSources.ContainsValue(TouchCatchAction.DashLeft))
                keyBindingContainer.TriggerPressed(CatchAction.Dash);
            else
                keyBindingContainer.TriggerReleased(CatchAction.Dash);
        }

        private TouchCatchAction getTouchCatchActionFromInput(Vector2 screenSpaceInputPosition)
        {
            if (leftDashBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.DashLeft;
            if (rightDashBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.DashRight;
            if (leftBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.MoveLeft;
            if (rightBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.MoveRight;

            return TouchCatchAction.None;
        }

        protected override void PopIn() => mainContent.FadeIn(500, Easing.OutQuint);

        protected override void PopOut() => mainContent.FadeOut(300);

        private class InputArea : CompositeDrawable, IKeyBindingHandler<CatchAction>
        {
            private readonly TouchCatchAction handledAction;

            private readonly Box overlay;

            private readonly IEnumerable<KeyValuePair<object, TouchCatchAction>> trackedActions;

            private bool isHighlighted;

            public InputArea(TouchCatchAction handledAction, IEnumerable<KeyValuePair<object, TouchCatchAction>> trackedActions, Color4 colour)
            {
                this.handledAction = handledAction;
                this.trackedActions = trackedActions;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        Width = 1,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Alpha = 0.8f,
                                Colour = colour,
                                Width = 1,
                                RelativeSizeAxes = Axes.Both,
                            },
                            overlay = new Box
                            {
                                Alpha = 0,
                                Colour = colour.Multiply(1.4f),
                                Blending = BlendingParameters.Additive,
                                Width = 1,
                                RelativeSizeAxes = Axes.Both,
                            }
                        }
                    }
                };
            }

            public bool OnPressed(KeyBindingPressEvent<CatchAction> _)
            {
                updateHighlight();
                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> _)
            {
                updateHighlight();
            }

            private void updateHighlight()
            {
                bool isHandling = trackedActions.Any(a => a.Value == handledAction);

                if (isHandling == isHighlighted)
                    return;

                isHighlighted = isHandling;
                overlay.FadeTo(isHighlighted ? 0.5f : 0, isHighlighted ? 80 : 400, Easing.OutQuint);
            }
        }

        public enum TouchCatchAction
        {
            MoveLeft = 0,
            MoveRight = 1,
            DashLeft = 2,
            DashRight = 3,
            None = 4
        }
    }
}
