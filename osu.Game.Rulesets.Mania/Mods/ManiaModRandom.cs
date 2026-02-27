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
using osu.Game.Rulesets.Objects;

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

            switch (Randomizer.Value)
            {
                case RandomizationType.Notes:
                {
                    double[] columnEndTimes = new double[availableColumns];
                    double? lastStartTime = null;
                    var availableColumnsList = new List<int>();

                    const double release_buffer = 1.5; // Minimum gap to avoid conflict at end of HoldNote

                    foreach (var h in beatmap.HitObjects.OfType<ManiaHitObject>())
                    {
                        double currentStartTime = h.StartTime;

                        if (currentStartTime != lastStartTime)
                        {
                            availableColumnsList = Enumerable.Range(0, availableColumns)
                                                             .Where(i => columnEndTimes[i] < currentStartTime - release_buffer)
                                                             .ToList();
                        }

                        if (availableColumnsList.Count == 0)
                            continue; // Skip if no free columns for aspire maps

                        int randomIndex = rng.Next(availableColumnsList.Count);
                        int randomColumn = availableColumnsList[randomIndex];

                        h.Column = randomColumn;
                        availableColumnsList.Remove(randomColumn);

                        if (h is HoldNote hold)
                            columnEndTimes[randomColumn] = hold.GetEndTime();

                        lastStartTime = currentStartTime;
                    }

                    maniaBeatmap.HitObjects = maniaBeatmap.HitObjects.OrderBy(h => h.StartTime).ToList();
                    break;
                }

                case RandomizationType.Columns:
                    beatmap.HitObjects.OfType<ManiaHitObject>().ForEach(h => h.Column = shuffledColumns[h.Column]);
                    break;
            }
        }

        public enum RandomizationType
        {
            Columns,
            Notes
        }
    }
}
