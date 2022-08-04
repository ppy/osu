using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public interface IApplicableToTaikoClassic : IApplicableMod
    {
        public void ApplyToTaikoModClassic(TaikoModClassic taikoModClassic);
    }
}