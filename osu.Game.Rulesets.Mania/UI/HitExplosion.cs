using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.UI
{
    internal class HitExplosion : CompositeDrawable
    {
        private readonly Box inner;

        public HitExplosion(DrawableHitObject<ManiaHitObject, ManiaJudgement> judgedObject)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;

            RelativeSizeAxes = Axes.Both;
            FillMode = FillMode.Fit;

            BlendingMode = BlendingMode.Additive;

            InternalChild = new CircularContainer
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                BorderThickness = 1,
                BorderColour = judgedObject.AccentColour,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = judgedObject.AccentColour,
                    Radius = 10,
                    Hollow = true
                },
                Child = inner = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = judgedObject.AccentColour,
                    Alpha = 1,
                    AlwaysPresent = true,
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.ScaleTo(2f, 600, Easing.OutQuint).FadeOut(500);
            inner.FadeOut(250);
        }
    }
}
