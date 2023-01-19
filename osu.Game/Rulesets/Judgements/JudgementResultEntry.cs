// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Judgements
{
    internal class JudgementResultEntry : IComparable<JudgementResultEntry>
    {
        public readonly double Time;

        public readonly HitObjectLifetimeEntry HitObjectEntry;

        public readonly JudgementResult Result;

        public JudgementResultEntry(double time, HitObjectLifetimeEntry hitObjectEntry, JudgementResult result)
        {
            Time = time;
            HitObjectEntry = hitObjectEntry;
            Result = result;
        }

        public int CompareTo(JudgementResultEntry? other) => Time.CompareTo(other?.Time);
    }
}