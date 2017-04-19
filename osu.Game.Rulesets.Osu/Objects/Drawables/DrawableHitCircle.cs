// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;

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
                        if (Judgement.Result != HitResult.None) return false;

                        Judgement.PositionOffset = Vector2.Zero; //todo: set to correct value
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

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > HitObject.HitWindowFor(OsuScoreResult.Hit50))
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            if (hitOffset < HitObject.HitWindowFor(OsuScoreResult.Hit50))
            {
                Judgement.Result = HitResult.Hit;
                Judgement.Score = HitObject.ScoreResultForOffset(hitOffset);
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override void UpdateInitialState()
        {
            base.UpdateInitialState();

            //sane defaults
            ring.Alpha = circle.Alpha = number.Alpha = glow.Alpha = 1;
            ApproachCircle.Alpha = 0;
            ApproachCircle.Scale = new Vector2(4);
            explode.Alpha = 0;
        }

        protected override void UpdatePreemptState()
        {
            base.UpdatePreemptState();

            ApproachCircle.FadeIn(Math.Min(TIME_FADEIN * 2, TIME_PREEMPT));
            ApproachCircle.ScaleTo(1.1f, TIME_PREEMPT);
        }

        protected override void UpdateState(ArmedState state)
        {
            base.UpdateState(state);

            ApproachCircle.FadeOut();

            double endTime = (HitObject as IHasEndTime)?.EndTime ?? HitObject.StartTime;
            double duration = endTime - HitObject.StartTime;

            glow.Delay(duration);
            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    Delay(duration + TIME_PREEMPT);
                    FadeOut(TIME_FADEOUT);
                    Expire(true);
                    break;
                case ArmedState.Miss:
                    FadeOut(TIME_FADEOUT / 5);
                    Expire();
                    break;
                case ArmedState.Hit:
                    const double flash_in = 40;

                    flash.FadeTo(0.8f, flash_in);
                    flash.Delay(flash_in);
                    flash.FadeOut(100);

                    explode.FadeIn(flash_in);

                    Delay(flash_in, true);

                    //after the flash, we can hide some elements that were behind it
                    ring.FadeOut();
                    circle.FadeOut();
                    number.FadeOut();

                    FadeOut(800);
                    ScaleTo(Scale * 1.5f, 400, EasingTypes.OutQuad);
                    Expire();
                    break;
            }
        }

        public Drawable ProxiedLayer => ApproachCircle;
    }
}
