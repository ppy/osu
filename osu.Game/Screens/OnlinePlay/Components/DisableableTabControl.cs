// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.OnlinePlay.Components
{
    public abstract partial class DisableableTabControl<T> : OsuTabControl<T>
    {
        public readonly BindableBool Enabled = new BindableBool(true);

        protected override void AddTabItem(TabItem<T> tab, bool addToDropdown = true)
        {
            if (tab is DisableableTabItem disableable)
                disableable.Enabled.BindTo(Enabled);
            base.AddTabItem(tab, addToDropdown);
        }

        protected abstract partial class DisableableTabItem : TabItem<T>
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
