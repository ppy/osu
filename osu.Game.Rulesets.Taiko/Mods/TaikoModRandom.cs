// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModRandom : ModRandom, IApplicableToBeatmap
    {
        public override string Description => @"Shuffle around the colours!";

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;

            foreach (var obj in taikoBeatmap.HitObjects)
            {
                if (obj is Hit hit)
                    hit.Type = RNG.Next(2) == 0 ? HitType.Centre : HitType.Rim;
            }
        }
    }
}
