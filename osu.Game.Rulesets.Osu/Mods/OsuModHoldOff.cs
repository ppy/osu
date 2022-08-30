// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModHoldOff : Mod, IApplicableAfterBeatmapConversion
    {
        public override string Name => "Hold Off";

        public override string Acronym => "HO";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => @"Converts all sliders to streams.";

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModTarget), typeof(OsuModStrictTracking) };

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var osuBeatmap = (OsuBeatmap)beatmap;

            var newObjects = new List<OsuHitObject>();

            foreach (var hitObject in osuBeatmap.HitObjects)
            {
                if (hitObject is Slider s)
                {
                    var point = beatmap.ControlPointInfo.TimingPointAt(s.StartTime);
                    s.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
                    newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertSliderToStream(s, point, 4));
                }
                else
                {
                    newObjects.Add(hitObject);
                }
            }

            osuBeatmap.HitObjects = newObjects;
        }
    }
}
