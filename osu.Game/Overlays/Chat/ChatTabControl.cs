using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class ChatTabControl : Container, IHasCurrentValue<ChatBase>
    {
        public readonly ChannelTabControl channelTabControl;
        private readonly UserTabControl userTabControl;

        public Bindable<ChatBase> Current { get; } = new Bindable<ChatBase>();
        public Action<ChatBase> OnRequestLeave;
        public Action OnRequestChannelSelection;

        public ChatTabControl()
        {
            Masking = false;

            Children = new Drawable[]
            {
                channelTabControl = new ChannelTabControl
                {
                    Width = 0.5f,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                    OnRequestLeave = chat => OnRequestLeave?.Invoke(chat)
                },
                userTabControl = new UserTabControl
                {
                    Width = 0.5f,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativeSizeAxes = Axes.Both,
                    OnRequestLeave = chat => OnRequestLeave?.Invoke(chat)
                },
            };

            Current.ValueChanged += currentTabChanged;
            channelTabControl.Current.ValueChanged += chat =>
            {
                if (chat != null)
                    Current.Value = chat;
            };
            userTabControl.Current.ValueChanged += chat =>
            {
                if (chat != null)
                    Current.Value = chat;
            };
        }

        private void currentTabChanged(ChatBase tab)
        {
            switch (tab)
            {
                case UserChat userChat:
                    userTabControl.Current.Value = userChat;
                    channelTabControl.Current.Value = null;
                    break;
                case ChannelChat channelChat:
                    channelTabControl.Current.Value = channelChat;
                    userTabControl.Current.Value = null;
                    break;
            }
        }

        public void AddItem(ChatBase chat)
        {
            switch (chat)
            {
                case UserChat userChat:
                    userTabControl.AddItem(userChat);
                    break;
                case ChannelChat channelChat:
                    channelTabControl.AddItem(channelChat);
                    break;
            }
        }

        public void RemoveItem(ChatBase chat)
        {
            switch (chat)
            {
                case UserChat userChat:
                    userTabControl.RemoveItem(userChat);
                    Current.Value = null;
                    break;
                case ChannelChat channelChat:
                    channelTabControl.RemoveItem(channelChat);
                    Current.Value = null;
                    break;
            }
        }
    }
}
