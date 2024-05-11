// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.HUD;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class GameplayAccuracyCounter : PercentageCounter
    {
        [SettingSource(typeof(GameplayAccuracyCounterStrings), nameof(GameplayAccuracyCounterStrings.AccuracyDisplay), nameof(GameplayAccuracyCounterStrings.AccuracyDisplayDescription))]
        public Bindable<AccuracyDisplayMode> AccuracyDisplay { get; } = new Bindable<AccuracyDisplayMode>();

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AccuracyDisplay.BindValueChanged(mode =>
            {
                Current.UnbindBindings();

                switch (mode.NewValue)
                {
                    case AccuracyDisplayMode.Standard:
                        Current.BindTo(scoreProcessor.Accuracy);
                        break;

                    case AccuracyDisplayMode.MinimumAchievable:
                        Current.BindTo(scoreProcessor.MinimumAccuracy);
                        break;

                    case AccuracyDisplayMode.MaximumAchievable:
                        Current.BindTo(scoreProcessor.MaximumAccuracy);
                        break;
                }
            }, true);

            // if the accuracy counter is using the "minimum achievable" mode,
            // then its initial value is 0%, rather than the 100% that the base PercentageCounter assumes.
            // to counteract this, manually finish transforms on DisplayedCount once after the initial callback above
            // to stop it from rolling down from 100% to 0%.
            FinishTransforms(targetMember: nameof(DisplayedCount));
        }

        public enum AccuracyDisplayMode
        {
            [LocalisableDescription(typeof(GameplayAccuracyCounterStrings), nameof(GameplayAccuracyCounterStrings.AccuracyDisplayModeStandard))]
            Standard,

            [LocalisableDescription(typeof(GameplayAccuracyCounterStrings), nameof(GameplayAccuracyCounterStrings.AccuracyDisplayModeMax))]
            MaximumAchievable,

            [LocalisableDescription(typeof(GameplayAccuracyCounterStrings), nameof(GameplayAccuracyCounterStrings.AccuracyDisplayModeMin))]
            MinimumAchievable
        }
    }
}
