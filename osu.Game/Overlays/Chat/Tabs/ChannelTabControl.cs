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
        public const float SHEAR_WIDTH = 10;

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

            ((ChannelTabItem)item).OnRequestClose += (channelItem) => OnRequestLeave?.Invoke(channelItem.Value);

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
        /// If the selected channel is the one that is being removed, the next available channel will be selected.
        /// </summary>
        /// <param name="channel">The channel that is going to be removed.</param>
        public void RemoveChannel(Channel channel)
        {
            if (Current.Value == channel)
            {
                var itemsList = Items.ToList();
                var dropdownItemsCount = itemsList.Except(VisibleItems).Count();
                var currentIndex = itemsList.IndexOf(channel);
                var isNextTabSelector = itemsList.ElementAt(currentIndex + 1) == selectorTab.Value;

                // If the tab being closed is the last one visible and the '+' is in the dropdown menu
                // the '+' is always gonna be the next tab, even if there are other channels in there
                if (dropdownItemsCount > 1 && isNextTabSelector)
                {
                    RemoveItem(channel);
                    UpdateSubTree(); // This forces the next channel tab to become visible to ensure we can switch to it
                    SelectTab(TabContainer.TabItems.ElementAt(currentIndex));
                }
                else
                {
                    // Show the ChannelSelector if the channel being removed is the last one
                    if (isNextTabSelector && itemsList.Count == 2)
                        SelectTab(selectorTab);
                    else
                        SwitchTab(isNextTabSelector ? -1 : 1);
                    RemoveItem(channel);
                }
            }
            else
                RemoveItem(channel);

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
    }
}
