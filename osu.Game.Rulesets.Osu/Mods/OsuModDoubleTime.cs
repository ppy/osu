// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModDoubleTime : ModDoubleTime
    {
        public override double ScoreMultiplier => this.SpeedChange.Value >= 1.45 ? 1.12 : 1;
    }
}
