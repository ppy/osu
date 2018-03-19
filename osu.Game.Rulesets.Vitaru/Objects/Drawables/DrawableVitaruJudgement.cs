using osu.Framework.Graphics;
using OpenTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Vitaru.Objects.Drawables
{
    public class DrawableVitaruJudgement : DrawableJudgement
    {
        public DrawableVitaruJudgement(Judgement judgement, DrawableHitObject judgedObject)
            : base(judgement, judgedObject)
        {
        }

        protected override void LoadComplete()
        {
            if (Judgement.Result != HitResult.Miss)
                JudgementText.TransformSpacingTo(new Vector2(14, 0), 1800, Easing.OutQuint);

            base.LoadComplete();
        }
    }
}
