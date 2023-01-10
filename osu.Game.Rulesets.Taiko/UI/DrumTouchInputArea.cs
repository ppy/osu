// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#pragma warning disable IDE0001 // Simplify Names

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Taiko.Configuration;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Taiko.UI
{
    using TaikoInput = TaikoAction; // Functionally identical to TaikoAction, it's just a readability thing
    /// <summary>
    /// An overlay that captures and displays osu!taiko mouse and touch input.
    /// </summary>
    public partial class DrumTouchInputArea : VisibilityContainer
    {
        // visibility state affects our child. we always want to handle input.
        public override bool PropagatePositionalInputSubTree => true;
        public override bool PropagateNonPositionalInputSubTree => true;

        private KeyBindingContainer<TaikoAction> keyBindingContainer = null!;

        private readonly Dictionary<object, TaikoAction> trackedActions = new Dictionary<object, TaikoAction>();

        private Container mainContent = null!;

        private QuarterCircle leftCentre = null!;
        private QuarterCircle rightCentre = null!;
        private QuarterCircle leftRim = null!;
        private QuarterCircle rightRim = null!;

        private readonly Bindable<TaikoTouchControlScheme> configTouchControlScheme = new Bindable<TaikoTouchControlScheme>();

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(TaikoInputManager taikoInputManager, TaikoRulesetConfigManager config)
        {
            Debug.Assert(taikoInputManager.KeyBindingContainer != null);
            keyBindingContainer = taikoInputManager.KeyBindingContainer;

            // Container should handle input everywhere.
            RelativeSizeAxes = Axes.Both;

            const float centre_region = 0.80f;

            config.BindWith(TaikoRulesetSetting.TouchControlScheme, configTouchControlScheme);

            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.X,
                    Height = 350,
                    Y = 20,
                    Masking = true,
                    FillMode = FillMode.Fit,
                    Children = new Drawable[]
                    {
                        mainContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                leftRim = new QuarterCircle(getTaikoActionFromInput(TaikoInput.LeftRim), getColourFromTaikoAction(getTaikoActionFromInput(TaikoInput.LeftRim)))
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = -2,
                                },
                                rightRim = new QuarterCircle(getTaikoActionFromInput(TaikoInput.RightRim), getColourFromTaikoAction(getTaikoActionFromInput(TaikoInput.RightRim)))
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = 2,
                                    Rotation = 90,
                                },
                                leftCentre = new QuarterCircle(getTaikoActionFromInput(TaikoInput.LeftCentre), getColourFromTaikoAction(getTaikoActionFromInput(TaikoInput.LeftCentre)))
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = -2,
                                    Scale = new Vector2(centre_region),
                                },
                                rightCentre = new QuarterCircle(getTaikoActionFromInput(TaikoInput.RightCentre), getColourFromTaikoAction(getTaikoActionFromInput(TaikoInput.RightCentre)))
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomRight,
                                    X = 2,
                                    Scale = new Vector2(centre_region),
                                    Rotation = 90,
                                }
                            }
                        },
                    }
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
            handleDown(e.Touch.Source, e.ScreenSpaceTouchDownPosition);
            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            handleUp(e.Touch.Source);
            base.OnTouchUp(e);
        }

        private void handleDown(object source, Vector2 position)
        {
            Show();

            TaikoInput taikoInput = getTaikoActionFromPosition(position);

            // Not too sure how this can happen, but let's avoid throwing.
            if (trackedActions.ContainsKey(source))
                return;

            trackedActions.Add(source, taikoInput);
            keyBindingContainer.TriggerPressed(taikoInput);
        }

        private void handleUp(object source)
        {
            keyBindingContainer.TriggerReleased(trackedActions[source]);
            trackedActions.Remove(source);
        }

        private bool validMouse(MouseButtonEvent e) =>
            leftRim.Contains(e.ScreenSpaceMouseDownPosition) || rightRim.Contains(e.ScreenSpaceMouseDownPosition);

        #pragma warning disable format
        private TaikoAction getTaikoActionFromInput(TaikoInput input)
        {
            switch (configTouchControlScheme.Value)
            {
                case TaikoTouchControlScheme.KDDK:

                    switch (input)
                    {
                        case TaikoInput.LeftRim:     return TaikoAction.LeftRim;
                        case TaikoInput.LeftCentre:  return TaikoAction.LeftCentre;
                        case TaikoInput.RightCentre: return TaikoAction.RightCentre;
                        case TaikoInput.RightRim:    return TaikoAction.RightRim;
                    }
                    break;

                case TaikoTouchControlScheme.DDKK:
                    switch (input)
                    {
                        case TaikoInput.LeftRim:     return TaikoAction.LeftCentre;
                        case TaikoInput.LeftCentre:  return TaikoAction.RightCentre;
                        case TaikoInput.RightCentre: return TaikoAction.LeftRim;
                        case TaikoInput.RightRim:    return TaikoAction.RightRim;
                    }
                    break;

                case TaikoTouchControlScheme.KKDD:
                    switch (input)
                    {
                        case TaikoInput.LeftRim:     return TaikoAction.LeftRim;
                        case TaikoInput.LeftCentre:  return TaikoAction.RightRim;
                        case TaikoInput.RightCentre: return TaikoAction.LeftCentre;
                        case TaikoInput.RightRim:    return TaikoAction.RightCentre;
                    }
                    break;
            }
            return TaikoAction.LeftCentre;
        }
        #pragma warning restore format

        private TaikoAction getTaikoActionFromPosition(Vector2 inputPosition)
        {
            bool centreHit = leftCentre.Contains(inputPosition) || rightCentre.Contains(inputPosition);
            bool leftSide = ToLocalSpace(inputPosition).X < DrawWidth / 2;
            TaikoInput input;

            if (leftSide)
                input = centreHit ? TaikoInput.LeftCentre : TaikoInput.LeftRim;
            else
                input = centreHit ? TaikoInput.RightCentre : TaikoInput.RightRim;

            return getTaikoActionFromInput(input);
        }

        protected override void PopIn()
        {
            mainContent.FadeIn(500, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            mainContent.FadeOut(300);
        }

        private Color4 getColourFromTaikoAction(TaikoAction handledAction)
        {
            #pragma warning disable format
            switch (handledAction)
            {
                case TaikoAction.LeftRim:     return colours.Blue;
                case TaikoAction.LeftCentre:  return colours.Red;
                case TaikoAction.RightCentre: return colours.Red;
                case TaikoAction.RightRim:    return colours.Blue;
            }
            #pragma warning restore format
            return colours.Red;
        }
        private partial class QuarterCircle : CompositeDrawable, IKeyBindingHandler<TaikoAction>
        {
            private readonly Circle overlay;

            private readonly TaikoAction handledAction;

            private readonly Circle circle;

            public override bool Contains(Vector2 screenSpacePos) => circle.Contains(screenSpacePos);

            public QuarterCircle(TaikoAction handledAction, Color4 colour)
            {
                this.handledAction = handledAction;
                RelativeSizeAxes = Axes.Both;

                FillMode = FillMode.Fit;

                InternalChildren = new Drawable[]
                {
                    new Container
                    {
                        Masking = true,
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            circle = new Circle
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = colour.Multiply(1.4f).Darken(2.8f),
                                Alpha = 0.8f,
                                Scale = new Vector2(2),
                            },
                            overlay = new Circle
                            {
                                Alpha = 0,
                                RelativeSizeAxes = Axes.Both,
                                Blending = BlendingParameters.Additive,
                                Colour = colour,
                                Scale = new Vector2(2),
                            }
                        }
                    },
                };
            }

            public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                if (e.Action == handledAction)
                    overlay.FadeTo(1f, 80, Easing.OutQuint);
                return false;
            }

            public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
            {
                if (e.Action == handledAction)
                    overlay.FadeOut(1000, Easing.OutQuint);
            }
        }
    }
}
