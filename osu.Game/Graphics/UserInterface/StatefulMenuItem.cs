// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;

namespace osu.Game.Graphics.UserInterface
{
    /// <summary>
    /// An <see cref="OsuMenuItem"/> which contains and displays a state.
    /// </summary>
    public abstract class StatefulMenuItem : OsuMenuItem
    {
        /// <summary>
        /// The current state that should be displayed.
        /// </summary>
        public readonly Bindable<object> State = new Bindable<object>();

        /// <summary>
        /// Creates a new <see cref="StatefulMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="changeStateFunc">A function that mutates a state to another state after this <see cref="StatefulMenuItem"/> is pressed.</param>
        /// <param name="type">The type of action which this <see cref="StatefulMenuItem"/> performs.</param>
        protected StatefulMenuItem(LocalisableString text, Func<object, object> changeStateFunc, MenuItemType type = MenuItemType.Standard)
            : this(text, changeStateFunc, type, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="StatefulMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="changeStateFunc">A function that mutates a state to another state after this <see cref="StatefulMenuItem"/> is pressed.</param>
        /// <param name="type">The type of action which this <see cref="StatefulMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="StatefulMenuItem"/> is pressed.</param>
        protected StatefulMenuItem(LocalisableString text, Func<object, object>? changeStateFunc, MenuItemType type, Action<object>? action)
            : base(text, type)
        {
            Action.Value = () =>
            {
                State.Value = changeStateFunc?.Invoke(State.Value) ?? State.Value;
                action?.Invoke(State.Value);
            };
        }

        /// <summary>
        /// Retrieves the icon to be displayed for a state.
        /// </summary>
        /// <param name="state">The state to retrieve the relevant icon for.</param>
        /// <returns>The icon to be displayed for <paramref name="state"/>.</returns>
        public abstract IconUsage? GetIconForState(object state);
    }

    public abstract class StatefulMenuItem<T> : StatefulMenuItem
        where T : struct
    {
        /// <summary>
        /// The current state that should be displayed.
        /// </summary>
        public new readonly Bindable<T> State = new Bindable<T>();

        /// <summary>
        /// Creates a new <see cref="StatefulMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="changeStateFunc">A function that mutates a state to another state after this <see cref="StatefulMenuItem"/> is pressed.</param>
        /// <param name="type">The type of action which this <see cref="StatefulMenuItem"/> performs.</param>
        protected StatefulMenuItem(LocalisableString text, Func<T, T>? changeStateFunc, MenuItemType type = MenuItemType.Standard)
            : this(text, changeStateFunc, type, null)
        {
        }

        /// <summary>
        /// Creates a new <see cref="StatefulMenuItem"/>.
        /// </summary>
        /// <param name="text">The text to display.</param>
        /// <param name="changeStateFunc">A function that mutates a state to another state after this <see cref="StatefulMenuItem"/> is pressed.</param>
        /// <param name="type">The type of action which this <see cref="StatefulMenuItem"/> performs.</param>
        /// <param name="action">A delegate to be invoked when this <see cref="StatefulMenuItem"/> is pressed.</param>
        protected StatefulMenuItem(LocalisableString text, Func<T, T>? changeStateFunc, MenuItemType type, Action<T>? action)
            : base(text, o => changeStateFunc?.Invoke((T)o) ?? o, type, o => action?.Invoke((T)o))
        {
            base.State.BindValueChanged(state =>
            {
                if (state.NewValue == null)
                    base.State.Value = default(T);

                State.Value = (T)base.State.Value;
            }, true);

            State.BindValueChanged(state => base.State.Value = state.NewValue);
        }

        public sealed override IconUsage? GetIconForState(object state) => GetIconForState((T)state);

        /// <summary>
        /// Retrieves the icon to be displayed for a state.
        /// </summary>
        /// <param name="state">The state to retrieve the relevant icon for.</param>
        /// <returns>The icon to be displayed for <paramref name="state"/>.</returns>
        public abstract IconUsage? GetIconForState(T state);
    }
}
