// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Resources.Localisation.Web;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Component that handles creating and posting notifications for incoming messages.
    /// </summary>
    public partial class MessageNotifier : Component
    {
        [Resolved]
        private INotificationOverlay notifications { get; set; }

        [Resolved]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved]
        private ChannelManager channelManager { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        private Bindable<bool> notifyOnUsername;
        private Bindable<bool> notifyOnPrivateMessage;

        private readonly IBindable<APIUser> localUser = new Bindable<APIUser>();
        private readonly IBindableList<Channel> joinedChannels = new BindableList<Channel>();

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            notifyOnUsername = config.GetBindable<bool>(OsuSetting.NotifyOnUsernameMentioned);
            notifyOnPrivateMessage = config.GetBindable<bool>(OsuSetting.NotifyOnPrivateMessage);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            joinedChannels.BindCollectionChanged(channelsChanged, true);

            localUser.BindTo(api.LocalUser);
            joinedChannels.BindTo(channelManager.JoinedChannels);
        }

        private void channelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (var channel in e.NewItems.Cast<Channel>())
                        channel.NewMessagesArrived += checkNewMessages;

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var channel in e.OldItems.Cast<Channel>())
                        channel.NewMessagesArrived -= checkNewMessages;

                    break;
            }
        }

        private void checkNewMessages(IEnumerable<Message> messages)
        {
            if (!messages.Any())
                return;

            var channel = channelManager.JoinedChannels.SingleOrDefault(c => c.Id > 0 && c.Id == messages.First().ChannelId);

            if (channel == null)
                return;

            // Only send notifications if ChatOverlay or the target channel aren't visible, or if the window is unfocused
            if (chatOverlay.IsPresent && channelManager.CurrentChannel.Value == channel && host.IsActive.Value)
                return;

            foreach (var message in messages.OrderByDescending(m => m.Id))
            {
                // ignore messages that already have been read
                if (message.Id <= channel.LastReadId)
                    return;

                // ignore notifications triggered by local user's own chat messages
                if (message.Sender.Id == localUser.Value.Id)
                    continue;

                // check for private messages first to avoid both posting two notifications about the same message
                if (checkForPMs(channel, message))
                    continue;

                checkForMentions(channel, message);
            }
        }

        /// <summary>
        /// Checks whether the user enabled private message notifications and whether specified <paramref name="message"/> is a direct message.
        /// </summary>
        /// <param name="channel">The channel associated to the <paramref name="message"/></param>
        /// <param name="message">The message to be checked</param>
        /// <returns>Whether a notification was fired.</returns>
        private bool checkForPMs(Channel channel, Message message)
        {
            if (!notifyOnPrivateMessage.Value || channel.Type != ChannelType.PM)
                return false;

            notifications.Post(new PrivateMessageNotification(message, channel));
            return true;
        }

        private void checkForMentions(Channel channel, Message message)
        {
            if (!notifyOnUsername.Value)
                return;

            var match = MatchUsername(message.Content, localUser.Value.Username);
            if (!match.Success)
                return;

            notifications.Post(new MentionNotification(message, channel, match));
        }

        /// <summary>
        /// Checks if <paramref name="message"/> mentions <paramref name="username"/>.
        /// This will match against the case where underscores are used instead of spaces (which is how osu-stable handles usernames with spaces).
        /// </summary>
        public static Match MatchUsername(string message, string username)
        {
            string fullName = Regex.Escape(username);
            string underscoreName = Regex.Escape(username.Replace(' ', '_'));
            return Regex.Match(message, $@"(^|\W)({fullName}|{underscoreName})($|\W)", RegexOptions.IgnoreCase);
        }

        private const int truncate_length = 60;

        public partial class PrivateMessageNotification : UserAvatarNotification
        {
            private readonly Message message;
            private readonly Channel channel;

            public PrivateMessageNotification(Message message, Channel channel)
                : base(message.Sender)
            {
                this.message = message;
                this.channel = channel;
            }

            [BackgroundDependencyLoader]
            private void load(ChatOverlay chatOverlay, INotificationOverlay notificationOverlay, OverlayColourProvider colourProvider)
            {
                TextFlow.AddText(NotificationsStrings.ItemChannelChannelDefault.ToUpper(), s => s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold));
                TextFlow.NewLine();
                TextFlow.AddText($"{message.Sender.Username}", s =>
                {
                    s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold);
                    s.Colour = colourProvider.Content2;
                });
                TextFlow.AddParagraph($"\"{message.Content.Truncate(truncate_length)}\"");

                Avatar.Colour = OsuColour.Gray(0.4f);
                Icon = FontAwesome.Solid.Comments;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.HighlightMessage(message, channel);
                    return true;
                };
            }
        }

        public partial class MentionNotification : UserAvatarNotification
        {
            public override string PopInSampleName => "UI/notification-mention";

            private readonly Message message;
            private readonly Channel channel;
            private readonly Match match;

            public MentionNotification(Message message, Channel channel, Match match)
                : base(message.Sender)
            {
                this.message = message;
                this.channel = channel;
                this.match = match;
            }

            [BackgroundDependencyLoader]
            private void load(ChatOverlay chatOverlay, INotificationOverlay notificationOverlay, OverlayColourProvider colourProvider)
            {
                TextFlow.AddText(Localisation.NotificationsStrings.Mention.ToUpper(), s => s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.Bold));
                TextFlow.NewLine();
                TextFlow.AddText($"{message.Sender.Username} in {channel.Name}", s =>
                {
                    s.Font = OsuFont.Style.Caption2.With(weight: FontWeight.SemiBold);
                    s.Colour = colourProvider.Content2;
                });

                int start = match.Index;
                int end = match.Index + match.Length;

                TextFlow.AddParagraph($"\"{message.Content[..start].Truncate(truncate_length / 2, "…", from: TruncateFrom.Left)}");
                TextFlow.AddText(message.Content[start..end], s =>
                {
                    s.Font = s.Font.With(weight: FontWeight.SemiBold);
                    s.Colour = colourProvider.Colour0;
                });
                TextFlow.AddText($"{message.Content[end..].Truncate(truncate_length / 2, "…", from: TruncateFrom.Right)}\"");

                Avatar.Colour = OsuColour.Gray(0.4f);
                Icon = FontAwesome.Solid.At;

                Activated = delegate
                {
                    notificationOverlay.Hide();
                    chatOverlay.HighlightMessage(message, channel);
                    return true;
                };
            }
        }
    }
}
