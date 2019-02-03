// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Pieces
{
    public class SliderBall : CircularContainer, ISliderProgress
    {
        private const float width = 128;

        private Color4 accentColour = Color4.Black;

        /// <summary>
        /// The colour that is used for the slider ball.
        /// </summary>
        public Color4 AccentColour
        {
            get { return accentColour; }
            set
            {
                accentColour = value;
                if (drawableBall != null)
                    drawableBall.Colour = value;
            }
        }

        private readonly Slider slider;
        public readonly Drawable FollowCircle;
        private Drawable drawableBall;
        private readonly DrawableSlider drawableSlider;

        public SliderBall(Slider slider, DrawableSlider drawableSlider = null)
        {
            this.drawableSlider = drawableSlider;
            this.slider = slider;
            Masking = true;
            AutoSizeAxes = Axes.Both;
            Blending = BlendingMode.Additive;
            Origin = Anchor.Centre;

            Children = new[]
            {
                FollowCircle = new Container
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Width = width,
                    Height = width,
                    Alpha = 0,
                    Child = new SkinnableDrawable("Play/osu/sliderfollowcircle", _ => new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = 5,
                        BorderColour = Color4.Orange,
                        Blending = BlendingMode.Additive,
                        Child = new Box
                        {
                            Colour = Color4.Orange,
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.2f,
                        }
                    }),
                },
                new CircularContainer
                {
                    Masking = true,
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Alpha = 1,
                    Child = new Container
                    {
                        Width = width,
                        Height = width,
                        // TODO: support skin filename animation (sliderb0, sliderb1...)
                        Child = new SkinnableDrawable("Play/osu/sliderb", _ => new CircularContainer
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both,
                            BorderThickness = 10,
                            BorderColour = Color4.White,
                            Alpha = 1,
                            Child = drawableBall = new Box
                            {
                                Colour = AccentColour,
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.4f,
                            }
                        }),
                    }
                }
            };
        }

        private Vector2? lastScreenSpaceMousePosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            lastScreenSpaceMousePosition = e.ScreenSpaceMousePosition;
            return base.OnMouseDown(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            lastScreenSpaceMousePosition = e.ScreenSpaceMousePosition;
            return base.OnMouseUp(e);
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            lastScreenSpaceMousePosition = e.ScreenSpaceMousePosition;
            return base.OnMouseMove(e);
        }

        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null)
        {
            // Consider the case of rewinding - children's transforms are handled internally, so propagating down
            // any further will cause weirdness with the Tracking bool below. Let's not propagate further at this point.
            base.ClearTransformsAfter(time, false, targetMember);
        }

        private bool tracking;

        public bool Tracking
        {
            get { return tracking; }
            private set
            {
                if (value == tracking)
                    return;
                tracking = value;

                FollowCircle.ScaleTo(tracking ? 2f : 1, 300, Easing.OutQuint);
                FollowCircle.FadeTo(tracking ? 1f : 0, 300, Easing.OutQuint);
            }
        }

        private bool canCurrentlyTrack => Time.Current >= slider.StartTime && Time.Current < slider.EndTime;

        protected override void Update()
        {
            base.Update();

            if (Time.Current < slider.EndTime)
            {
                // Make sure to use the base version of ReceivePositionalInputAt so that we correctly check the position.
                Tracking = canCurrentlyTrack
                           && lastScreenSpaceMousePosition.HasValue
                           && ReceivePositionalInputAt(lastScreenSpaceMousePosition.Value)
                           && (drawableSlider?.OsuActionInputManager?.PressedActions.Any(x => x == OsuAction.LeftButton || x == OsuAction.RightButton) ?? false);
            }
        }

        public void UpdateProgress(double completionProgress)
        {
            Position = slider.CurvePositionAt(completionProgress);
        }
    }
}
