// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
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

        private IAPIProvider api;
        private ScheduledDelegate fetchMessagesScheduleder;
        private GetMessagesRequest fetchMsgReq;
        private GetPrivateMessagesRequest fetchPrivateMsgReq;
        private long? lastChannelMsgId;
        private long? lastUserMsgId;

        public void OpenChannel(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            CurrentChannel.Value = AvailableChannels.FirstOrDefault(c => c.Name == name)
                                   ?? throw new ChannelNotFoundException(name);
        }

        public void OpenUserChannel(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            CurrentChannel.Value = JoinedChannels.FirstOrDefault(c => c.Target == TargetType.User && c.Id == user.Id)
                                   ?? new Channel(user);
        }

        public ChannelManager()
        {
            CurrentChannel.ValueChanged += currentChannelChanged;
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

            if (!api.IsLoggedIn)
            {
                CurrentChannel.Value.AddNewMessages(new ErrorMessage("Please sign in to participate in chat!"));
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

            CurrentChannel.Value.AddLocalEcho(message);

            var req = new PostMessageRequest(message);
            req.Failure += exception =>
            {
                Logger.Error(exception, "Posting message failed.");
                CurrentChannel.Value?.ReplaceMessage(message, null);
            };
            req.Success += m => CurrentChannel.Value?.ReplaceMessage(message, m);
            api.Queue(req);
        }

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
            if (fetchMsgReq == null)
                fetchMessages(
                    () => new GetMessagesRequest(JoinedChannels.Where(c => c.Target == TargetType.Channel), lastChannelMsgId),
                    messages =>
                    {
                        if (messages == null)
                            return;
                        handleChannelMessages(messages);
                        lastChannelMsgId = messages.LastOrDefault()?.Id ?? lastChannelMsgId;
                        fetchMsgReq = null;
                    }
                );


            if (fetchPrivateMsgReq == null)
                fetchMessages(
                    () => new GetPrivateMessagesRequest(lastChannelMsgId),
                    messages =>
                    {
                        if (messages == null)
                            return;
                        handleUserMessages(messages);
                        lastUserMsgId = messages.LastOrDefault()?.Id ?? lastUserMsgId;
                        fetchPrivateMsgReq = null;
                    }
                );
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
            var joinedUserChannels = JoinedChannels.Where(c => c.Target == TargetType.User).ToList();

            var outgoingMessages = messages.Where(m => m.Sender.Id == api.LocalUser.Value.Id);
            var outgoingMessagesGroups = outgoingMessages.GroupBy(m => m.TargetId);
            var incomingMessagesGroups = messages.Except(outgoingMessages).GroupBy(m => m.UserId);

            foreach (var messageGroup in incomingMessagesGroups)
            {
                var targetUser = messageGroup.First().Sender;
                var channel = joinedUserChannels.FirstOrDefault(c => c.Id == targetUser.Id);

                if (channel == null)
                {
                    channel = new Channel(targetUser);
                    JoinedChannels.Add(channel);
                    joinedUserChannels.Add(channel);
                }

                channel.AddNewMessages(messageGroup.ToArray());
                var outgoingTargetMessages = outgoingMessagesGroups.FirstOrDefault(g => g.Key == targetUser.Id);
                if (outgoingTargetMessages != null)
                    channel.AddNewMessages(outgoingTargetMessages.ToArray());
            }

            var withoutReplyGroups = outgoingMessagesGroups.Where(g => joinedUserChannels.All(m => m.Id != g.Key));

            foreach (var withoutReplyGroup in withoutReplyGroups)
            {
                var userReq = new GetUserRequest(withoutReplyGroup.First().TargetId);

                userReq.Failure += exception => Logger.Error(exception, "Failed to get user informations.");
                userReq.Success += user =>
                {
                    var channel = new Channel(user);

                    channel.AddNewMessages(withoutReplyGroup.ToArray());
                    JoinedChannels.Add(channel);
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
                channels.Where(channel => AvailableChannels.All(c => c.Id != channel.Id))
                        .ForEach(channel => AvailableChannels.Add(channel));

                channels.Where(channel => defaultChannels.Contains(channel.Name))
                        .Where(channel => JoinedChannels.All(c => c.Id != channel.Id))
                        .ForEach(channel =>
                        {
                            JoinedChannels.Add(channel);

                            var fetchInitialMsgReq = new GetMessagesRequest(new[] { channel }, null);
                            fetchInitialMsgReq.Success += handleChannelMessages;
                            fetchInitialMsgReq.Failure += exception => Logger.Error(exception, "Failed to fetch inital messages.");
                            api.Queue(fetchInitialMsgReq);
                        });

                fetchNewMessages();
            };
            req.Failure += error => Logger.Error(error, "Fetching channel list failed");

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
                    fetchMsgReq?.Cancel();
                    fetchMsgReq = null;
                    fetchMessagesScheduleder?.Cancel();
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


    public class ChannelNotFoundException : Exception
    {
        public ChannelNotFoundException(string channelName)
            : base($"A channel with the name {channelName} could not be found.")
        {
        }
    }
}
