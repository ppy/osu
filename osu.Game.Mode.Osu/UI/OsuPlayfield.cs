//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Game.Modes.UI;
using OpenTK;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private Container approachCircles;
        private Container judgementLayer;

        public override Vector2 Size
        {
            get
            {
                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public OsuPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.75f);

            Add(new Drawable[]
            {
                judgementLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                approachCircles = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                }
            });
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;
            DrawableHitCircle c = h as DrawableHitCircle;
            if (c != null)
            {
                approachCircles.Add(c.ApproachCircle.CreateProxy());
            }

            h.OnJudgement += judgement;

            base.Add(h);
        }

        private void judgement(DrawableHitObject h, JudgementInfo j)
        {
            HitExplosion explosion = new HitExplosion((OsuJudgementInfo)j, (OsuHitObject)h.HitObject);

            judgementLayer.Add(explosion);
        }
    }
}