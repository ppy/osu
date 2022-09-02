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
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Catch.UI
{
    public class TouchInputField : VisibilityContainer
    {
        public enum TouchCatchAction
        {
            MoveLeft = 0,
            MoveRight = 1,
            DashLeft = 2,
            DashRight = 3,
            None = 4
        }

        private Dictionary<object, TouchCatchAction> trackedActions = new Dictionary<object, TouchCatchAction>();

        private KeyBindingContainer<CatchAction> keyBindingContainer = null!;

        private Container mainContent = null!;

        // Fill values with null because UI is not declared in constructor
        private ArrowHitbox leftBox = null!;
        private ArrowHitbox rightBox = null!;
        private ArrowHitbox leftDashBox = null!;
        private ArrowHitbox rightDashBox = null!;

        // Force input to be prossed even when hidden.
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
                                leftBox = new ArrowHitbox(TouchCatchAction.MoveLeft, ref trackedActions, colours.Gray3)
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                leftDashBox = new ArrowHitbox(TouchCatchAction.DashLeft, ref trackedActions, colours.Gray2)
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
                                rightBox = new ArrowHitbox(TouchCatchAction.MoveRight, ref trackedActions, colours.Gray3)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.Both,
                                    RelativePositionAxes = Axes.Both,
                                    Width = 0.5f,
                                },
                                rightDashBox = new ArrowHitbox(TouchCatchAction.DashRight, ref trackedActions, colours.Gray2)
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
            if (getTouchCatchActionFromInput(e.ScreenSpaceMousePosition) == TouchCatchAction.None)
                return false;

            handleDown(e.Button, e.ScreenSpaceMousePosition);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (getTouchCatchActionFromInput(e.ScreenSpaceMousePosition) == TouchCatchAction.None)
                return;

            handleUp(e.Button);
            base.OnMouseUp(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            return true;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            // I'm not sure if this is posible but let's be safe
            if (!trackedActions.ContainsKey(e.Button))
                trackedActions.Add(e.Button, TouchCatchAction.None);

            trackedActions[e.Button] = getTouchCatchActionFromInput(e.ScreenSpaceMousePosition);
            calculateActiveKeys();

            base.OnDrag(e);
        }
        protected override void OnTouchMove(TouchMoveEvent e)
        {
            // I'm not sure if this is posible but let's be safe
            if (!trackedActions.ContainsKey(e.Touch.Source))
                trackedActions.Add(e.Touch.Source, TouchCatchAction.None);

            trackedActions[e.Touch.Source] = getTouchCatchActionFromInput(e.ScreenSpaceTouchDownPosition);

            calculateActiveKeys();

            base.OnTouchMove(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            handleDown(e.Touch.Source, e.ScreenSpaceTouchDownPosition);
            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            handleUp(e.Touch.Source);
            base.OnTouchUp(e);
        }

        private void calculateActiveKeys()
        {
            if (trackedActions.ContainsValue(TouchCatchAction.DashLeft) || trackedActions.ContainsValue(TouchCatchAction.MoveLeft))
                keyBindingContainer.TriggerPressed(CatchAction.MoveLeft);
            else
                keyBindingContainer.TriggerReleased(CatchAction.MoveLeft);

            if (trackedActions.ContainsValue(TouchCatchAction.DashRight) || trackedActions.ContainsValue(TouchCatchAction.MoveRight))
                keyBindingContainer.TriggerPressed(CatchAction.MoveRight);
            else
                keyBindingContainer.TriggerReleased(CatchAction.MoveRight);

            if (trackedActions.ContainsValue(TouchCatchAction.DashRight) || trackedActions.ContainsValue(TouchCatchAction.DashLeft))
                keyBindingContainer.TriggerPressed(CatchAction.Dash);
            else
                keyBindingContainer.TriggerReleased(CatchAction.Dash);
        }

        private void handleDown(object source, Vector2 position)
        {
            Show();

            TouchCatchAction catchAction = getTouchCatchActionFromInput(position);

            // Not too sure how this can happen, but let's avoid throwing.
            if (trackedActions.ContainsKey(source))
                return;

            trackedActions.Add(source, catchAction);
            calculateActiveKeys();
        }

        private void handleUp(object source)
        {
            trackedActions.Remove(source);

            calculateActiveKeys();
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
            {
                Logger.Log(inputPosition.ToString());
                return TouchCatchAction.MoveRight;
            }
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

        private class ArrowHitbox : CompositeDrawable, IKeyBindingHandler<CatchAction>
        {
            private readonly TouchCatchAction handledAction;

            private readonly Box overlay;

            private readonly Dictionary<object, TouchCatchAction> trackedActions;

            private bool isHiglighted;

            public ArrowHitbox(TouchCatchAction handledAction, ref Dictionary<object, TouchCatchAction> trackedActions, Color4 colour)
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
    }
}
