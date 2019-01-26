using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindDown<T> : ModWindUp<T>
        where T : HitObject
    {
        public override string Name => "Wind Down";
        public override string Acronym => "WD";
        public override string Description => "Slow down.";
        public override FontAwesome Icon => FontAwesome.fa_chevron_circle_down;
        public override double AppendRate => -0.25;
    }

}