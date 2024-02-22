// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModWhiplash : Mod, IApplicableToHitObject, IApplicableToBeatmapProcessor
    {
        public override string Name => "Whiplash";
        public override string Acronym => "WL";
        public override LocalisableString Description => "Am I rushing.. or am I dragging?";
        public override ModType Type => ModType.Conversion;

        private IBeatmap? currentBeatmap { get; set; }

        [SettingSource("Target BPM", "Target BPM to center the changes around.")]
        public Bindable<TargetBPMStyle> TargetBPM { get; } = new Bindable<TargetBPMStyle>(TargetBPMStyle.MaxBPM);

        public override double ScoreMultiplier => TargetBPM.Value == TargetBPMStyle.MaxBPM ? 1.0 : 0.75;

        public void ApplyToHitObject(HitObject hitObject)
        {
            if (currentBeatmap == null || hitObject is not OsuHitObject hitCircle)
                return;

            double currentBpm = currentBeatmap.ControlPointInfo.TimingPointAt(hitCircle.StartTime).BPM;

            if (currentBpm < 1)
            {
                updateTimePreempty(hitCircle, hitCircle.StartTime);
                return;
            }

            double scaleBpm = getBaseBPM(currentBeatmap);
            double preempt = hitCircle.TimePreempt;
            double newTimePreempt = (scaleBpm / Math.Max(currentBpm, 1)) * preempt;

            //don't allow AR to go over 11
            updateTimePreempty(hitCircle, Math.Max(newTimePreempt, 300));
        }

        private void updateTimePreempty(OsuHitObject hitCircle, double value)
        {
            hitCircle.TimePreempt = value;

            if (hitCircle is Slider slider)
            {
                slider.HeadCircle.TimePreempt = value;
            }
        }

        private double getBaseBPM(IBeatmap beatmap)
        {
            switch (TargetBPM.Value)
            {
                case TargetBPMStyle.MainBPM:
                    return beatmap.BeatmapInfo.BPM;

                case TargetBPMStyle.MaxBPM:
                    return beatmap.ControlPointInfo.BPMMaximum;

                case TargetBPMStyle.MinBPM:
                    return beatmap.ControlPointInfo.BPMMinimum;

                default:
                    throw new ArgumentOutOfRangeException(nameof(TargetBPM), TargetBPM, @"Unsupported BPM scale");
            }
        }

        public void ApplyToBeatmapProcessor(IBeatmapProcessor beatmapProcessor)
        {
            if (currentBeatmap != beatmapProcessor.Beatmap)
            {
                currentBeatmap = beatmapProcessor.Beatmap;
            }
        }

        public enum TargetBPMStyle
        {
            MainBPM,
            MinBPM,
            MaxBPM,
        }
    }
}
