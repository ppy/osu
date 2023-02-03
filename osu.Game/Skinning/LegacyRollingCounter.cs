// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Skinning
{
    /// <summary>
    /// An integer <see cref="RollingCounter{T}"/> that uses number sprites from a legacy skin.
    /// </summary>
    public partial class LegacyRollingCounter : RollingCounter<int>
    {
        private readonly LegacyFont font;

        protected override bool IsRollingProportional => true;

        /// <summary>
        /// Creates a new <see cref="LegacyRollingCounter"/>.
        /// </summary>
        /// <param name="font">The legacy font to use for the counter.</param>
        public LegacyRollingCounter(LegacyFont font)
        {
            this.font = font;
        }

        protected override double GetProportionalDuration(int currentValue, int newValue)
        {
            return Math.Abs(newValue - currentValue) * 75.0;
        }

        protected sealed override OsuSpriteText CreateSpriteText() => new LegacySpriteText(font);
    }
}
