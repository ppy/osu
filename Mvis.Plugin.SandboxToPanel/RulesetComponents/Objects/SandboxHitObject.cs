using Mvis.Plugin.SandboxToPanel.RulesetComponents.Judgements;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Objects
{
    public partial class SandboxHitObject : HitObject
    {
        protected override HitWindows CreateHitWindows() => HitWindows.Empty;

        public override Judgement CreateJudgement() => new SandboxJudgement();
    }
}
