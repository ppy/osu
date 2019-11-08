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

        protected StatefulMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        protected StatefulMenuItem(string text, MenuItemType type, Func<object, object> changeStateFunc)
            : base(text, type)
        {
            Action.Value = () => State.Value = changeStateFunc?.Invoke(State.Value) ?? State.Value;
        }

        public abstract IconUsage? GetIconForState(object state);
    }

    public abstract class StatefulMenuItem<T> : StatefulMenuItem
        where T : struct
    {
        public new readonly Bindable<T> State = new Bindable<T>();

        protected StatefulMenuItem(string text, MenuItemType type = MenuItemType.Standard)
            : this(text, type, null)
        {
        }

        protected StatefulMenuItem(string text, MenuItemType type, Func<T, T> changeStateFunc)
            : base(text, type, o => changeStateFunc?.Invoke((T)o) ?? o)
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
