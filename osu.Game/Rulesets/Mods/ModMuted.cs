// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.Mods;
using osu.Game.Overlays.Settings;
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
        public override LocalisableString Description => MutedModStrings.Description;
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
    }

    public abstract class ModMuted<TObject> : ModMuted, IApplicableToDrawableRuleset<TObject>, IApplicableToTrack, IApplicableToScoreProcessor
        where TObject : HitObject
    {
        private readonly BindableNumber<double> mainVolumeAdjust = new BindableDouble(0.5);
        private readonly BindableNumber<double> metronomeVolumeAdjust = new BindableDouble(0.5);

        private readonly BindableNumber<int> currentCombo = new BindableInt();

        [SettingSource(typeof(MutedModStrings), nameof(MutedModStrings.InverseMuting), nameof(MutedModStrings.InverseMutingDescription))]
        public BindableBool InverseMuting { get; } = new BindableBool();

        [SettingSource(typeof(MutedModStrings), nameof(MutedModStrings.EnableMetronome), nameof(MutedModStrings.EnableMetronomeDescription))]
        public BindableBool EnableMetronome { get; } = new BindableBool(true);

        [SettingSource(typeof(MutedModStrings), nameof(MutedModStrings.MuteComboCount), nameof(MutedModStrings.MuteComboCountDescription), SettingControlType = typeof(SettingsSlider<int, MuteComboSlider>))]
        public BindableInt MuteComboCount { get; } = new BindableInt(100)
        {
            MinValue = 0,
            MaxValue = 500,
        };

        [SettingSource(typeof(MutedModStrings), nameof(MutedModStrings.AffectsHitSounds), nameof(MutedModStrings.AffectsHitSoundsDescription))]
        public BindableBool AffectsHitSounds { get; } = new BindableBool(true);

        protected ModMuted()
        {
            InverseMuting.BindValueChanged(i => MuteComboCount.MinValue = i.NewValue ? 1 : 0, true);
        }

        public void ApplyToTrack(IAdjustableAudioComponent track)
        {
            track.AddAdjustment(AdjustableProperty.Volume, mainVolumeAdjust);
        }

        public void ApplyToDrawableRuleset(DrawableRuleset<TObject> drawableRuleset)
        {
            if (EnableMetronome.Value)
            {
                MetronomeBeat metronomeBeat;

                drawableRuleset.Overlays.Add(metronomeBeat = new MetronomeBeat(drawableRuleset.Beatmap.HitObjects.First().StartTime));

                metronomeBeat.AddAdjustment(AdjustableProperty.Volume, metronomeVolumeAdjust);
            }

            if (AffectsHitSounds.Value)
                drawableRuleset.Audio.AddAdjustment(AdjustableProperty.Volume, mainVolumeAdjust);
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            currentCombo.BindTo(scoreProcessor.Combo);
            currentCombo.BindValueChanged(combo =>
            {
                double dimFactor = MuteComboCount.Value == 0 ? 1 : (double)combo.NewValue / MuteComboCount.Value;

                if (InverseMuting.Value)
                    dimFactor = 1 - dimFactor;

                scoreProcessor.TransformBindableTo(metronomeVolumeAdjust, dimFactor, 500, Easing.OutQuint);
                scoreProcessor.TransformBindableTo(mainVolumeAdjust, 1 - dimFactor, 500, Easing.OutQuint);
            }, true);
        }

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;
    }

    public partial class MuteComboSlider : RoundedSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always muted" : base.TooltipText;
    }
}
