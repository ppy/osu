// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModSwap : Mod, IApplicableToBeatmap
    {
        public override string Name => "Swap";
        public override string Acronym => "SW";
        public override string Description => @"Dons become kats, kats become dons";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => base.IncompatibleMods.Append(typeof(ModRandom)).ToArray();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;

            foreach (var obj in taikoBeatmap.HitObjects)
            {
                if (obj is Hit hit)
                    hit.Type = hit.Type == HitType.Centre ? HitType.Rim : HitType.Centre;
            }
        }
    }
}
