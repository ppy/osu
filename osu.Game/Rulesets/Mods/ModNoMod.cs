// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Indicates a type of mod that doesn't do anything.
    /// </summary>
    public sealed class ModNoMod : Mod
    {
        public override string Name => "No Mod";
        public override string Acronym => "NM";
        public override double ScoreMultiplier => 1;
        public override IconUsage? Icon => FontAwesome.Solid.Ban;
        public override ModType Type => ModType.System;
    }
}
