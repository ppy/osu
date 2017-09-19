// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Judgements;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        public ApproachCircle ApproachCircle;
        private readonly CirclePiece circle;
        private readonly RingPiece ring;
        private readonly FlashPiece flash;
        private readonly ExplodePiece explode;
        private readonly NumberPiece number;
        private readonly GlowPiece glow;

        public DrawableHitCircle(OsuHitObject h) : base(h)
        {
            Origin = Anchor.Centre;

            Position = HitObject.StackedPosition;
            Scale = new Vector2(HitObject.Scale);

            Children = new Drawable[]
            {
                glow = new GlowPiece
                {
                    Colour = AccentColour
                },
                circle = new CirclePiece
                {
                    Colour = AccentColour,
                    Hit = () =>
                    {
                        if (AllJudged)
                            return false;

                        UpdateJudgement(true);
                        return true;
                    },
                },
                number = new NumberPiece
                {
                    Text = h is Spinner ? "S" : (HitObject.ComboIndex + 1).ToString(),
                },
                ring = new RingPiece(),
                flash = new FlashPiece(),
                explode = new ExplodePiece
                {
                    Colour = AccentColour,
                },
                ApproachCircle = new ApproachCircle
                {
                    Colour = AccentColour,
                }
            };

            //may not be so correct
            Size = circle.DrawSize;
        }

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > HitObject.HitWindowFor(HitResult.Meh))
                    AddJudgement(new OsuJudgement { Result = HitResult.Miss });
                return;
            }

            AddJudgement(new OsuJudgement
            {
                Result = HitObject.ScoreResultForOffset(Math.Abs(timeOffset)),
                PositionOffset = Vector2.Zero //todo: set to correct value
            });
        }

        protected override void UpdateInitialState()
        {
            base.UpdateInitialState();

            // sane defaults
            ring.Show();
            circle.Show();
            number.Show();
            glow.Show();

            ApproachCircle.Hide();
            ApproachCircle.ScaleTo(new Vector2(4));
            explode.Hide();
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            ApproachCircle.FadeIn(Math.Min(TIME_FADEIN * 2, TIME_PREEMPT));
            ApproachCircle.ScaleTo(1.1f, TIME_PREEMPT);
        }

        protected override void UpdateCurrentState(ArmedState state)
        {
            double duration = ((HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime) - HitObject.StartTime;

            glow.Delay(duration).FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    this.Delay(duration + TIME_PREEMPT).FadeOut(TIME_FADEOUT);
                    Expire(true);
                    break;
                case ArmedState.Miss:
                    ApproachCircle.FadeOut(50);
                    this.FadeOut(TIME_FADEOUT / 5);
                    Expire();
                    break;
                case ArmedState.Hit:
                    ApproachCircle.FadeOut(50);

                    const double flash_in = 40;
                    flash.FadeTo(0.8f, flash_in)
                         .Then()
                         .FadeOut(100);

                    explode.FadeIn(flash_in);

                    using (BeginDelayedSequence(flash_in, true))
                    {
                        //after the flash, we can hide some elements that were behind it
                        ring.FadeOut();
                        circle.FadeOut();
                        number.FadeOut();

                        this.FadeOut(800)
                            .ScaleTo(Scale * 1.5f, 400, Easing.OutQuad);
                    }

                    Expire();
                    break;
            }
        }

        public Drawable ProxiedLayer => ApproachCircle;
    }
}
