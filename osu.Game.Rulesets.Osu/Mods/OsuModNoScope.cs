// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModNoScope : Mod, IUpdatableByPlayfield, IApplicableToScoreProcessor, IApplicableToPlayer, IApplicableToBeatmap
    {
        /// <summary>
        /// Slightly higher than the cutoff for <see cref="Drawable.IsPresent"/>.
        /// </summary>
        private const float min_alpha = 0.0002f;

        private const float transition_duration = 100;

        public override string Name => "No Scope";
        public override string Acronym => "NS";
        public override ModType Type => ModType.Fun;
        public override IconUsage? Icon => FontAwesome.Solid.EyeSlash;
        public override string Description => "Where's the cursor?";
        public override double ScoreMultiplier => 1;

        private BindableNumber<int> currentCombo;
        private IBindable<bool> isBreakTime;
        private PeriodTracker spinnerPeriods;

        private float comboBasedAlpha;

        [SettingSource(
            "Hidden at combo",
            "The combo count at which the cursor becomes completely hidden",
            SettingControlType = typeof(SettingsSlider<int, HiddenComboSlider>)
        )]
        public BindableInt HiddenComboCount { get; } = new BindableInt
        {
            Default = 10,
            Value = 10,
            MinValue = 0,
            MaxValue = 50,
        };

        public ScoreRank AdjustRank(ScoreRank rank, double accuracy) => rank;

        public void ApplyToPlayer(Player player)
        {
            isBreakTime = player.IsBreakTime.GetBoundCopy();
        }

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            spinnerPeriods = new PeriodTracker(beatmap.HitObjects.OfType<Spinner>().Select(b => new Period(b.StartTime - transition_duration, b.EndTime)));
        }

        public void ApplyToScoreProcessor(ScoreProcessor scoreProcessor)
        {
            if (HiddenComboCount.Value == 0) return;

            currentCombo = scoreProcessor.Combo.GetBoundCopy();
            currentCombo.BindValueChanged(combo =>
            {
                comboBasedAlpha = Math.Max(min_alpha, 1 - (float)combo.NewValue / HiddenComboCount.Value);
            }, true);
        }

        public virtual void Update(Playfield playfield)
        {
            bool shouldAlwaysShowCursor = isBreakTime.Value || spinnerPeriods.IsInAny(playfield.Clock.CurrentTime);
            float targetAlpha = shouldAlwaysShowCursor ? 1 : comboBasedAlpha;
            playfield.Cursor.Alpha = (float)Interpolation.Lerp(playfield.Cursor.Alpha, targetAlpha, Math.Clamp(playfield.Time.Elapsed / transition_duration, 0, 1));
        }
    }

    public class HiddenComboSlider : OsuSliderBar<int>
    {
        public override LocalisableString TooltipText => Current.Value == 0 ? "always hidden" : base.TooltipText;
    }
}
