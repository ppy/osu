// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Wraps a <see cref="ISkillAttributes"/> object and adds a time value for which the attribute is valid.
    /// </summary>
    public class TimedSkillAttributes : IComparable<TimedSkillAttributes>
    {
        public readonly double Time;
        public readonly ISkillAttributes Attributes;

        public TimedSkillAttributes(ISkillAttributes attributes, double time)
        {
            Attributes = attributes;
            Time = time;
        }

        public int CompareTo(TimedSkillAttributes? other) => Time.CompareTo(other?.Time);
    }
}
