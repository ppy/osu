// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.UI.Cursor;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModBloom : Mod, IApplicableToScoreProcessor, IUpdatableByPlayfield, IApplicableToPlayer
    {
        public override string Name => "Bloom";
        public override string Acronym => "BM";
        public override IconUsage? Icon => OsuIcon.ModBloom;
        public override ModType Type => ModType.Fun;
        public override LocalisableString Description => "The cursor blooms into.. a larger cursor!";
        public override double ScoreMultiplier => 1;
        protected const float MIN_SIZE = 1;
        protected const float TRANSITION_DURATION = 100;
        public override Type[] IncompatibleMods => new[] { typeof(OsuModFlashlight), typeof(OsuModNoScope), typeof(ModTouchDevice) };

        protected readonly BindableNumber<int> CurrentCombo = new BindableInt();
        protected readonly IBindable<bool> IsBreakTime = new Bindable<bool>();

        private float currentSize;

        [SettingSource(
            "Max size at combo",
            "The combo count at which the cursor reaches its maximum size",
            SettingControlType = typeof(SettingsSlider<int, RoundedSliderBar<int>>)
        )]
        public BindableInt MaxSizeComboCount { get; } = new BindableInt(50)
        {
            MinValue = 5,
            MaxValue = 100,
        };

        [SettingSource(
            "Final size multiplier",
            "The multiplier applied to cursor size when combo reaches maximum",
            SettingControlType = typeof(SettingsSlider<float, RoundedSliderBar<float>>)
        )]
        public BindableFloat MaxCursorSize { get; } = new BindableFloat(10f)
        {
            MinValue = 5f,
            MaxValue = 15f,
            Precision = 0.5f,
        };

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToPlayer(Player player)
        {
            IsBreakTime.BindTo(player.IsBreakTime);
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            CurrentCombo.BindTo(scoreProcessor.Combo);
            CurrentCombo.BindValueChanged(combo =>
            {
                currentSize = Math.Clamp(MaxCursorSize.Value * ((float)combo.NewValue / MaxSizeComboCount.Value), MIN_SIZE, MaxCursorSize.Value);
            }, true);
        }

        public void Update(Playfield playfield)
        {
            OsuCursor cursor = (OsuCursor)(playfield.Cursor!.ActiveCursor);

            if (IsBreakTime.Value)
                cursor.ModScaleAdjust.Value = 1;
            else
                cursor.ModScaleAdjust.Value = (float)Interpolation.Lerp(cursor.ModScaleAdjust.Value, currentSize, Math.Clamp(cursor.Time.Elapsed / TRANSITION_DURATION, 0, 1));
        }
    }
}
