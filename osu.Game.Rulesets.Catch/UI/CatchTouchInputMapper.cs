// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using System.Diagnostics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK.Graphics;
using osuTK;
using System.Collections.Generic;
using osuTK.Input;

namespace osu.Game.Rulesets.Catch.UI
{
    public class CatchTouchInputMapper : VisibilityContainer
    {
        private Dictionary<object, TouchCatchAction> trackedActionSources = new Dictionary<object, TouchCatchAction>();

        private KeyBindingContainer<CatchAction> keyBindingContainer = null!;

        private Container mainContent = null!;

        private InputArea leftBox = null!;
        private InputArea rightBox = null!;
        private InputArea leftDashBox = null!;
        private InputArea rightDashBox = null!;

        public override bool PropagatePositionalInputSubTree => true;
        public override bool PropagateNonPositionalInputSubTree => true;

        [BackgroundDependencyLoader]
        private void load(CatchInputManager catchInputManager, OsuColour colours)
        {
            Debug.Assert(catchInputManager.KeyBindingContainer != null);

            keyBindingContainer = catchInputManager.KeyBindingContainer;

            // Container should handle input everywhere.
            RelativeSizeAxes = Axes.Both;

            Children = new Drawable[]
            {
                mainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.15f,
                            Height = 1,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Children = new Drawable[]
                            {
                                leftBox = new InputArea(TouchCatchAction.MoveLeft, ref trackedActionSources, colours.Gray3)
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                leftDashBox = new InputArea(TouchCatchAction.DashLeft, ref trackedActionSources, colours.Gray2)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 0.5f,
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.15f,
                            Height = 1,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            Children = new Drawable[]
                            {
                                rightBox = new InputArea(TouchCatchAction.MoveRight, ref trackedActionSources, colours.Gray3)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                rightDashBox = new InputArea(TouchCatchAction.DashRight, ref trackedActionSources, colours.Gray2)
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 0.5f,
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

        protected override void OnMouseUp(MouseUpEvent e)
        {
            handleUp(e.Button);
            base.OnMouseUp(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            Show();

            TouchCatchAction touchCatchAction = getTouchCatchActionFromInput(e.ScreenSpaceMousePosition);

            // Loop through the buttons to avoid keeping a button pressed if both mouse buttons are pressed.
            foreach (MouseButton i in e.PressedButtons)
                trackedActionSources[i] = touchCatchAction;

            calculateActiveKeys();
            return true;
        }

        protected override void OnTouchMove(TouchMoveEvent e)
        {
            Show();

            trackedActionSources[e.Touch.Source] = getTouchCatchActionFromInput(e.ScreenSpaceTouch.Position);
            calculateActiveKeys();

            base.OnTouchMove(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            handleDown(e.Touch.Source, e.ScreenSpaceTouch.Position);
            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            handleUp(e.Touch.Source);
            base.OnTouchUp(e);
        }

        private bool handleDown(object source, Vector2 position)
        {
            TouchCatchAction catchAction = getTouchCatchActionFromInput(position);

            if (catchAction == TouchCatchAction.None)
                return false;

            trackedActionSources[source] = catchAction;

            calculateActiveKeys();

            return true;
        }

        private void handleUp(object source)
        {
            trackedActionSources.Remove(source);

            calculateActiveKeys();
        }

        private void calculateActiveKeys()
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

        private TouchCatchAction getTouchCatchActionFromInput(Vector2 inputPosition)
        {
            if (leftDashBox.Contains(inputPosition))
                return TouchCatchAction.DashLeft;
            if (rightDashBox.Contains(inputPosition))
                return TouchCatchAction.DashRight;
            if (leftBox.Contains(inputPosition))
                return TouchCatchAction.MoveLeft;
            if (rightBox.Contains(inputPosition))
                return TouchCatchAction.MoveRight;

            return TouchCatchAction.None;
        }

        protected override void PopIn()
        {
            mainContent.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            mainContent.FadeOut(300);
        }

        private class InputArea : CompositeDrawable, IKeyBindingHandler<CatchAction>
        {
            private readonly TouchCatchAction handledAction;

            private readonly Box overlay;

            private readonly Dictionary<object, TouchCatchAction> trackedActions;

            private bool isHiglighted;

            public InputArea(TouchCatchAction handledAction, ref Dictionary<object, TouchCatchAction> trackedActions, Color4 colour)
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
                if (trackedActions.ContainsValue(handledAction))
                {
                    isHiglighted = true;
                    overlay.FadeTo(0.5f, 80, Easing.OutQuint);
                }

                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<CatchAction> _)
            {
                if (isHiglighted && !trackedActions.ContainsValue(handledAction))
                {
                    isHiglighted = false;
                    overlay.FadeOut(1000, Easing.Out);
                }
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
