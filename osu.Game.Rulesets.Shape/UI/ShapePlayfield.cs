using osu.Framework.Graphics;
using osu.Game.Rulesets.Shape.Objects;
using osu.Game.Rulesets.Shape.Objects.Drawables;
using osu.Game.Rulesets.UI;
using OpenTK;
using osu.Game.Rulesets.Shape.Judgements;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Shape.UI
{
    public class ShapePlayfield : Playfield
    {
        private Container shapePlayfield;
        private readonly Container judgementLayer;

        //public override bool ProvidingUserCursor => false;

        public static readonly Vector2 BASE_SIZE = new Vector2(512, 384);

        public override Vector2 Size
        {
            get
            {
                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public ShapePlayfield() : base(BASE_SIZE.X)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AddRange(new Drawable[]
            {
                shapePlayfield = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -3,
                },
                judgementLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -2,
                },
                //Will need custom UI like vitaru does it
            });
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;

            h.OnJudgement += onJudgement;

            IDrawableHitObjectWithProxiedApproach c = h as IDrawableHitObjectWithProxiedApproach;
            if (c != null)
                shapePlayfield.Add(c.ProxiedLayer.CreateProxy());

            base.Add(h);
        }

        private void onJudgement(DrawableHitObject judgedObject, Judgement judgement)
        {
            var shapeJudgement = (ShapeJudgement)judgement;
            var shapeObject = (ShapeHitObject)judgedObject.HitObject;

            DrawableShapeJudgement explosion = new DrawableShapeJudgement(shapeJudgement, judgedObject)
            {
                Scale = new Vector2(0.5f),
                Alpha = 0.5f,
                Origin = Anchor.Centre,
                Position = judgedObject.Position,
            };

            judgementLayer.Add(explosion);
        }
    }
}
