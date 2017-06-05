// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Rulesets.Osu.OsuDifficulty.Skills
{
    public class Aim : Skill
    {
        protected override double skillMultiplier => 26.25;
        protected override double strainDecayBase => 0.15;

        protected override double strainValue() => Math.Pow(current.Distance, 0.99) / current.MS;
    }
}