// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat.Tabs;

namespace osu.Game.Overlays.Chat
{
    public class ChatTabControl : Container, IHasCurrentValue<Channel>
    {
        public readonly ChannelTabControl ChannelTabControl;

        public Bindable<Channel> Current { get; } = new Bindable<Channel>();
        public Action<Channel> OnRequestLeave;

        public ChatTabControl()
        {
            Masking = false;

            Children = new Drawable[]
            {
                ChannelTabControl = new ChannelTabControl
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    OnRequestLeave = channel => OnRequestLeave?.Invoke(channel)
                },
            };

            Current.ValueChanged += currentTabChanged;
            ChannelTabControl.Current.ValueChanged += channel =>
            {
                if (channel != null)
                    Current.Value = channel;
            };
        }

        private void currentTabChanged(Channel channel)
        {
            ChannelTabControl.Current.Value = channel;
        }

        /// <summary>
        /// Adds a channel to the ChatTabControl.
        /// The first channel added will automaticly selected.
        /// </summary>
        /// <param name="channel">The channel that is going to be added.</param>
        public void AddItem(Channel channel)
        {
            if (!ChannelTabControl.Items.Contains(channel))
                ChannelTabControl.AddItem(channel);

            if (Current.Value == null)
                Current.Value = channel;
        }

        /// <summary>
        /// Removes a channel from the ChatTabControl.
        /// If the selected channel is the one that is beeing removed, the next available channel will be selected.
        /// </summary>
        /// <param name="channel">The channel that is going to be removed.</param>
        public void RemoveItem(Channel channel)
        {
            ChannelTabControl.RemoveItem(channel);
            if (Current.Value == channel)
                Current.Value = ChannelTabControl.Items.FirstOrDefault();
        }
    }
}
