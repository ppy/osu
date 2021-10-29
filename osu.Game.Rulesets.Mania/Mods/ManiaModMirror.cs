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
    public class ManiaModMirror : ModMirror, IApplicableToBeatmap
    {
        public override string Description => "Notes are flipped horizontally.";

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            int availableColumns = ((ManiaBeatmap)beatmap).TotalColumns;

            beatmap.HitObjects.OfType<ManiaHitObject>().ForEach(h => h.Column = availableColumns - 1 - h.Column);
        }
    }
}
