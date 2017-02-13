using System;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Generic;

namespace osu.Game.Overlays.Options
{
    public class OptionEnumDropDown<T> : OptionDropDown<T>
    {
        public OptionEnumDropDown()
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
