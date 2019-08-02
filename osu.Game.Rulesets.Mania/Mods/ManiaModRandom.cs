// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics.Sprites;
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModRandom : Mod, IApplicableToBeatmap
    {
        public override string Name => "Random";
        public override string Acronym => "RD";
        public override ModType Type => ModType.Conversion;
        public override IconUsage Icon => OsuIcon.Dice;
        public override string Description => @"Shuffle around the keys!";
        public override double ScoreMultiplier => 1;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var availableColumns = ((ManiaBeatmap)beatmap).TotalColumns;
            var shuffledColumns = Enumerable.Range(0, availableColumns).OrderBy(item => RNG.Next()).ToList();

            beatmap.HitObjects.OfType<ManiaHitObject>().ForEach(h => h.Column = shuffledColumns[h.Column]);
        }
    }
}
