// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD
{
    public abstract partial class GameplayAccuracyCounter : PercentageCounter
    {
        [SettingSource("Accuracy display mode", "Which accuracy mode should be displayed.")]
        public Bindable<AccuracyDisplayMode> AccuracyDisplay { get; } = new Bindable<AccuracyDisplayMode>();

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            AccuracyDisplay.BindValueChanged(mod =>
            {
                Current.UnbindBindings();

                switch (mod.NewValue)
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
        }

        public enum AccuracyDisplayMode
        {
            [Description("Standard")]
            Standard,

            [Description("Maximum achievable")]
            MaximumAchievable,

            [Description("Minimum achievable")]
            MinimumAchievable
        }
    }
}
