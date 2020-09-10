using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Tau.Scoring
{
    public class TauScoreProcessor : ScoreProcessor
    {
        public override HitWindows CreateHitWindows() => new TauHitWindows();
    }
}
