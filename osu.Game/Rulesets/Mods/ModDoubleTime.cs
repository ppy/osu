// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModDoubleTime : ModTimeAdjust, IApplicableToClock
    {
        public override string Name => "Double Time";
        public override string Acronym => "DT";
        public override IconUsage Icon => OsuIcon.ModDoubletime;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Zoooooooooom...";
        public override bool Ranked => true;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModHalfTime)).ToArray();

        protected override double RateAdjust => 1.5;
    }
}
