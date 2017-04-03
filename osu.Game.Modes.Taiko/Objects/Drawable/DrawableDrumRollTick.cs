// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Modes.Taiko.Judgements;
using System;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Taiko.Objects.Drawable
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject
    {
        /// <summary>
        /// The size of a tick.
        /// </summary>
        private const float tick_size = TaikoHitObject.CIRCLE_RADIUS / 2;
        
        /// <summary>
        /// Any tick that is not the first for a drumroll is not filled, but is instead displayed
        /// as a hollow circle. This is what controls the border width of that circle.
        /// </summary>
        private const float tick_border_width = tick_size / 4;

        private readonly DrumRollTick tick;

        private readonly CircularContainer bodyContainer;

        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            this.tick = tick;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.Centre;

            RelativePositionAxes = Axes.X;
            Size = new Vector2(tick_size);

            Children = new[]
            {
                bodyContainer = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = tick_border_width,
                    BorderColour = Color4.White,
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = tick.FirstTick ? 1 : 0,
                            AlwaysPresent = true
                        }
                    }
                }
            };
        }

        protected override TaikoJudgement CreateJudgement() => new TaikoDrumRollTickJudgement { SecondHit = tick.IsStrong };

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > tick.HitWindow)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            if (Math.Abs(Judgement.TimeOffset) < tick.HitWindow)
            {
                Judgement.Result = HitResult.Hit;
                Judgement.TaikoResult = TaikoHitResult.Great;
            }
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    bodyContainer.ScaleTo(0, 100, EasingTypes.OutQuint);
                    break;
            }
        }

        protected override void UpdateScrollPosition(double time)
        {
            // Ticks don't move
        }

        protected override bool HandleKeyPress(Key key)
        {
            return Judgement.Result == HitResult.None && UpdateJudgement(true);
        }
    }
}
