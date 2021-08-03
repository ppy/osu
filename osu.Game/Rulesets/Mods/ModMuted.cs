// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMuted : Mod
    {
        public override string Name => "Muted";
        public override string Acronym => "MU";
        public override IconUsage? Icon => FontAwesome.Solid.VolumeMute;
        public override string Description => "Can you still feel the rhythm without music?";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
    }

    public abstract class ModMuted<TObject> : ModMuted, IApplicableToDrawableRuleset<TObject>, IApplicableToTrack, IApplicableToScoreProcessor
        where TObject : HitObject
    {
        private readonly BindableNumber<double> mainVolumeAdjust = new BindableDouble(0.5);
        private readonly BindableNumber<double> metronomeVolumeAdjust = new BindableDouble(0.5);

        private BindableNumber<int> currentCombo;

        [SettingSource("Enable metronome", "Add a metronome beat to help you keep track of the rhythm.")]
        public BindableBool EnableMetronome { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        [SettingSource("Final volume at combo", "The combo count at which point the track reaches its final volume.")]
        public BindableInt MuteComboCount { get; } = new BindableInt
        {
            Default = 100,
            Value = 100,
            MinValue = 0,
            MaxValue = 500,
        };

        [SettingSource("Start muted", "Increase volume as combo builds.")]
        public BindableBool InverseMuting { get; } = new BindableBool
        {
            Default = false,
            Value = false
        };

        [SettingSource("Mute hit sounds", "Hit sounds are also muted alongside the track.")]
        public BindableBool AffectsHitSounds { get; } = new BindableBool
        {
            Default = true,
            Value = true
        };

        public void ApplyToTrack(ITrack track)
        {
            track.AddAdjustment(AdjustableProperty.Volume, mainVolumeAdjust);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            if (EnableMetronome.Value)
            {
                Metronome metronome;

                drawableRuleset.Overlays.Add(metronome = new Metronome(drawableRuleset.Beatmap.HitObjects.First().StartTime));

                metronome.AddAdjustment(AdjustableProperty.Volume, metronomeVolumeAdjust);
            }

            if (AffectsHitSounds.Value)
                drawableRuleset.Audio.AddAdjustment(AdjustableProperty.Volume, mainVolumeAdjust);
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            currentCombo = scoreProcessor.Combo.GetBoundCopy();
            currentCombo.BindValueChanged(combo =>
            {
                double dimFactor = Math.Min(1, (double)combo.NewValue / MuteComboCount.Value);

                if (InverseMuting.Value)
                    dimFactor = 1 - dimFactor;

                scoreProcessor.TransformBindableTo(metronomeVolumeAdjust, dimFactor, 500, Easing.OutQuint);
                scoreProcessor.TransformBindableTo(mainVolumeAdjust, 1 - dimFactor, 500, Easing.OutQuint);
            }, true);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }
}
