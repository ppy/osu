using System;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public class ModNoDrain : Mod, IApplicableToHealthProcessor
    {
        public override string Name => "健壮";
        public override LocalisableString Description => "禁用自动掉血";
        public override double ScoreMultiplier => 1;
        public override string Acronym => "NH";
        public override bool UserPlayable => false;

        public void ApplyToHealthProcessor(HealthProcessor healthProcessor)
        {
            if (healthProcessor is DrainingHealthProcessor drainingHealthProcessor)
            {
                drainingHealthProcessor.DrainRate = 0;
            }
            else throw new InvalidOperationException("healthProcessor不是drainingHealthProcessor");
        }
    }
}
