using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModAutoHold : Mod
    {
        public override string Name => "Auto Hold";
        public override string Acronym => "AH";
        public override IconUsage? Icon => OsuIcon.ModSpunout;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => @"Hold beat will automatically be completed.";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
    }
}
