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

            if (Randomizer.Value is RandomizationType.Notes or RandomizationType.Both)
            {
                var newObjects = new List<ManiaHitObject>();
                int lastRandomColumn = -1;
                double lastStartTime = -1;
                double lastEndTime = -1;
                var availableColumnsList = Enumerable.Range(0, availableColumns).ToList();
                var removedColumnsList = new List<int> {};

                foreach (var h in beatmap.HitObjects.OfType<ManiaHitObject>())
                {
                    double startTime = h.StartTime;

                    // Reset available columns if we're on a new time group
                    if (startTime != lastStartTime)
                    {
                        availableColumnsList = Enumerable.Range(0, availableColumns).ToList();
                    }

                    // Ensure we still have options
                    if (availableColumnsList.Count == 0)
                        continue; // Or handle differently (e.g., assign default column)

                    int randomIndex = rng.Next(availableColumnsList.Count);
                    int randomColumn = availableColumnsList[randomIndex];

                    h.Column = randomColumn;

                    // Remove the used column to avoid reuse for this time group
                    availableColumnsList.Remove(randomColumn);

                    lastRandomColumn = randomColumn;
                    lastStartTime = startTime;
                }

                maniaBeatmap.HitObjects = maniaBeatmap.HitObjects.Concat(newObjects).OrderBy(h => h.StartTime).ToList();
            }

            if (Randomizer.Value is RandomizationType.Columns or RandomizationType.Both)
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
