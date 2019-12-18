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
        public TernaryStateMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="TernaryStateMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="type">The type of action which this <see cref="TernaryStateMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="TernaryStateMenuItem"/> is pressed.</param>
        public TernaryStateMenuItem(string text, MenuItemType type, Action<TernaryState> action)
            : this(text,
                state => state switch
                {
                    TernaryState.False => TernaryState.True,
                    TernaryState.Indeterminate => TernaryState.True,
                    TernaryState.True => TernaryState.False,
                    _ => throw new ArgumentOutOfRangeException(nameof(state), state, null),
                }, type, action)
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
            => state switch
            {
                TernaryState.Indeterminate => FontAwesome.Solid.DotCircle,
                TernaryState.True => FontAwesome.Solid.Check,
                _ => null,
            };
    }
}
