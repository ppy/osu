// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using OpenTK;

namespace osu.Game.Overlays.Chat
{
    public class UserTabControl : OsuTabControl<UserChat>
    {
        protected override TabItem<UserChat> CreateTabItem(UserChat value) => new UserTabItem(value) { OnRequestClose = tabCloseRequested };

        protected override Dropdown<UserChat> CreateDropdown() => null;

        public Action<UserChat> OnRequestLeave;

        public UserTabControl()
        {
            TabContainer.Spacing = new Vector2(-10, 0);
            TabContainer.Masking = false;
            Margin = new MarginPadding
            {
                Right = 10
            };
        }

        protected override void AddTabItem(TabItem<UserChat> item, bool addToDropdown = true)
        {
            base.AddTabItem(item, addToDropdown);

            if (SelectedTab == null)
                SelectTab(item);
        }

        private void tabCloseRequested(TabItem<UserChat> priv)
        {
            int totalTabs = TabContainer.Count -1; // account for selectorTab
            int currentIndex = MathHelper.Clamp(TabContainer.IndexOf(priv), 1, totalTabs);

            if (priv == SelectedTab && totalTabs > 1)
                // Select the tab after tab-to-be-removed's index, or the tab before if current == last
                SelectTab(TabContainer[currentIndex == totalTabs ? currentIndex - 1 : currentIndex + 1]);

            OnRequestLeave?.Invoke(priv.Value);
        }

        public void DeselectAll()
        {
            if (SelectedTab != null)
                SelectedTab.Active.Value = false;
            SelectedTab = null;

        }
    }
}
