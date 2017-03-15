// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.UserInterface.Tab;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabControl<T> : TabControl<T>
    {
        protected override TabDropDownMenu<T> CreateDropDownMenu() => new FilterTabDropDownMenu<T>();

        protected override TabItem<T> CreateTabItem(T value) => new FilterTabItem<T> { Value = value };

        public FilterTabControl(float offset = 0) : base(offset)
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("FilterTabControl only supports enums as the generic type argument");

            foreach (var val in (T[])Enum.GetValues(typeof(T)))
                AddTab(val);
        }
    }
}
