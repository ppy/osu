// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using OpenTK;
using osu.Framework.Configuration;
using System;
using osu.Game.Overlays.Chat.Tabs;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class ChannelTabControl : OsuTabControl<Channel>
    {
        public static readonly float shear_width = 10;

        public Action<Channel> OnRequestLeave;

        public readonly Bindable<bool> ChannelSelectorActive = new Bindable<bool>();

        private readonly ChannelSelectorTabItem selectorTab;

        public ChannelTabControl()
        {
            TabContainer.Margin = new MarginPadding { Left = 50 };
            TabContainer.Spacing = new Vector2(-shear_width, 0);
            TabContainer.Masking = false;

            AddInternal(new SpriteIcon
            {
                Icon = FontAwesome.fa_comments,
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                Size = new Vector2(20),
                Margin = new MarginPadding(10),
            });

            AddTabItem(selectorTab = new ChannelSelectorTabItem(new Channel { Name = "+" }));

            ChannelSelectorActive.BindTo(selectorTab.Active);
        }

        protected override void AddTabItem(TabItem<Channel> item, bool addToDropdown = true)
        {
            if (item != selectorTab && TabContainer.GetLayoutPosition(selectorTab) < float.MaxValue)
                // performTabSort might've made selectorTab's position wonky, fix it
                TabContainer.SetLayoutPosition(selectorTab, float.MaxValue);

            base.AddTabItem(item, addToDropdown);
        }

        protected override TabItem<Channel> CreateTabItem(Channel value)
        {
            switch (value.Target)
            {
                case TargetType.Channel:
                    return new ChannelTabItem(value) { OnRequestClose = tabCloseRequested };
                case TargetType.User:
                    return new UserTabItem(value) { OnRequestClose = tabCloseRequested };
                default:
                    throw new InvalidOperationException("Only TargetType User and Channel are supported.");
            }
        }

        protected override void SelectTab(TabItem<Channel> tab)
        {
            if (tab is ChannelSelectorTabItem)
            {
                tab.Active.Toggle();
                return;
            }

            selectorTab.Active.Value = false;

            base.SelectTab(tab);
        }

        private void tabCloseRequested(TabItem<Channel> tab)
        {
            int totalTabs = TabContainer.Count - 1; // account for selectorTab
            int currentIndex = MathHelper.Clamp(TabContainer.IndexOf(tab), 1, totalTabs);

            if (tab == SelectedTab && totalTabs > 1)
                // Select the tab after tab-to-be-removed's index, or the tab before if current == last
                SelectTab(TabContainer[currentIndex == totalTabs ? currentIndex - 1 : currentIndex + 1]);
            else if (totalTabs == 1 && !selectorTab.Active)
                // Open channel selection overlay if all channel tabs will be closed after removing this tab
                SelectTab(selectorTab);

            OnRequestLeave?.Invoke(tab.Value);
        }
    }
}
