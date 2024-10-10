// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Reflection;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;

namespace osu.Game.Utils
{
    internal static class BindableValueAccessor
    {
        private static readonly MethodInfo get_method = typeof(BindableValueAccessor).GetMethod(nameof(getValue), BindingFlags.Static | BindingFlags.NonPublic)!;
        private static readonly MethodInfo set_method = typeof(BindableValueAccessor).GetMethod(nameof(setValue), BindingFlags.Static | BindingFlags.NonPublic)!;

        public static object GetValue(IBindable bindable)
        {
            Type? bindableWithValueType = bindable.GetType().GetInterfaces().FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IBindable<>));
            if (bindableWithValueType == null)
                return bindable;

            return get_method.MakeGenericMethod(bindableWithValueType.GenericTypeArguments[0]).Invoke(null, [bindable])!;
        }

        public static void SetValue(IBindable bindable, object value)
        {
            Type? bindableWithValueType = bindable.GetType().EnumerateBaseTypes().SingleOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Bindable<>));
            if (bindableWithValueType == null)
                return;

            set_method.MakeGenericMethod(bindableWithValueType.GenericTypeArguments[0]).Invoke(null, [bindable, value]);
        }

        private static object getValue<T>(object bindable) => ((IBindable<T>)bindable).Value!;

        private static object setValue<T>(object bindable, object value) => ((Bindable<T>)bindable).Value = (T)value;
    }
}
