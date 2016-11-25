//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableHitObject
    {
        private OsuHitObject osuObject;

        public ApproachCircle ApproachCircle;
        private CirclePiece circle;
        private RingPiece ring;
        private FlashPiece flash;
        private ExplodePiece explode;
        private NumberPiece number;
        private GlowPiece glow;
        private HitExplosion explosion;

        public DrawableHitCircle(HitCircle h) : base(h)
        {
            osuObject = h;

            Origin = Anchor.Centre;
            Position = osuObject.Position;

            Children = new Drawable[]
            {
                glow = new GlowPiece
                {
                    Colour = osuObject.Colour
                },
                circle = new CirclePiece
                {
                    Colour = osuObject.Colour,
                    Hit = () => Hit(new JudgementInfo {
                        UserTriggered = true,
                    }),
                },
                number = new NumberPiece(),
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //may not be so correct
            Size = circle.DrawSize;

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject
            ApproachCircle.Flush(true);

            double t = osuObject.EndTime + (Judgement?.TimeOffset ?? 0);

            Alpha = 0;

            //sane defaults
            ring.Alpha = circle.Alpha = number.Alpha = glow.Alpha = 1;
            ApproachCircle.Alpha = 0;
            ApproachCircle.Scale = new Vector2(2);
            explode.Alpha = 0;
            Scale = new Vector2(0.5f); //this will probably need to be moved to DrawableHitObject at some point.

            const float preempt = 600;

            const float fadein = 400;

            Delay(t - Time.Current - preempt, true);

            FadeIn(fadein);

            ApproachCircle.FadeIn(Math.Min(fadein * 2, preempt));
            ApproachCircle.ScaleTo(0.6f, preempt);

            Delay(preempt, true);

            ApproachCircle.FadeOut();

            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Idle:
                    Delay(osuObject.Duration + 500);
                    FadeOut(500);

                    explosion?.Expire();
                    explosion = null;
                    break;
                case ArmedState.Miss:
                    ring.FadeOut();
                    circle.FadeOut();
                    number.FadeOut();
                    glow.FadeOut();

                    explosion?.Expire();
                    explosion = null;

                    Schedule(() => Add(explosion = new HitExplosion(HitResult.Miss)));

                    FadeOut(800);
                    break;
                case ArmedState.Hit:
                    const double flash_in = 30;

                    flash.FadeTo(0.8f, flash_in);
                    flash.Delay(flash_in);
                    flash.FadeOut(100);

                    explode.FadeIn(flash_in);

                    Schedule(() => Add(explosion = new HitExplosion(Judgement.Result)));

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
    }
}
