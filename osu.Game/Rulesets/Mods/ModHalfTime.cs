// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModHalfTime : ModTimeAdjust, IApplicableToClock
    {
        public override string Name => "Half Time";
        public override string Acronym => "HT";
        public override IconUsage Icon => OsuIcon.ModHalftime;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => "Less zoom...";
        public override bool Ranked => true;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModDoubleTime)).ToArray();

        protected override double RateAdjust => 0.75;
    }
}
