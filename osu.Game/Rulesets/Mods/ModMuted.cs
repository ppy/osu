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
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public abstract class ModMuted : Mod
    {
        public override string Name => "静音";
        public override string Acronym => "MU";
        public override IconUsage? Icon => FontAwesome.Solid.VolumeMute;
        public override LocalisableString Description => "你还能感受到歌曲的节奏吗？";
        public override ModType Type => ModType.Fun;
        public override double ScoreMultiplier => 1;
    }

    public abstract class ModMuted<TObject> : ModMuted, IApplicableToDrawableRuleset<TObject>, IApplicableToTrack, IApplicableToScoreProcessor
        where TObject : HitObject
    {
        private readonly BindableNumber<double> mainVolumeAdjust = new BindableDouble(0.5);
        private readonly BindableNumber<double> metronomeVolumeAdjust = new BindableDouble(0.5);

        private readonly BindableNumber<int> currentCombo = new BindableInt();

        [SettingSource("以静音开始", "随连击增加音量")]
        public BindableBool InverseMuting { get; } = new BindableBool();

        [SettingSource("启用节拍器", "添加节拍器来帮助你跟住歌曲的节奏。")]
        public BindableBool EnableMetronome { get; } = new BindableBool(true);

        [SettingSource("抵达最大音量的连击", "抵达最大音量时的连击数", SettingControlType = typeof(SettingsSlider<int, MuteComboSlider>))]
        public BindableInt MuteComboCount { get; } = new BindableInt(100)
        {
            MinValue = 0,
            MaxValue = 500,
        };

        [SettingSource("静音音效", "音效也会跟着音频静音。")]
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

                // Importantly, this is added to FrameStableComponents and not Overlays as the latter would cause it to be self-muted by the mod's volume adjustment.
                drawableRuleset.FrameStableComponents.Add(metronomeBeat = new MetronomeBeat(drawableRuleset.Beatmap.HitObjects.First().StartTime));

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
        public override LocalisableString TooltipText => Current.Value == 0 ? "总是静音" : base.TooltipText;
    }
}
