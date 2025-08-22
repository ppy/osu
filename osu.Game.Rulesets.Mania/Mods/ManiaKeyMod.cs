// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Mania.Mods
{
    public abstract class ManiaKeyMod : Mod, IApplicableToBeatmapConverter
    {
        public override string Acronym => Name;
        public abstract int KeyCount { get; }
        public override ModType Type => ModType.Conversion;
        
        /// <summary>
        /// Whether this key mod will actually modify the beatmap.
        /// If false (i.e., when applied to a mania-specific beatmap with the same key count), 
        /// no score penalty should be applied.
        /// </summary>
        private bool willModifyBeatmap = true;
        
        public override double ScoreMultiplier => willModifyBeatmap ? 0.9 : 1.0;
        public override bool Ranked => UsesDefaultConfiguration;

        public void ApplyToBeatmapConverter(IBeatmapConverter beatmapConverter)
        {
            var mbc = (ManiaBeatmapConverter)beatmapConverter;

            // Check if this key mod will actually modify the beatmap
            if (mbc.IsForCurrentRuleset)
            {
                // For mania-specific beatmaps, check if the key count matches
                // If it matches, this mod doesn't change anything, so no penalty should apply
                willModifyBeatmap = mbc.TargetColumns != KeyCount;
                
                // Don't apply keymod conversion to mania-specific beatmaps
                return;
            }

            // For non-mania beatmaps, this mod will always modify the beatmap
            willModifyBeatmap = true;
            mbc.TargetColumns = KeyCount;
        }

        public override Type[] IncompatibleMods => new[]
        {
            typeof(ManiaModKey1),
            typeof(ManiaModKey2),
            typeof(ManiaModKey3),
            typeof(ManiaModKey4),
            typeof(ManiaModKey5),
            typeof(ManiaModKey6),
            typeof(ManiaModKey7),
            typeof(ManiaModKey8),
            typeof(ManiaModKey9),
            typeof(ManiaModKey10),
        }.Except(new[] { GetType() }).ToArray();
    }
}
