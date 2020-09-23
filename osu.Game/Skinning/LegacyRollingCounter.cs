// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An integer <see cref="RollingCounter{T}"/> that uses number sprites from a legacy skin.
    /// </summary>
    public class LegacyRollingCounter : RollingCounter<int>
    {
        private readonly ISkin skin;

        private readonly string fontName;
        private readonly float fontOverlap;

        protected override bool IsRollingProportional => true;

        /// <summary>
        /// Creates a new <see cref="LegacyRollingCounter"/>.
        /// </summary>
        /// <param name="skin">The <see cref="ISkin"/> from which to get counter number sprites.</param>
        /// <param name="fontName">The name of the legacy font to use.</param>
        /// <param name="fontOverlap">
        /// The numeric overlap of number sprites to use.
        /// A positive number will bring the number sprites closer together, while a negative number
        /// will split them apart more.
        /// </param>
        public LegacyRollingCounter(ISkin skin, string fontName, float fontOverlap)
        {
            this.skin = skin;
            this.fontName = fontName;
            this.fontOverlap = fontOverlap;
        }

        protected override double GetProportionalDuration(int currentValue, int newValue)
        {
            return Math.Abs(newValue - currentValue) * 75.0;
        }

        protected sealed override OsuSpriteText CreateSpriteText() =>
            new LegacySpriteText(skin, fontName)
            {
                Spacing = new Vector2(-fontOverlap, 0f)
            };
    }
}
