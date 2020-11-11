// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModMirror : Mod, IApplicableToBeatmap
    {
        public override string Name => "Mirror";
        public override string Acronym => "MR";
        public override ModType Type => ModType.Conversion;
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var availableColumns = ((ManiaBeatmap)beatmap).TotalColumns;

            beatmap.HitObjects.OfType<ManiaHitObject>().ForEach(h => h.Column = availableColumns - 1 - h.Column);
        }
    }
}
