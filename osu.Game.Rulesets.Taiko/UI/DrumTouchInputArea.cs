// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// An overlay that captures and displays Taiko mouse and touch input.
    /// The boundaries of this overlay defines the interactable area for touch input.
    /// A secondary InputDrum is attached by this overlay, which defines the circulary boundary which distinguishes "centre" from "rim" hits, and also displays input.
    /// </summary>
    public class DrumTouchInputArea : Container
    {
        // The percent of the drum that extends past the bottom of the screen (set to 0.0f to show the full drum)
        private const float offscreenPercent = 0.35f;
        private InputDrum touchInputDrum;
        private Circle  drumBackground;

        private KeyBindingContainer<TaikoAction> keyBindingContainer;

        // Which Taiko action was pressed by the last OnMouseDown event, so that the corresponding action can be released OnMouseUp even if the cursor position moved
        private TaikoAction mouseAction;

        // A map of (Finger Index OnTouchDown -> Which Taiko action was pressed), so that the corresponding action can be released OnTouchUp is released even if the touch position moved
        private Dictionary<TouchSource, TaikoAction> touchActions = new Dictionary<TouchSource, TaikoAction>(Enum.GetNames(typeof(TouchSource)).Length);

        private Container visibleComponents;

        public DrumTouchInputArea()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Children = new Drawable[]
            {
                visibleComponents = new Container() {
                    Alpha = 0.0f,
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        drumBackground = new Circle() {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fit,
                            Alpha = 0.9f,
                        },
                        touchInputDrum = new InputDrum() {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            Padding = new MarginPadding {
                Top = TaikoPlayfield.DEFAULT_HEIGHT * 2f, // Visual elements should start right below the playfield
                Bottom = -touchInputDrum.DrawHeight * offscreenPercent, // The drum should go past the bottom of the screen so that it can be wider
            };
        }

        [BackgroundDependencyLoader]
        private void load(TaikoInputManager taikoInputManager, OsuColour colours)
        {
            keyBindingContainer = taikoInputManager?.KeyBindingContainer;
            drumBackground.Colour = colours.Gray0;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            ShowTouchControls();
            mouseAction = getTaikoActionFromInput(e.ScreenSpaceMouseDownPosition);
            keyBindingContainer?.TriggerPressed(mouseAction);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            keyBindingContainer?.TriggerReleased(mouseAction);
            base.OnMouseUp(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            ShowTouchControls();
            TaikoAction taikoAction = getTaikoActionFromInput(e.ScreenSpaceTouchDownPosition);
            touchActions.Add(e.Touch.Source, taikoAction);
            keyBindingContainer?.TriggerPressed(touchActions[e.Touch.Source]);
            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            keyBindingContainer?.TriggerReleased(touchActions[e.Touch.Source]);
            touchActions.Remove(e.Touch.Source);
            base.OnTouchUp(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            HideTouchControls();
            return false;
        }

        public void ShowTouchControls()
        {
            visibleComponents.Animate(components => components.FadeIn(500, Easing.OutQuint));
        }

        public void HideTouchControls()
        {
            visibleComponents.Animate(components => components.FadeOut(2000, Easing.OutQuint));
        }

        private TaikoAction getTaikoActionFromInput(Vector2 inputPosition)
        {
            bool centreHit = inputIsCenterHit(inputPosition);
            bool leftSide = inputIsOnLeftSide(inputPosition);

            return centreHit ?
                (leftSide ? TaikoAction.LeftCentre : TaikoAction.RightCentre) :
                (leftSide ? TaikoAction.LeftRim : TaikoAction.RightRim);
        }

        private bool inputIsOnLeftSide(Vector2 inputPosition)
        {
            Vector2 inputPositionToDrumCentreDelta = touchInputDrum.ToLocalSpace(inputPosition) - touchInputDrum.OriginPosition;
            return inputPositionToDrumCentreDelta.X < 0f;
        }

        private bool inputIsCenterHit(Vector2 inputPosition)
        {
            Vector2 inputPositionToDrumCentreDelta = touchInputDrum.ToLocalSpace(inputPosition) - touchInputDrum.OriginPosition;

            float inputDrumRadius = Math.Max(touchInputDrum.Width, touchInputDrum.DrawHeight) / 2f;
            float centreRadius = (inputDrumRadius * touchInputDrum.centre_size);
            return inputPositionToDrumCentreDelta.Length <= centreRadius;
        }
    }
}
