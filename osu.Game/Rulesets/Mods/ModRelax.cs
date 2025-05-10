// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRelax : Mod
    {
        public override string Name => "Relax";
        public override string Acronym => "RX";
        public override IconUsage? Icon => OsuIcon.ModRelax;
        public override ModType Type => ModType.Automation;
        public override double ScoreMultiplier => 0.1;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
    }
}
