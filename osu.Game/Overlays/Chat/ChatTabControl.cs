using System;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class ChatTabControl : Container, IHasCurrentValue<Channel>
    {
        public readonly ChannelTabControl ChannelTabControl;
        public readonly UserTabControl UserTabControl;

        public Bindable<Channel> Current { get; } = new Bindable<Channel>();
        public Action<Channel> OnRequestLeave;

        public ChatTabControl()
        {
            Masking = false;

            Children = new Drawable[]
            {
                ChannelTabControl = new ChannelTabControl
                {
                    Width = 0.5f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    OnRequestLeave = channel => OnRequestLeave?.Invoke(channel)
                },
                UserTabControl = new UserTabControl
                {
                    Width = 0.5f,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
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
            UserTabControl.Current.ValueChanged += channel =>
            {
                if (channel != null)
                    Current.Value = channel;
            };
        }

        private void currentTabChanged(Channel channel)
        {
            switch (channel.Target)
            {
                case TargetType.User:
                    UserTabControl.Current.Value = channel;
                    ChannelTabControl.Current.Value = null;
                    break;
                case TargetType.Channel:
                    ChannelTabControl.Current.Value = channel;
                    UserTabControl.Current.Value = null;
                    break;
            }
        }

        public void AddItem(Channel channel)
        {
            switch (channel.Target)
            {
                case TargetType.User:
                    UserTabControl.AddItem(channel);
                    break;
                case TargetType.Channel:
                    ChannelTabControl.AddItem(channel);
                    break;
            }
        }

        public void RemoveItem(Channel channel)
        {
            Channel nextSelectedChannel = null;

            switch (channel.Target)
            {
                case TargetType.User:
                    UserTabControl.RemoveItem(channel);
                    if (Current.Value == channel)
                        Current.Value = UserTabControl.Items.FirstOrDefault() ?? ChannelTabControl.Items.FirstOrDefault();
                    break;
                case TargetType.Channel:
                    ChannelTabControl.RemoveItem(channel);
                    if (Current.Value == channel)
                        Current.Value = ChannelTabControl.Items.FirstOrDefault() ?? UserTabControl.Items.FirstOrDefault();
                    break;
            }
        }
    }
}
