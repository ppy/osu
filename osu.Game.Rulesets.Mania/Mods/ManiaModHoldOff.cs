// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Beatmaps;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHoldOff : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Hold Off";

        public override string Acronym => "HO";

        public override double ScoreMultiplier => 1;

        public override string Description => @"Replaces all hold notes with normal notes.";

        public override IconUsage? Icon => FontAwesome.Solid.DotCircle;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(ManiaModInvert) };

        public const double END_NOTE_ALLOW_THRESHOLD = 0.5;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var newObjects = new List<ManiaHitObject>();

            foreach (var h in beatmap.HitObjects.OfType<HoldNote>())
            {
                // Add a note for the beginning of the hold note
                newObjects.Add(new Note
                {
                    Column = h.Column,
                    StartTime = h.StartTime,
                    Samples = h.GetNodeSamples(0)
                });

                // Don't add an end note if the duration is shorter than the threshold
                double noteValue = GetNoteDurationInBeatLength(h, maniaBeatmap); // 1/1, 1/2, 1/4, etc.

                if (noteValue >= END_NOTE_ALLOW_THRESHOLD)
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

        public static double GetNoteDurationInBeatLength(HoldNote holdNote, ManiaBeatmap beatmap)
        {
            double beatLength = beatmap.ControlPointInfo.TimingPointAt(holdNote.StartTime).BeatLength;
            return holdNote.Duration / beatLength;
        }
    }
}
