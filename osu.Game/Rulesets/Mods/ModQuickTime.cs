// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModSlowTime)).Append(typeof(ModHalfTime)).ToArray();

        protected override double RateAdjust => 1.25;
    }
}
