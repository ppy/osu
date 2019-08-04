// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindUp : ModTimeRamp
    {
        public override string Name => "Wind Up";
        public override string Acronym => "WU";
        public override string Description => "Can you keep up?";
        public override IconUsage Icon => FontAwesome.Solid.ChevronCircleUp;
        public override double ScoreMultiplier => 1.0;

        protected override double FinalRateAdjustment => 0.5;

        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModWindDown)).ToArray();
    }
}
