// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModRelax : ModBlockFail
    {
        public override string Name => "Relax";
        public override string Acronym => "RX";
        public override string Description => @"你不用点击,只需移动,让你用来点击的手指放松一下";
        public override IconUsage Icon => OsuIcon.ModRelax;
        public override ModType Type => ModType.Automation;
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay), typeof(ModNoFail), typeof(ModSuddenDeath) };
    }
}
