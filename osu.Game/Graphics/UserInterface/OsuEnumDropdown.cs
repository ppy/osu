// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.ComponentModel;
using System.Reflection;
using System.Collections.Generic;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuEnumDropdown<T> : OsuDropdown<T>
    {
        public OsuEnumDropdown()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OsuEnumDropdown only supports enums as the generic type argument");

            List<KeyValuePair<string, T>> items = new List<KeyValuePair<string, T>>();
            foreach (var val in (T[])Enum.GetValues(typeof(T)))
            {
                var field = typeof(T).GetField(Enum.GetName(typeof(T), val));
                items.Add(
                    new KeyValuePair<string, T>(
                        field.GetCustomAttribute<DescriptionAttribute>()?.Description ?? Enum.GetName(typeof(T), val),
                        val
                    )
                );
            }
            Items = items;
        }
    }
}
