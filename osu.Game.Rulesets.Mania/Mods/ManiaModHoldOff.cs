// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Mania.Beatmaps;

namespace osu.Game.Rulesets.Mania.Mods
{
    public class ManiaModHoldOff : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Hold Off";

        public override string Acronym => "HO";

        public override double ScoreMultiplier => 0.9;

        public override LocalisableString Description => @"Replaces all hold notes with normal notes.";

        public override IconUsage? Icon => FontAwesome.Solid.DotCircle;

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(ManiaModInvert) };

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
            }

            maniaBeatmap.HitObjects = maniaBeatmap.HitObjects.OfType<Note>().Concat(newObjects).OrderBy(h => h.StartTime).ToList();
        }
    }
}
