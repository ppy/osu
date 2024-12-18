// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Catch.UI
{
    public partial class CatchTouchInputMapper : VisibilityContainer
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
            const float width = 0.15f;
            // Ratio between normal move area height and total input height
            const float normal_area_height_ratio = 0.45f;

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
                            Width = width,
                            Children = new Drawable[]
                            {
                                leftBox = new InputArea(TouchCatchAction.MoveLeft, trackedActionSources)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = normal_area_height_ratio,
                                    Colour = colours.Gray9,
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                },
                                leftDashBox = new InputArea(TouchCatchAction.DashLeft, trackedActionSources)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 1 - normal_area_height_ratio,
                                },
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = width,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            Children = new Drawable[]
                            {
                                rightBox = new InputArea(TouchCatchAction.MoveRight, trackedActionSources)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = normal_area_height_ratio,
                                    Colour = colours.Gray9,
                                    Anchor = Anchor.BottomRight,
                                    Origin = Anchor.BottomRight,
                                },
                                rightDashBox = new InputArea(TouchCatchAction.DashRight, trackedActionSources)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Height = 1 - normal_area_height_ratio,
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

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            return updateAction(e.Touch.Source, getTouchCatchActionFromInput(e.ScreenSpaceTouch.Position));
        }

        protected override void OnTouchMove(TouchMoveEvent e)
        {
            updateAction(e.Touch.Source, getTouchCatchActionFromInput(e.ScreenSpaceTouch.Position));
            base.OnTouchMove(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            updateAction(e.Touch.Source, null);
            base.OnTouchUp(e);
        }

        private bool updateAction(object source, TouchCatchAction? newAction)
        {
            TouchCatchAction? actionBefore = null;

            if (trackedActionSources.TryGetValue(source, out TouchCatchAction found))
                actionBefore = found;

            if (actionBefore != newAction)
            {
                if (newAction != null)
                    trackedActionSources[source] = newAction.Value;
                else
                    trackedActionSources.Remove(source);

                updatePressedActions();
            }

            return newAction != null;
        }

        private void updatePressedActions()
        {
            Show();

            if (trackedActionSources.ContainsValue(TouchCatchAction.DashLeft) || trackedActionSources.ContainsValue(TouchCatchAction.MoveLeft))
                keyBindingContainer.TriggerPressed(CatchAction.MoveLeft);
            else
                keyBindingContainer.TriggerReleased(CatchAction.MoveLeft);

            if (trackedActionSources.ContainsValue(TouchCatchAction.DashRight) || trackedActionSources.ContainsValue(TouchCatchAction.MoveRight))
                keyBindingContainer.TriggerPressed(CatchAction.MoveRight);
            else
                keyBindingContainer.TriggerReleased(CatchAction.MoveRight);

            if (trackedActionSources.ContainsValue(TouchCatchAction.DashLeft) || trackedActionSources.ContainsValue(TouchCatchAction.DashRight))
                keyBindingContainer.TriggerPressed(CatchAction.Dash);
            else
                keyBindingContainer.TriggerReleased(CatchAction.Dash);
        }

        private TouchCatchAction? getTouchCatchActionFromInput(Vector2 screenSpaceInputPosition)
        {
            if (leftDashBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.DashLeft;
            if (rightDashBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.DashRight;
            if (leftBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.MoveLeft;
            if (rightBox.Contains(screenSpaceInputPosition))
                return TouchCatchAction.MoveRight;

            return null;
        }

        protected override void PopIn() => mainContent.FadeIn(300, Easing.OutQuint);

        protected override void PopOut() => mainContent.FadeOut(300, Easing.OutQuint);

        private partial class InputArea : CompositeDrawable, IKeyBindingHandler<CatchAction>
        {
            private readonly TouchCatchAction handledAction;

            private readonly Box highlightOverlay;

            private readonly IEnumerable<KeyValuePair<object, TouchCatchAction>> trackedActions;

            private bool isHighlighted;

            public InputArea(TouchCatchAction handledAction, IEnumerable<KeyValuePair<object, TouchCatchAction>> trackedActions)
            {
                this.handledAction = handledAction;
                this.trackedActions = trackedActions;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 10,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.15f,
                            },
                            highlightOverlay = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                Blending = BlendingParameters.Additive,
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
                highlightOverlay.FadeTo(isHighlighted ? 0.1f : 0, isHighlighted ? 80 : 400, Easing.OutQuint);
            }
        }

        public enum TouchCatchAction
        {
            MoveLeft,
            MoveRight,
            DashLeft,
            DashRight,
        }
    }
}
