// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Beatmaps;

namespace osu.Game.Rulesets.Taiko.Mods
{
    public class TaikoModConstantSpeed : Mod, IApplicableToBeatmap
    {
        public override string Name => "Constant Speed";
        public override string Acronym => "CS";
        public override double ScoreMultiplier => 0.8;
        public override LocalisableString Description => "No more tricky speed changes!";
        public override IconUsage? Icon => FontAwesome.Solid.Equals;
        public override ModType Type => ModType.Conversion;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var taikoBeatmap = (TaikoBeatmap)beatmap;

            foreach (var effectControlPoint in taikoBeatmap.ControlPointInfo.EffectPoints)
            {
                effectControlPoint.ScrollSpeed = 1;
            }
        }
    }
}
