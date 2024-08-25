// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Mods;
using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHoldOff : ModHoldOff, IApplicableAfterBeatmapConversion
    {
        public override double ScoreMultiplier => 0.7;

        public override LocalisableString Description => @"Removes sliderbodies, transforming sliders into circles.";

        public override Type[] IncompatibleMods => new[] { typeof(OsuModStrictTracking), typeof(OsuModTargetPractice) };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var osuBeatmap = (OsuBeatmap)beatmap;

            var newObjects = new List<OsuHitObject>();

            foreach (var s in beatmap.HitObjects.OfType<Slider>())
            {
                newObjects.Add(new HitCircle
                {
                    StartTime = s.StartTime,
                    Position = s.Position,
                    NewCombo = s.NewCombo,
                    Samples = s.GetNodeSamples(0)
                });
            }

            osuBeatmap.HitObjects = osuBeatmap.HitObjects.Where(o => o is not Slider).Concat(newObjects).OrderBy(h => h.StartTime).ToList();
        }
    }
}
