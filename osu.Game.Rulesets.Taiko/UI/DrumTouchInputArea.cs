// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// An overlay that captures and displays osu!taiko mouse and touch input.
    /// </summary>
    public class DrumTouchInputArea : Container
    {
        private readonly Circle outerCircle;

        private KeyBindingContainer<TaikoAction> keyBindingContainer = null!;

        private readonly Dictionary<TouchSource, TaikoAction> trackedTouches = new Dictionary<TouchSource, TaikoAction>();
        private readonly Dictionary<MouseButton, TaikoAction> trackedMouseButtons = new Dictionary<MouseButton, TaikoAction>();

        private readonly Container mainContent;

        private readonly Circle centreCircle;

        public DrumTouchInputArea()
        {
            RelativeSizeAxes = Axes.X;
            Height = 300;

            Masking = true;

            Children = new Drawable[]
            {
                mainContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Height = 2,
                    Children = new Drawable[]
                    {
                        outerCircle = new Circle
                        {
                            FillMode = FillMode.Fit,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        centreCircle = new Circle
                        {
                            FillMode = FillMode.Fit,
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Scale = new Vector2(0.5f),
                        },
                        new Box
                        {
                            FillMode = FillMode.Fit,
                            RelativeSizeAxes = Axes.Y,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = Color4.Black,
                            Width = 3,
                        },
                    }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(TaikoInputManager taikoInputManager, OsuColour colours)
        {
            Debug.Assert(taikoInputManager.KeyBindingContainer != null);

            keyBindingContainer = taikoInputManager.KeyBindingContainer;

            outerCircle.Colour = colours.Gray0;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            mainContent.Show();

            TaikoAction taikoAction = getTaikoActionFromInput(e.ScreenSpaceMouseDownPosition);

            trackedMouseButtons.Add(e.Button, taikoAction);
            keyBindingContainer.TriggerPressed(taikoAction);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            keyBindingContainer.TriggerReleased(trackedMouseButtons[e.Button]);
            trackedMouseButtons.Remove(e.Button);
            base.OnMouseUp(e);
        }

        protected override bool OnTouchDown(TouchDownEvent e)
        {
            mainContent.Show();

            TaikoAction taikoAction = getTaikoActionFromInput(e.ScreenSpaceTouchDownPosition);

            trackedTouches.Add(e.Touch.Source, taikoAction);
            keyBindingContainer.TriggerPressed(taikoAction);
            return true;
        }

        protected override void OnTouchUp(TouchUpEvent e)
        {
            keyBindingContainer.TriggerReleased(trackedTouches[e.Touch.Source]);
            trackedTouches.Remove(e.Touch.Source);
            base.OnTouchUp(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            mainContent.Hide();
            return false;
        }

        private TaikoAction getTaikoActionFromInput(Vector2 inputPosition)
        {
            bool centreHit = centreCircle.ScreenSpaceDrawQuad.Contains(inputPosition);
            bool leftSide = ToLocalSpace(inputPosition).X < DrawWidth / 2;

            if (leftSide)
                return centreHit ? TaikoAction.LeftCentre : TaikoAction.LeftRim;

            return centreHit ? TaikoAction.RightCentre : TaikoAction.RightRim;
        }
    }
}
