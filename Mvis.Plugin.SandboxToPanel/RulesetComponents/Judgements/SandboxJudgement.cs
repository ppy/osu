using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Judgements
{
    public partial class SandboxJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;
    }
}
