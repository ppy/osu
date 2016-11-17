//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Transformations;
using osu.Framework.IO.Stores;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects.Drawables.Pieces;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class DrawableHitCircle : DrawableHitObject
    {
        private Sprite approachCircle;
        private CirclePiece circle;
        private RingPiece ring;
        private FlashPiece flash;
        private ExplodePiece explode;
        private NumberPiece number;
        private GlowPiece glow;
        private OsuBaseHit h;
        private HitExplosion explosion;

        public DrawableHitCircle(HitCircle h) : base(h)
        {
            this.h = h;

            Origin = Anchor.Centre;
            Position = h.Position;

            Children = new Drawable[]
            {
                glow = new GlowPiece
                {
                    Colour = h.Colour
                },
                circle = new CirclePiece
                {
                    Colour = h.Colour,
                    Hit = Hit,
                },
                number = new NumberPiece(),
                ring = new RingPiece(),
                flash = new FlashPiece(),
                explode = new ExplodePiece
                {
                    Colour = h.Colour,
                },
                approachCircle = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = h.Colour
                }
            };

            //may not be so correct
            Size = circle.DrawSize;
        }

        [BackgroundDependencyLoader]
        private void load(BaseGame game)
        {
            approachCircle.Texture = game.Textures.Get(@"Play/osu/approachcircle@2x");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            //force application of the state that was set before we loaded.
            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded) return;

            Flush(true); //move to DrawableHitObject

            double t = HitTime ?? h.StartTime;

            //sane defaults
            ring.Alpha = circle.Alpha = number.Alpha = approachCircle.Alpha = glow.Alpha = 1;
            explode.Alpha = 0;
            Scale = Vector2.One;

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
                    Delay(h.Duration + 200);
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
