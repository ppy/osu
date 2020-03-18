// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Tau.Objects;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModNightcore : ModNightcore<TauHitObject>
    {
        public override double ScoreMultiplier => 1.12;
    }
}
