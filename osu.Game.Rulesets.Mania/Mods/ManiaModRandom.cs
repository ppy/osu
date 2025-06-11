// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModRandom : ModRandom, IApplicableToBeatmap
    {
        public override LocalisableString Description => @"Shuffle around the keys!";

        [SettingSource("Randomization type")]
        public Bindable<RandomizationType> Randomizer { get; } = new Bindable<RandomizationType>();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            Seed.Value ??= RNG.Next();
            var rng = new Random((int)Seed.Value);
            var maniaBeatmap = (ManiaBeatmap)beatmap;
            int availableColumns = maniaBeatmap.TotalColumns;
            var shuffledColumns = Enumerable.Range(0, availableColumns).OrderBy(_ => rng.Next()).ToList();

            if (Randomizer.Value == RandomizationType.Columns)
            {
                beatmap.HitObjects.OfType<ManiaHitObject>().ForEach(h => h.Column = shuffledColumns[h.Column]);
            }

            if (Randomizer.Value == RandomizationType.Notes)
            {
                var newObjects = new List<ManiaHitObject>();

                foreach (var h in beatmap.HitObjects.OfType<ManiaHitObject>())
                {
                    h.Column = rng.Next(0, availableColumns);
                }

                maniaBeatmap.HitObjects = maniaBeatmap.HitObjects.Concat(newObjects).OrderBy(h => h.StartTime).ToList();
            }

            if (Randomizer.Value == RandomizationType.Both)
            {
                beatmap.HitObjects.OfType<ManiaHitObject>().ForEach(h => h.Column = shuffledColumns[h.Column]);
            }
        }

        public enum RandomizationType
        {
            Columns,
            Notes,
            Both
        }
    }
}
