// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Objects.Legacy
{
    public static class LegacyConverterExtensions
    {
        public static int GetSplitCount(this ReadOnlySpan<char> text, char separator)
        {
            int count = 0;

            foreach (char c in text)
            {
                if (c == separator)
                    count++;
            }

            return count + 1;
        }

        public static ReadOnlySpan<char> Slice(this ReadOnlySpan<char> text, Range range)
            => text.Slice(range.Start.Value, range.Length());

        public static int Length(this Range range) => range.End.Value - range.Start.Value;
    }
}
