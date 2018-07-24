// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Manages everything channel related
    /// </summary>
    public class ChannelManager : Component, IOnlineComponent
    {
        /// <summary>
        /// The channels the player joins on startup
        /// </summary>
        private readonly string[] defaultChannels =
        {
            @"#lazer",
            @"#osu",
            @"#lobby"
        };

        /// <summary>
        /// The currently opened channel
        /// </summary>
        public Bindable<Channel> CurrentChannel { get; } = new Bindable<Channel>();

        /// <summary>
        /// The Channels the player has joined
        /// </summary>
        public ObservableCollection<Channel> JoinedChannels { get; } = new ObservableCollection<Channel>();

        /// <summary>
        /// The channels available for the player to join
        /// </summary>
        public ObservableCollection<Channel> AvailableChannels { get; } = new ObservableCollection<Channel>();

        private readonly IncomingMessagesHandler channelMessagesHandler;
        private readonly IncomingMessagesHandler privateMessagesHandler;

        private IAPIProvider api;
        private ScheduledDelegate fetchMessagesScheduleder;

        /// <summary>
        /// Opens a channel or switches to the channel if already opened.
        /// </summary>
        /// <exception cref="ChannelNotFoundException">If the name of the specifed channel was not found this exception will be thrown.</exception>
        /// <param name="name"></param>
        public void OpenChannel(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            CurrentChannel.Value = AvailableChannels.FirstOrDefault(c => c.Name == name)
                                   ?? throw new ChannelNotFoundException(name);
        }

        /// <summary>
        /// Opens a new private channel.
        /// </summary>
        /// <param name="user">The user the private channel is opened with.</param>
        public void OpenPrivateChannel(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            CurrentChannel.Value = JoinedChannels.FirstOrDefault(c => c.Target == TargetType.User && c.Id == user.Id)
                                   ?? new PrivateChannel { User = user };
        }

        public ChannelManager()
        {
            CurrentChannel.ValueChanged += currentChannelChanged;

            channelMessagesHandler = new IncomingMessagesHandler(
                () => new GetMessagesRequest(JoinedChannels.Where(c => c.Target == TargetType.Channel), channelMessagesHandler.LastMessageId), handleChannelMessages);

            privateMessagesHandler = new IncomingMessagesHandler(
                () => new GetPrivateMessagesRequest(privateMessagesHandler.LastMessageId),handleUserMessages);
        }

        private void currentChannelChanged(Channel channel)
        {
            if (!JoinedChannels.Contains(channel))
                JoinedChannels.Add(channel);
        }

        /// <summary>
        /// Posts a message to the currently opened channel.
        /// </summary>
        /// <param name="text">The message text that is going to be posted</param>
        /// <param name="isAction">Is true if the message is an action, e.g.: user is currently eating </param>
        public void PostMessage(string text, bool isAction = false)
        {
            if (CurrentChannel.Value == null)
                return;

            var currentChannel = CurrentChannel.Value;

            if (!api.IsLoggedIn)
            {
                currentChannel.AddNewMessages(new ErrorMessage("Please sign in to participate in chat!"));
                return;
            }

            var message = new LocalEchoMessage
            {
                Sender = api.LocalUser.Value,
                Timestamp = DateTimeOffset.Now,
                TargetType = CurrentChannel.Value.Target,
                TargetId = CurrentChannel.Value.Id,
                IsAction = isAction,
                Content = text
            };

            currentChannel.AddLocalEcho(message);

            var req = new PostMessageRequest(message);
            req.Failure += exception =>
            {
                Logger.Error(exception, "Posting message failed.");
                currentChannel.ReplaceMessage(message, null);
            };
            req.Success += m => currentChannel.ReplaceMessage(message, m);
            api.Queue(req);
        }

        /// <summary>
        /// Posts a command locally. Commands like /help will result in a help message written in the current channel.
        /// </summary>
        /// <param name="text">the text containing the command identifier and command parameters.</param>
        public void PostCommand(string text)
        {
            if (CurrentChannel.Value == null)
                return;

            var parameters = text.Split(new[] { ' ' }, 2);
            string command = parameters[0];
            string content = parameters.Length == 2 ? parameters[1] : string.Empty;

            switch (command)
            {
                case "me":
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        CurrentChannel.Value.AddNewMessages(new ErrorMessage("Usage: /me [action]"));
                        break;
                    }

                    PostMessage(content, true);
                    break;

                case "help":
                    CurrentChannel.Value.AddNewMessages(new InfoMessage("Supported commands: /help, /me [action]"));
                    break;

                default:
                    CurrentChannel.Value.AddNewMessages(new ErrorMessage($@"""/{command}"" is not supported! For a list of supported commands see /help"));
                    break;
            }
        }

        private void fetchNewMessages()
        {
            if (channelMessagesHandler.CanRequestNewMessages)
                channelMessagesHandler.RequestNewMessages(api);

            if (privateMessagesHandler.CanRequestNewMessages)
                privateMessagesHandler.RequestNewMessages(api);
        }

        private void fetchMessages(Func<APIMessagesRequest> messagesRequest, Action<List<Message>> handler)
        {
            if (messagesRequest == null)
                throw new ArgumentNullException(nameof(messagesRequest));
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));

            var messagesReq = messagesRequest.Invoke();

            messagesReq.Success += handler.Invoke;
            messagesReq.Failure += exception => Logger.Error(exception, "Fetching messages failed.");

            api.Queue(messagesReq);
        }

        private void handleUserMessages(IEnumerable<Message> messages)
        {
            var joinedPrivateChannels = JoinedChannels.Where(c => c.Target == TargetType.User).ToList();

            Channel getChannelForUser(User user)
            {
                var channel = joinedPrivateChannels.FirstOrDefault(c => c.Id == user.Id);

                if (channel == null)
                {
                    channel = new PrivateChannel { User = user };
                    JoinedChannels.Add(channel);
                    joinedPrivateChannels.Add(channel);
                }

                return channel;
            }

            long localUserId = api.LocalUser.Value.Id;

            var outgoingGroups = messages.Where(m => m.Sender.Id == localUserId).GroupBy(m => m.TargetId);
            var incomingGroups = messages.Where(m => m.Sender.Id != localUserId).GroupBy(m => m.UserId);

            foreach (var group in incomingGroups)
            {
                var targetUser = group.First().Sender;

                var channel = getChannelForUser(targetUser);

                channel.AddNewMessages(group.ToArray());

                var outgoingTargetMessages = outgoingGroups.FirstOrDefault(g => g.Key == targetUser.Id);
                if (outgoingTargetMessages != null)
                    channel.AddNewMessages(outgoingTargetMessages.ToArray());
            }

            // Because of the way the API provides data right now, outgoing messages do not contain required
            // user (or in the future, target channel) metadata. As such we need to do a second request
            // to find out the specifics of the user.
            var withoutReplyGroups = outgoingGroups.Where(g => joinedPrivateChannels.All(m => m.Id != g.Key));

            foreach (var withoutReplyGroup in withoutReplyGroups)
            {
                var userReq = new GetUserRequest(withoutReplyGroup.First().TargetId);

                userReq.Failure += exception => Logger.Error(exception, "Failed to get user informations.");
                userReq.Success += user =>
                {
                    var channel = getChannelForUser(user);
                    channel.AddNewMessages(withoutReplyGroup.ToArray());
                };

                api.Queue(userReq);
            }
        }

        private void handleChannelMessages(IEnumerable<Message> messages)
        {
            var channels = JoinedChannels.ToList();

            foreach (var group in messages.GroupBy(m => m.TargetId))
                channels.Find(c => c.Id == group.Key)?.AddNewMessages(group.ToArray());
        }

        private void initializeDefaultChannels()
        {
            var req = new ListChannelsRequest();

            req.Success += channels =>
            {
                foreach (var channel in channels)
                {
                    if (JoinedChannels.Any(c => c.Id == channel.Id))
                        continue;

                    // add as available if not already
                    if (AvailableChannels.All(c => c.Id != channel.Id))
                        AvailableChannels.Add(channel);

                    // join any channels classified as "defaults"
                    if (defaultChannels.Any(c => c.Equals(channel.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        JoinedChannels.Add(channel);

                        // TODO: remove this when the API supports returning initial fetch messages for more than one channel.
                        // right now it caps out at 50 messages and therefore only returns one channel's worth of content.
                        var fetchInitialMsgReq = new GetMessagesRequest(new[] { channel }, null);
                        fetchInitialMsgReq.Success += handleChannelMessages;
                        fetchInitialMsgReq.Failure += exception => Logger.Error(exception, "Failed to fetch inital messages.");
                        api.Queue(fetchInitialMsgReq);
                    }
                }

                fetchNewMessages();
            };
            req.Failure += error =>
            {
                Logger.Error(error, "Fetching channel list failed");

                initializeDefaultChannels();
            };

            api.Queue(req);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                case APIState.Online:
                    if (JoinedChannels.Count == 0)
                        initializeDefaultChannels();

                    fetchMessagesScheduleder = Scheduler.AddDelayed(fetchNewMessages, 1000, true);
                    break;
                default:
                    channelMessagesHandler.CancelOngoingRequests();
                    privateMessagesHandler.CancelOngoingRequests();

                    fetchMessagesScheduleder?.Cancel();
                    fetchMessagesScheduleder = null;
                    break;
            }
        }

        [BackgroundDependencyLoader]
        private void load(IAPIProvider api)
        {
            this.api = api;
            api.Register(this);
        }
    }

    /// <summary>
    /// An exception thrown when a channel could not been found.
    /// </summary>
    public class ChannelNotFoundException : Exception
    {
        public ChannelNotFoundException(string channelName)
            : base($"A channel with the name {channelName} could not be found.")
        {
        }
    }
}
