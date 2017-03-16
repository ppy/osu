// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics.UserInterface.Tab;

namespace osu.Game.Graphics.UserInterface
{
    public class OsuTabControl<T> : TabControl<T>
    {
        protected override TabDropDownMenu<T> CreateDropDownMenu() => new OsuTabDropDownMenu<T>();

        protected override TabItem<T> CreateTabItem(T value) => new OsuTabItem<T> { Value = value };

        public OsuTabControl(float offset = 0) : base(offset)
        {
            if (!typeof(T).IsEnum)
                throw new InvalidOperationException("OsuTabControl only supports enums as the generic type argument");

            foreach (var val in (T[])Enum.GetValues(typeof(T)))
                AddTab(val);
        }
    }
}
