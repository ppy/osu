// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

namespace osu.Game.Overlays.Options
{
    public class OptionEnumDropdown<T> : OptionDropdown<T>
    {
        public OptionEnumDropdown()
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OptionsDropdown only supports enums as the generic type argument");

            List<KeyValuePair<string, T>> items = new List<KeyValuePair<string, T>>();
            foreach(var val in (T[])Enum.GetValues(typeof(T)))
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
