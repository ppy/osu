// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableOsuHitObject, IDrawableHitObjectWithProxiedApproach
    {
        private OsuHitObject osuObject;

        public ApproachCircle ApproachCircle;
        private CirclePiece circle;
        private RingPiece ring;
        private FlashPiece flash;
        private ExplodePiece explode;
        private NumberPiece number;
        private GlowPiece glow;

        public DrawableHitCircle(OsuHitObject h) : base(h)
        {
            Origin = Anchor.Centre;

            osuObject = h;

            Position = osuObject.StackedPosition;
            Scale = new Vector2(osuObject.Scale);

            Children = new Drawable[]
            {
                glow = new GlowPiece
                {
                    Colour = osuObject.Colour
                },
                circle = new CirclePiece
                {
                    Colour = osuObject.Colour,
                    Hit = () =>
                    {
                        if (Judgement.Result.HasValue) return false;

                        ((PositionalJudgementInfo)Judgement).PositionOffset = Vector2.Zero; //todo: set to correct value
                        UpdateJudgement(true);
                        return true;
                    },
                },
                number = new NumberPiece()
                {
                    Text = h is Spinner ? "S" : (HitObject.ComboIndex + 1).ToString(),
                },
                ring = new RingPiece(),
                flash = new FlashPiece(),
                explode = new ExplodePiece
                {
                    Colour = osuObject.Colour,
                },
                ApproachCircle = new ApproachCircle()
                {
                    Colour = osuObject.Colour,
                }
            };

            //may not be so correct
            Size = circle.DrawSize;
        }

        double hit50 = 150;
        double hit100 = 80;
        double hit300 = 30;

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > hit50)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            OsuJudgementInfo osuJudgement = Judgement as OsuJudgementInfo;

            if (hitOffset < hit50)
            {
                Judgement.Result = HitResult.Hit;

                if (hitOffset < hit300)
                    osuJudgement.Score = OsuScoreResult.Hit300;
                else if (hitOffset < hit100)
                    osuJudgement.Score = OsuScoreResult.Hit100;
                else if (hitOffset < hit50)
                    osuJudgement.Score = OsuScoreResult.Hit50;
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
            if (!IsLoaded) return;

            base.UpdateState(state);

            ApproachCircle.FadeOut();

            glow.Delay(osuObject.Duration);
            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    Delay(osuObject.Duration + TIME_PREEMPT);
                    FadeOut(TIME_FADEOUT);
                    break;
                case ArmedState.Miss:
                    FadeOut(TIME_FADEOUT / 5);
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
                    break;
            }
        }

        public Drawable ProxiedLayer => ApproachCircle;
    }
}
