// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        private string formatString = string.Empty;

        /// <summary>
        /// Displays score.
        /// </summary>
        /// <param name="leading">How many leading zeroes the counter will have.</param>
        /// <param name="useCommaSeparator">Whether comma separators should be displayed.</param>
        protected ScoreCounter(int leading = 0, bool useCommaSeparator = false)
        {
            if (useCommaSeparator)
            {
                if (leading > 0)
                    throw new ArgumentException("Should not mix leading zeroes and comma separators as it doesn't make sense");

                formatString = @"N0";
            }

            RequiredDisplayDigits.Value = leading;
            RequiredDisplayDigits.BindValueChanged(displayDigitsChanged, true);
        }

        private void displayDigitsChanged(ValueChangedEvent<int> _)
        {
            formatString = new string('0', RequiredDisplayDigits.Value);
            UpdateDisplay();
        }

        protected override double GetProportionalDuration(double currentValue, double newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override LocalisableString FormatCount(double count) => ((long)count).ToLocalisableString(formatString);

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(fixedWidth: true));
    }
}
