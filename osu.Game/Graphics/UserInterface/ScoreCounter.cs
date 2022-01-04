// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class ScoreCounter : RollingCounter<double>
    {
        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;

        public Bindable<int> RequiredDisplayDigits { get; } = new Bindable<int>();

        private string formatString;

        /// <summary>
        /// Displays score.
        /// </summary>
        /// <param name="leading">How many leading zeroes the counter will have.</param>
        protected ScoreCounter(int leading = 0)
        {
            RequiredDisplayDigits.Value = leading;
            RequiredDisplayDigits.BindValueChanged(displayDigitsChanged, true);
        }

        private void displayDigitsChanged(ValueChangedEvent<int> _)
        {
            formatString = new string('0', RequiredDisplayDigits.Value);
            UpdateDisplay();
        }

        protected override double GetProportionalDuration(double currentValue, double newValue) =>
            currentValue > newValue ? currentValue - newValue : newValue - currentValue;

        protected override LocalisableString FormatCount(double count) => ((long)count).ToLocalisableString(formatString);

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(fixedWidth: true));
    }
}
