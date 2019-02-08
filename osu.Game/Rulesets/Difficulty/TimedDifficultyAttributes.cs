// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    public class TimedDifficultyAttributes
    {
        public readonly double Time;
        public readonly DifficultyAttributes Attributes;

        public TimedDifficultyAttributes(double time, DifficultyAttributes attributes)
        {
            this.Time = time;
            this.Attributes = attributes;
        }
    }
}
