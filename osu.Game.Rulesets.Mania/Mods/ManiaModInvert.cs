// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModInvert : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Invert";

        public override string Acronym => "IN";
        public override double ScoreMultiplier => 1;

        public override string Description => "Hold the keys. To the beat.";

        public override ModType Type => ModType.Conversion;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var newObjects = new List<ManiaHitObject>();

            foreach (var column in maniaBeatmap.HitObjects.GroupBy(h => h.Column))
            {
                var newColumnObjects = new List<ManiaHitObject>();

                var locations = column.OfType<Note>().Select(n => (startTime: n.StartTime, samples: n.Samples))
                                      .Concat(column.OfType<HoldNote>().SelectMany(h => new[]
                                      {
                                          (startTime: h.StartTime, samples: h.GetNodeSamples(0)),
                                          (startTime: h.EndTime, samples: h.GetNodeSamples(1))
                                      }))
                                      .OrderBy(h => h.startTime).ToList();

                for (int i = 0; i < locations.Count - 1; i += 2)
                {
                    newColumnObjects.Add(new HoldNote
                    {
                        Column = column.Key,
                        StartTime = locations[i].startTime,
                        Duration = locations[i + 1].startTime - locations[i].startTime,
                        Samples = locations[i].samples,
                        NodeSamples = new List<IList<HitSampleInfo>>
                        {
                            locations[i].samples,
                            locations[i + 1].samples
                        }
                    });
                }

                newObjects.AddRange(newColumnObjects);
            }

            maniaBeatmap.HitObjects = newObjects.OrderBy(h => h.StartTime).ToList();

            // No breaks
            maniaBeatmap.Breaks.Clear();
        }
    }
}
