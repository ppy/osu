// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Rulesets.Objects
{
    public abstract class HitObjectParser
    {
        public abstract HitObject Parse(ReadOnlySpan<char> text);
    }
}
