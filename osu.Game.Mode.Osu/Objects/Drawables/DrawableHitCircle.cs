//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableHitObject
    {
        private OsuBaseHit osuObject;

        private ApproachCircle approachCircle;
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
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
                    Hit = Hit,
                },
                number = new NumberPiece(),
                ring = new RingPiece(),
                flash = new FlashPiece(),
                explode = new ExplodePiece
                {
                    Colour = osuObject.Colour,
                },
                approachCircle = new ApproachCircle()
                {
                    Colour = osuObject.Colour,
                }
            };

            //may not be so correct
            Size = circle.DrawSize;

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject
            approachCircle.Flush(true);

            double t = HitTime ?? osuObject.StartTime;

            Alpha = 0;

            //sane defaults
            ring.Alpha = circle.Alpha = number.Alpha = approachCircle.Alpha = glow.Alpha = 1;
            explode.Alpha = 0;
            Scale = new Vector2(0.5f); //this will probably need to be moved to DrawableHitObject at some point.

            //always-present transforms
            Transforms.Add(new TransformAlpha { StartTime = t - 1000, EndTime = t - 800, StartValue = 0, EndValue = 1 });

            approachCircle.Transforms.Add(new TransformScale { StartTime = t - 1000, EndTime = t, StartValue = new Vector2(2f), EndValue = new Vector2(0.6f) });

            //set transform delay to t==hitTime
            Delay(t - Time.Current, true);

            approachCircle.FadeOut();

            glow.FadeOut(400);

            switch (state)
            {
                case ArmedState.Disarmed:
                    Delay(osuObject.Duration + 200);
                    FadeOut(200);

                    explosion?.Expire();
                    explosion = null;
                    break;
                case ArmedState.Armed:
                    const double flash_in = 30;

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

                    Schedule(() => Add(explosion = new HitExplosion(Judgement.Hit300)));
                    break;
            }
        }
    }
}
