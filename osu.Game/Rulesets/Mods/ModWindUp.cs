// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mods
{
    public class ModWindUp<T> : ModTimeRamp<T>
        where T : HitObject
    {
        public override string Name => "Wind Up";
        public override string Acronym => "WU";
        public override string Description => "Crank it up!";
        public override FontAwesome Icon => FontAwesome.fa_chevron_circle_up;
        public override double ScoreMultiplier => 1.0;
        public override double AppendRate => 0.5;
    }
}