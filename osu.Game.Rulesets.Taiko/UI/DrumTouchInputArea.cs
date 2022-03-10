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
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Rulesets.UI;
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
        private const float overhangPercent = 0.33f;
        private readonly InputDrum touchInputDrum;

        [Resolved(canBeNull: true)]
        private TaikoInputManager taikoInputManager { get; set; }

        private KeyBindingContainer<TaikoAction> keyBindingContainer;

        // Which Taiko action was pressed by the last OnMouseDown event, so that the corresponding action can be released OnMouseUp even if the cursor position moved
        private TaikoAction mouseAction;

        // A map of (Finger Index OnTouchDown -> Which Taiko action was pressed), so that the corresponding action can be released OnTouchUp is released even if the touch position moved
        private Dictionary<TouchSource, TaikoAction> touchActions = new Dictionary<TouchSource, TaikoAction>(Enum.GetNames(typeof(TouchSource)).Length);

        private Playfield playfield;

        public DrumTouchInputArea(Playfield playfield) {
            this.playfield = playfield;

            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box() {
                    Alpha = 0.0f,
                    Colour = new OsuColour().Blue,

                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                },
                new Container() {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,

                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,

                    Children = new Drawable[]
                    {
                        touchInputDrum = new InputDrum(playfield.HitObjectContainer) {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    }
                },
            };

        }
        protected override void LoadComplete()
        {
            keyBindingContainer = taikoInputManager?.KeyBindingContainer;

            Padding = new MarginPadding {
                Top =  playfield.ScreenSpaceDrawQuad.BottomLeft.Y,
                Bottom = -DrawHeight * overhangPercent,
            };
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            mouseAction = getTaikoActionFromInput(e.ScreenSpaceMouseDownPosition);
            keyBindingContainer?.TriggerPressed(mouseAction);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            keyBindingContainer?.TriggerReleased(mouseAction);
            base.OnMouseUp(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            TaikoAction taikoAction = getTaikoActionFromInput(e.ScreenSpaceTouchDownPosition);
            if (touchActions.ContainsKey(e.Touch.Source)) {
                touchActions[e.Touch.Source] = taikoAction;
            }
            else {
                touchActions.Add(e.Touch.Source, taikoAction);
            }
            keyBindingContainer?.TriggerPressed(touchActions[e.Touch.Source]);

            return base.OnTouchDown(e);
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            keyBindingContainer?.TriggerReleased(touchActions[e.Touch.Source]);
            base.OnTouchUp(e);
        }

        private TaikoAction getTaikoActionFromInput(Vector2 inputPosition) {
            bool leftSide = inputIsOnLeftSide(inputPosition);
            bool centreHit = inputIsCenterHit(inputPosition);

            return leftSide ?
                (centreHit ? TaikoAction.LeftCentre : TaikoAction.LeftRim) :
                (centreHit ? TaikoAction.RightCentre : TaikoAction.RightRim);
        }

        private bool inputIsOnLeftSide(Vector2 inputPosition) {
            Vector2 inputPositionToDrumCentreDelta = touchInputDrum.ToLocalSpace(inputPosition) - touchInputDrum.OriginPosition;
            return inputPositionToDrumCentreDelta.X < 0f;
        }

        private bool inputIsCenterHit(Vector2 inputPosition) {
            Vector2 inputPositionToDrumCentreDelta = touchInputDrum.ToLocalSpace(inputPosition) - touchInputDrum.OriginPosition;

            float inputDrumRadius = Math.Max(touchInputDrum.Width, touchInputDrum.DrawHeight) / 2f;
            float centreRadius = (inputDrumRadius * InputDrum.centre_size);
            return inputPositionToDrumCentreDelta.Length <= centreRadius;
        }
    }
}
