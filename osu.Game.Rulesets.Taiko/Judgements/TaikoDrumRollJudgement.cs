using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    class TaikoDrumRollJudgement : TaikoJudgement
    {
        public override bool AffectsCombo => false;
        
        protected override int NumericResultFor(HitResult result) => 0;
    }
}
