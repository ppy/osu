// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// A ternary state menu item which toggles the state of this item <c>false</c> if clicked when <c>true</c>.
    /// </summary>
    public class TernaryStateToggleMenuItem : TernaryStateMenuItem
    {
        /// <summary>
        /// Creates a new <see cref="TernaryStateToggleMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="type">The type of action which this <see cref="TernaryStateMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="TernaryStateMenuItem"/> is pressed.</param>
        public TernaryStateToggleMenuItem(string text, MenuItemType type = MenuItemType.Standard, Action<TernaryState> action = null)
            : base(text, getNextState, type, action)
        {
        }

        private static TernaryState getNextState(TernaryState state)
        {
            switch (state)
            {
                case TernaryState.False:
                    return TernaryState.True;

                case TernaryState.Indeterminate:
                    return TernaryState.True;

                case TernaryState.True:
                    return TernaryState.False;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
