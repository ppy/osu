using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.LLin.Misc
{
    internal class LLinModRateAdjust : ModRateAdjust
    {
        public override string Name => ToString();
        public override string Acronym => "RA";
        public override string Description => "missingno";
        public override double ScoreMultiplier => 0;

        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            Value = 1,
            MaxValue = 2,
            MinValue = 0.1f
        };
    }
}
