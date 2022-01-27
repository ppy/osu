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
    public class ManiaModNoHolds : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "No Holds";

        public override string Acronym => "NH";

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

                // Don't add an end note if the duration is shorter than some threshold, or end notes are disabled
                if (AddEndNotes.Value && h.Duration > 200)
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
    }
}