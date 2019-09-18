// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModFadeIn : Mod
    {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override IconUsage Icon => OsuIcon.ModHidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => @"Keys appear out of nowhere!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };
    }
}
