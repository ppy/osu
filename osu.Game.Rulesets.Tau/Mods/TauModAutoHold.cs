// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Tau.Mods
{
    public class TauModAutoHold : Mod
    {
        public override string Name => "自动滑条";
        public override string Acronym => "AH";
        public override IconUsage? Icon => OsuIcon.ModSpunout;
        public override ModType Type => ModType.DifficultyReduction;
        public override string Description => @"滑条会自动完成";
        public override double ScoreMultiplier => 0.9;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModAutoplay) };
    }
}
