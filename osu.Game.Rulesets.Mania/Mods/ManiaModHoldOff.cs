// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Framework.Bindables;
using osu.Game.Configuration;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHoldOff : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Hold Off";

        public override string Acronym => "HO";

        public override double ScoreMultiplier => 1;

        public override string Description => @"Turns all hold notes into normal notes. No coordination required.";

        public override IconUsage? Icon => FontAwesome.Solid.DotCircle;

        public override ModType Type => ModType.Conversion;

        [SettingSource("Add end notes", "Also add a note at the end of a hold note")]
        public BindableBool AddEndNotes { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Minimum end note beat snap", "Don't add end notes for hold notes shorter than this beat division")]
        public Bindable<BeatDivisors> MinBeatSnap { get; } = new Bindable<BeatDivisors>(defaultValue: BeatDivisors.Half);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var newObjects = new List<ManiaHitObject>();
            double beatSnap = 1 / (Math.Pow(2, (double)MinBeatSnap.Value));

            foreach (var h in beatmap.HitObjects.OfType<HoldNote>())
            {
                // Add a note for the beginning of the hold note
                newObjects.Add(new Note
                {
                    Column = h.Column,
                    StartTime = h.StartTime,
                    Samples = h.GetNodeSamples(0)
                });

                // Don't add an end note if the duration is shorter than some threshold, or end notes are disabled
                double noteValue = getNoteValue(h, maniaBeatmap); // 1/1, 1/2, 1/4, etc.

                if (AddEndNotes.Value && noteValue >= beatSnap)
                {
                    newObjects.Add(new Note
                    {
                        Column = h.Column,
                        StartTime = h.EndTime,
                        Samples = h.GetNodeSamples((h.NodeSamples?.Count - 1) ?? 1)
                    });
                }
            }

            maniaBeatmap.HitObjects = maniaBeatmap.HitObjects.OfType<Note>().Concat(newObjects).OrderBy(h => h.StartTime).ToList();
        }

        private static double getNoteValue(HoldNote holdNote, ManiaBeatmap beatmap)
        {
            double bpmAtNoteTime = beatmap.ControlPointInfo.TimingPointAt(holdNote.StartTime).BPM;
            return (60 * holdNote.Duration) / (1000 * bpmAtNoteTime);
        }

        public enum BeatDivisors
        {
            Whole,
            Half,
            Quarter,
            Eighth,
            Sixteenth
        }
    }
}
