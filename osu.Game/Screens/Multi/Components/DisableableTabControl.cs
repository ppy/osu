// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Configuration;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.Multi.Components
{
    public abstract class DisableableTabControl<T> : TabControl<T>
    {
        public readonly BindableBool Enabled = new BindableBool();

        protected override void AddTabItem(TabItem<T> tab, bool addToDropdown = true)
        {
            if (tab is DisableableTabItem<T> disableable)
                disableable.Enabled.BindTo(Enabled);
            base.AddTabItem(tab, addToDropdown);
        }

        protected abstract class DisableableTabItem<T> : TabItem<T>
        {
            public readonly BindableBool Enabled = new BindableBool();

            protected DisableableTabItem(T value)
                : base(value)
            {
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled)
                    return true;
                return base.OnClick(e);
            }
        }
    }
}
