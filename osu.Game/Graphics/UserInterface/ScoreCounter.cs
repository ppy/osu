// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class ScoreCounter : RollingCounter<double>, IScoreCounter
    {
        protected override double RollingDuration => 1000;
        protected override Easing RollingEasing => Easing.Out;

        /// <summary>
        /// Whether comma separators should be displayed.
        /// </summary>
        public bool UseCommaSeparator { get; }

        /// <summary>
        /// How many leading zeroes the counter has.
        /// </summary>
        public uint LeadingZeroes { get; }

        /// <summary>
        /// Displays score.
        /// </summary>
        /// <param name="leading">How many leading zeroes the counter will have.</param>
        /// <param name="useCommaSeparator">Whether comma separators should be displayed.</param>
        protected ScoreCounter(uint leading = 0, bool useCommaSeparator = false)
        {
            UseCommaSeparator = useCommaSeparator;
            LeadingZeroes = leading;
        }

        protected override double GetProportionalDuration(double currentValue, double newValue)
        {
            return currentValue > newValue ? currentValue - newValue : newValue - currentValue;
        }

        protected override string FormatCount(double count)
        {
            string format = new string('0', (int)LeadingZeroes);

            if (UseCommaSeparator)
            {
                for (int i = format.Length - 3; i > 0; i -= 3)
                    format = format.Insert(i, @",");
            }

            return ((long)count).ToString(format);
        }

        protected override OsuSpriteText CreateSpriteText()
            => base.CreateSpriteText().With(s => s.Font = s.Font.With(fixedWidth: true));
    }
}
