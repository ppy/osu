// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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

        public override LocalisableString Description => "Hold the keys. To the beat.";

        public override IconUsage? Icon => FontAwesome.Solid.YinYang;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(ManiaModHoldOff) };

        [SettingSource("Invert Long Notes", "Invert long notes into nothing.")]
        public BindableBool FullInvert { get; } = new BindableBool();

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var newObjects = new List<ManiaHitObject>();

            foreach (var column in maniaBeatmap.HitObjects.GroupBy(h => h.Column))
            {
                var newColumnObjects = new List<ManiaHitObject>();

                List<(double startTime, IList<HitSampleInfo> samples, string type)> locations;

                if (FullInvert.Value)
                    locations = column.OfType<Note>().Select(n => (startTime: n.StartTime, samples: n.Samples, type: "note"))
                        .Concat(column.OfType<HoldNote>().SelectMany(h => new[]
                        {
                            (startTime: h.StartTime, samples: h.GetNodeSamples(0), type: "release"),
                            (startTime: h.EndTime, samples: h.GetNodeSamples(1), type: "note")
                        }))
                        .OrderBy(h => h.startTime).ToList();
                else
                    locations = column.Select(n => (startTime: n.StartTime, samples: n.Samples, type: "note"))
                        .OrderBy(h => h.startTime).ToList();

                for (int i = 0; i < locations.Count - 1; i++)
                {
                    if (locations[i].type == "release")
                        continue;

                    // Beat length at the end of the hold note.
                    double beatLength = beatmap.ControlPointInfo.TimingPointAt(locations[i + 1].startTime).BeatLength;

                    // Full duration of the hold note.
                    double duration = locations[i + 1].startTime - locations[i].startTime;

                    if (locations[i + 1].type != "release")
                        // Decrease the duration by at most a 1/4 beat to ensure there's no instantaneous notes.
                        duration = Math.Max(duration / 2, duration - beatLength / 4);

                    newColumnObjects.Add(new HoldNote
                    {
                        Column = column.Key,
                        StartTime = locations[i].startTime,
                        Duration = duration,
                        NodeSamples = new List<IList<HitSampleInfo>> { locations[i].samples, Array.Empty<HitSampleInfo>() }
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
