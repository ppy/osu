// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Graphics.UserInterface
{
    public abstract class StatefulMenuItem : OsuMenuItem
    {
        public readonly Bindable<object> State = new Bindable<object>();

        protected StatefulMenuItem(string text, Func<object, object> changeStateFunc, MenuItemType type = MenuItemType.Standard)
            : this(text, changeStateFunc, type, null)
        {
        }

        protected StatefulMenuItem(string text, Func<object, object> changeStateFunc, MenuItemType type, Action<object> action)
            : base(text, type)
        {
            Action.Value = () =>
            {
                State.Value = changeStateFunc?.Invoke(State.Value) ?? State.Value;
                action?.Invoke(State.Value);
            };
        }

        public abstract IconUsage? GetIconForState(object state);
    }

    public abstract class StatefulMenuItem<T> : StatefulMenuItem
        where T : struct
    {
        public new readonly Bindable<T> State = new Bindable<T>();

        protected StatefulMenuItem(string text, Func<T, T> changeStateFunc, MenuItemType type = MenuItemType.Standard)
            : this(text, changeStateFunc, type, null)
        {
        }

        protected StatefulMenuItem(string text, Func<T, T> changeStateFunc, MenuItemType type, Action<T> action)
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

        public abstract IconUsage? GetIconForState(T state);
    }
}
