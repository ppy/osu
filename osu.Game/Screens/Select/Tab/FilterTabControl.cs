// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.UserInterface.Tab;

namespace osu.Game.Screens.Select.Tab
{
    public class FilterTabControl<T> : TabControl<T>
    {
        protected override TabDropDownMenu<T> CreateDropDownMenu() => new FilterTabDropDownMenu<T>();

        protected override TabItem<T> CreateTabItem(T value) => new FilterTabItem<T> { Value = value };

        public FilterTabControl(float offset, params T[] pinned) : base(offset, pinned)
        {
        }

        public FilterTabControl(params T[] pinned) : base(pinned)
        {
        }
    }
}
