// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets
{
    public class RulesetLoadException : Exception
    {
        public RulesetLoadException(string message)
            : base(@$"Ruleset could not be loaded ({message})")
        {
        }
    }
}
