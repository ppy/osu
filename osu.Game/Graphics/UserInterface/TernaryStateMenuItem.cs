// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An <see cref="OsuMenuItem"/> with three possible states.
    /// </summary>
    public class TernaryStateMenuItem : StatefulMenuItem<TernaryState>
    {
        /// <summary>
        /// Creates a new <see cref="TernaryStateMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="type">The type of action which this <see cref="TernaryStateMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="TernaryStateMenuItem"/> is pressed.</param>
        public TernaryStateMenuItem(string text, MenuItemType type = MenuItemType.Standard, Action<TernaryState> action = null)
            : this(text, getNextState, type, action)
        {
        }

        /// <summary>
        /// Creates a new <see cref="TernaryStateMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="changeStateFunc">A function that mutates a state to another state after this <see cref="TernaryStateMenuItem"/> is pressed.</param>
        /// <param name="type">The type of action which this <see cref="TernaryStateMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="TernaryStateMenuItem"/> is pressed.</param>
        protected TernaryStateMenuItem(string text, Func<TernaryState, TernaryState> changeStateFunc, MenuItemType type, Action<TernaryState> action)
            : base(text, changeStateFunc, type, action)
        {
        }

        public override IconUsage? GetIconForState(TernaryState state)
        {
            switch (state)
            {
                case TernaryState.Indeterminate:
                    return FontAwesome.Solid.DotCircle;

                case TernaryState.True:
                    return FontAwesome.Solid.Check;
            }

            return null;
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
