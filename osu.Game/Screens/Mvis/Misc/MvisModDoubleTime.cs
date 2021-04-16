using osu.Framework.Bindables;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Screens.Mvis.Misc
{
    internal class MvisModRateAdjust : ModRateAdjust
    {
        public override string Name => ToString();
        public override string Acronym => "RA";
        public override double ScoreMultiplier => 0;

        public override BindableNumber<double> SpeedChange { get; } = new BindableDouble
        {
            Value = 1,
            MaxValue = 2,
            MinValue = 0.1f
        };
    }
}
