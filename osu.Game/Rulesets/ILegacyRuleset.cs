// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets
{
    public interface ILegacyRuleset
    {
        /// <summary>
        /// Identifies the server-side ID of a legacy ruleset.
        /// </summary>
        int LegacyID { get; }
    }
}
