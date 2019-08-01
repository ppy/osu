using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModQuickTime : ModTimeAdjust, IApplicableToClock
    {
        public override string Name => "Quick Time";

        public override string Acronym => "QT";

        public override IconUsage Icon => FontAwesome.Solid.ArrowUp;

        public override ModType Type => ModType.DifficultyIncrease;

        public override string Description => "Zoom.";

        public override bool Ranked => true;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModHalfTime)).ToArray();

        protected override double RateAdjust => throw new System.NotImplementedException();
    }
}
