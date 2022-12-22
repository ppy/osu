// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoSliding : Mod, IApplicableAfterBeatmapConversion
    {
        private const int deathstream_length = 16;

        public override string Name => "No Sliding";

        public override string Acronym => "NL";

        public override double ScoreMultiplier => 1;

        public override LocalisableString Description => @"Convert all sliders to streams.";

        public override ModType Type => ModType.Conversion;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModTarget), typeof(OsuModStrictTracking) };

        [SettingSource("Beat divisor")]
        public BindableInt BeatDivisor { get; } = new BindableInt(4)
        {
            MinValue = 1,
            MaxValue = 8
        };

        [SettingSource("Slow down too long streams")]
        public BindableBool DeathstreamsSlowingDown { get; } = new BindableBool(true);

        [SettingSource("Slow down too fast parts")]
        public BindableBool FastPartsSlowingDown { get; } = new BindableBool(true);

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            var osuBeatmap = (OsuBeatmap)beatmap;
            var newObjects = new List<OsuHitObject>();

            var points = beatmap.ControlPointInfo.TimingPoints.OrderByDescending(c => c.BeatLength);
            double minAllowedBeatLength = (points.FirstOrDefault() ?? TimingControlPoint.DEFAULT).BeatLength / 1.5d;

            foreach (var hitObject in osuBeatmap.HitObjects)
            {
                if (hitObject is not Slider s)
                {
                    newObjects.Add(hitObject);
                    continue;
                }

                var point = beatmap.ControlPointInfo.TimingPointAt(s.StartTime);
                s.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);

                if (FastPartsSlowingDown.Value && point.BeatLength < minAllowedBeatLength)
                {
                    double beatLength = point.BeatLength;
                    while (beatLength < minAllowedBeatLength)
                        beatLength *= 2;

                    point = (TimingControlPoint)point.DeepClone();
                    point.BeatLength = beatLength;
                }

                // this will be true if span is going to be converted into 1, 2, or 3 circles.
                bool spanTooShort = s.SpanDuration <= point.BeatLength * 4d / BeatDivisor.Value;
                // an attempt to fix too fast turns. I chose 250ms by eye, but if it is still going to be a problem this can be calculated based on AR.
                bool spanTooFast = s.SpanDuration <= 250d;

                if ((spanTooShort || spanTooFast) && s.RepeatCount > 0)
                {
                    newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertKickSliderToBurst(s, point, BeatDivisor.Value));
                }
                else
                {
                    double divisor = BeatDivisor.Value;
                    // dur/BL*div is always lower than actual note count by 1, so using >=, not >.
                    if (s.Duration / point.BeatLength * divisor >= deathstream_length && DeathstreamsSlowingDown.Value)
                        divisor /= 2d; // making stream slower twice, if it's longer than the limit.
                    newObjects.AddRange(OsuHitObjectGenerationUtils.ConvertSliderToStream(s, point, divisor));
                }
            }

            // guard from overlapping sliders

            newObjects.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));

            for (int i = newObjects.Count - 1; i > 0; i--)
            {
                // filtering out circles closer than 1ms
                if (Precision.AlmostEquals(newObjects[i].StartTime, newObjects[i - 1].StartTime, 1d))
                    newObjects.RemoveAt(i);
            }

            osuBeatmap.HitObjects = newObjects;
        }
    }
}
