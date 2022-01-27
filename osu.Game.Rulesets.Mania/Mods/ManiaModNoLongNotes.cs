// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using osu.Game.Audio;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Framework.Utils;
using osu.Game.Overlays.Settings;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Graphics;



namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModNoLongNotes : Mod, IApplicableAfterBeatmapConversion
    {

        public override string Name => "No Long Notes";

        public override string Acronym => "NL";
        
        public override double ScoreMultiplier => 1;
        
        public override string Description => @"Turns all held notes into tap notes. No coordination required.";
        
        public override IconUsage? Icon => FontAwesome.Solid.DotCircle;
        
        public override ModType Type => ModType.Conversion;

        [SettingSource("Add end notes", "Also add a note at the end of a held note")]
        public BindableBool AddEndNotes { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Length threshold", "Only add an end note for held notes longer than this threshold (in milliseconds)")]
        public BindableNumber<double> Threshold { get; } = new BindableDouble
        {
            MinValue = 1.0,
            MaxValue = 1990.0,
            Default = 200.0,
            Value = 200.0,
            Precision = 1.0,
        };
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var maniaBeatmap = (ManiaBeatmap)beatmap;

            var newObjects = new List<ManiaHitObject>();
            beatmap.HitObjects.OfType<HoldNote>().ForEach(h => 
            {
                // Add a note for the beginning of the hold note
                newObjects.Add(new Note
                {
                    Column = h.Column,
                    StartTime = h.StartTime,
                    Samples = h.Samples
                });

                // Don't add an end note if the duration is below the threshold, or end notes are disabled
                if (AddEndNotes.Value && h.Duration > Threshold.Value) 
                {
                    newObjects.Add(new Note
                    {
                        Column = h.Column,
                        StartTime = h.EndTime,
                        Samples = h.Samples
                    });
                }
            });

            maniaBeatmap.HitObjects = maniaBeatmap.HitObjects.OfType<Note>().Concat(newObjects).OrderBy(h => h.StartTime).ToList();            
        }
    }
}
