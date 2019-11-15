// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osuTK;
using System;
using System.Linq;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class ChannelTabControl : OsuTabControl<Channel>
    {
        public static readonly float SHEAR_WIDTH = 10;

        public Action<Channel> OnRequestLeave;

        public readonly Bindable<bool> ChannelSelectorActive = new Bindable<bool>();

        private readonly ChannelSelectorTabItem selectorTab;

        public ChannelTabControl()
        {
            Padding = new MarginPadding { Left = 50 };

            TabContainer.Spacing = new Vector2(-SHEAR_WIDTH, 0);
            TabContainer.Masking = false;

            AddTabItem(selectorTab = new ChannelSelectorTabItem());

            ChannelSelectorActive.BindTo(selectorTab.Active);
        }

        protected override void AddTabItem(TabItem<Channel> item, bool addToDropdown = true)
        {
            if (item != selectorTab && TabContainer.GetLayoutPosition(selectorTab) < float.MaxValue)
                // performTabSort might've made selectorTab's position wonky, fix it
                TabContainer.SetLayoutPosition(selectorTab, float.MaxValue);

            ((ChannelTabItem)item).OnRequestClose += tabCloseRequested;

            base.AddTabItem(item, addToDropdown);
        }

        protected override TabItem<Channel> CreateTabItem(Channel value)
        {
            switch (value.Type)
            {
                default:
                    return new ChannelTabItem(value);

                case ChannelType.PM:
                    return new PrivateChannelTabItem(value);
            }
        }

        /// <summary>
        /// Adds a channel to the ChannelTabControl.
        /// The first channel added will automaticly selected.
        /// </summary>
        /// <param name="channel">The channel that is going to be added.</param>
        public void AddChannel(Channel channel)
        {
            if (!Items.Contains(channel))
                AddItem(channel);

            if (Current.Value == null)
                Current.Value = channel;
        }

        /// <summary>
        /// Removes a channel from the ChannelTabControl.
        /// If the selected channel is the one that is beeing removed, the next available channel will be selected.
        /// </summary>
        /// <param name="channel">The channel that is going to be removed.</param>
        public void RemoveChannel(Channel channel)
        {
            RemoveItem(channel);

            if (Current.Value == channel)
                Current.Value = Items.FirstOrDefault();
        }

        protected override void SelectTab(TabItem<Channel> tab)
        {
            if (tab is ChannelSelectorTabItem)
            {
                tab.Active.Value = true;
                return;
            }

            base.SelectTab(tab);
            selectorTab.Active.Value = false;
        }

        private void tabCloseRequested(TabItem<Channel> tab)
        {
            int totalTabs = TabContainer.Count - 1; // account for selectorTab
            int currentIndex = MathHelper.Clamp(TabContainer.IndexOf(tab), 1, totalTabs);

            if (tab == SelectedTab && totalTabs > 1)
                // Select the tab after tab-to-be-removed's index, or the tab before if current == last
                SelectTab(TabContainer[currentIndex == totalTabs ? currentIndex - 1 : currentIndex + 1]);
            else if (totalTabs == 1 && !selectorTab.Active.Value)
                // Open channel selection overlay if all channel tabs will be closed after removing this tab
                SelectTab(selectorTab);

            OnRequestLeave?.Invoke(tab.Value);
        }
    }
}
