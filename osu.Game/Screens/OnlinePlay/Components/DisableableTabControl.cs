// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract class DisableableTabControl<T> : TabControl<T>
    {
        public readonly BindableBool Enabled = new BindableBool();

        protected override void AddTabItem(TabItem<T> tab, bool addToDropdown = true)
        {
            if (tab is DisableableTabItem disableable)
                disableable.Enabled.BindTo(Enabled);
            base.AddTabItem(tab, addToDropdown);
        }

        protected abstract class DisableableTabItem : TabItem<T>
        {
            protected DisableableTabItem(T value)
                : base(value)
            {
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Enabled.Value)
                    return true;

                return base.OnClick(e);
            }
        }
    }
}
