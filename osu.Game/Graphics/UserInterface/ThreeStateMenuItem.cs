// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An <see cref="OsuMenuItem"/> with three possible states.
    /// </summary>
    public class ThreeStateMenuItem : StatefulMenuItem<ThreeStates>
    {
        /// <summary>
        /// Creates a new <see cref="ThreeStateMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="type">The type of action which this <see cref="ThreeStateMenuItem"/> performs.</param>
        public ThreeStateMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ThreeStateMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="type">The type of action which this <see cref="ThreeStateMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="ThreeStateMenuItem"/> is pressed.</param>
        public ThreeStateMenuItem(string text, MenuItemType type, Action<ThreeStates> action)
            : this(text, getNextState, type, action)
        {
        }

        /// <summary>
        /// Creates a new <see cref="ThreeStateMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="changeStateFunc">A function that mutates a state to another state after this <see cref="ThreeStateMenuItem"/> is pressed.</param>
        /// <param name="type">The type of action which this <see cref="ThreeStateMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="ThreeStateMenuItem"/> is pressed.</param>
        protected ThreeStateMenuItem(string text, Func<ThreeStates, ThreeStates> changeStateFunc, MenuItemType type, Action<ThreeStates> action)
            : base(text, changeStateFunc, type, action)
        {
        }

        public override IconUsage? GetIconForState(ThreeStates state)
        {
            switch (state)
            {
                case ThreeStates.Indeterminate:
                    return FontAwesome.Regular.Circle;

                case ThreeStates.Enabled:
                    return FontAwesome.Solid.Check;
            }

            return null;
        }

        private static ThreeStates getNextState(ThreeStates state)
        {
            switch (state)
            {
                case ThreeStates.Disabled:
                    return ThreeStates.Enabled;

                case ThreeStates.Indeterminate:
                    return ThreeStates.Enabled;

                case ThreeStates.Enabled:
                    return ThreeStates.Disabled;

                default:
                    throw new ArgumentOutOfRangeException(nameof(state), state, null);
            }
        }
    }
}
