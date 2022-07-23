// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// A mod preset is a named collection of configured mods.
    /// Presets are presented to the user in the mod select overlay for convenience.
    /// </summary>
    public class ModPreset
    {
        /// <summary>
        /// The ruleset that the preset is valid for.
        /// </summary>
        public RulesetInfo RulesetInfo { get; set; } = null!;

        /// <summary>
        /// The name of the mod preset.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The description of the mod preset.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The set of configured mods that are part of the preset.
        /// </summary>
        public ICollection<Mod> Mods { get; set; } = Array.Empty<Mod>();
    }
}
