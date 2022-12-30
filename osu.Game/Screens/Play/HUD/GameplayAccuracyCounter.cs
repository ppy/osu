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
        [SettingSource("Accuracy Display Mode", "Which Accuracy will display")]
        public Bindable<AccuracyType> AccuracyDisplay { get; } = new Bindable<AccuracyType>();

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            AccuracyDisplay.BindValueChanged(mod =>
            {
                Current.UnbindBindings();

                switch (mod.NewValue)
                {
                    case AccuracyType.Rolling:
                        Current.BindTo(scoreProcessor.Accuracy);
                        break;

                    case AccuracyType.Increase:
                        Current.BindTo(scoreProcessor.IncreaseAccuracy);
                        break;

                    case AccuracyType.Decrease:
                        Current.BindTo(scoreProcessor.DecreaseAccuracy);
                        break;
                }
            }, true);
        }

        public enum AccuracyType
        {
            [Description("Rolling")]
            Rolling,

            [Description("Best achievable")]
            Increase,

            [Description("Worst achievable")]
            Decrease
        }
    }
}
