// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.Chat.Tabs;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Manages everything channel related
    /// </summary>
    public class ChannelManager : PollingComponent, IChannelPostTarget
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

        private readonly BindableList<Channel> availableChannels = new BindableList<Channel>();
        private readonly BindableList<Channel> joinedChannels = new BindableList<Channel>();

        /// <summary>
        /// The currently opened channel
        /// </summary>
        public Bindable<Channel> CurrentChannel { get; } = new Bindable<Channel>();

        /// <summary>
        /// The Channels the player has joined
        /// </summary>
        public IBindableList<Channel> JoinedChannels => joinedChannels;

        /// <summary>
        /// The channels available for the player to join
        /// </summary>
        public IBindableList<Channel> AvailableChannels => availableChannels;

        [Resolved]
        private IAPIProvider api { get; set; }

        public readonly BindableBool HighPollRate = new BindableBool();

        public ChannelManager()
        {
            CurrentChannel.ValueChanged += currentChannelChanged;

            HighPollRate.BindValueChanged(enabled => TimeBetweenPolls.Value = enabled.NewValue ? 1000 : 6000, true);
        }

        /// <summary>
        /// Opens a channel or switches to the channel if already opened.
        /// </summary>
        /// <exception cref="ChannelNotFoundException">If the name of the specifed channel was not found this exception will be thrown.</exception>
        /// <param name="name"></param>
        public void OpenChannel(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            CurrentChannel.Value = AvailableChannels.FirstOrDefault(c => c.Name == name) ?? throw new ChannelNotFoundException(name);
        }

        /// <summary>
        /// Opens a new private channel.
        /// </summary>
        /// <param name="user">The user the private channel is opened with.</param>
        public void OpenPrivateChannel(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.Id == api.LocalUser.Value.Id)
                return;

            CurrentChannel.Value = JoinedChannels.FirstOrDefault(c => c.Type == ChannelType.PM && c.Users.Count == 1 && c.Users.Any(u => u.Id == user.Id))
                                   ?? JoinChannel(new Channel(user));
        }

        private void currentChannelChanged(ValueChangedEvent<Channel> e)
        {
            if (!(e.NewValue is ChannelSelectorTabItem.ChannelSelectorTabChannel))
                JoinChannel(e.NewValue);
        }

        /// <summary>
        /// Ensure we run post actions in sequence, once at a time.
        /// </summary>
        private readonly Queue<Action> postQueue = new Queue<Action>();

        /// <summary>
        /// Posts a message to the currently opened channel.
        /// </summary>
        /// <param name="text">The message text that is going to be posted</param>
        /// <param name="isAction">Is true if the message is an action, e.g.: user is currently eating </param>
        /// <param name="target">An optional target channel. If null, <see cref="CurrentChannel"/> will be used.</param>
        public void PostMessage(string text, bool isAction = false, Channel target = null)
        {
            target ??= CurrentChannel.Value;

            if (target == null)
                return;

            void dequeueAndRun()
            {
                if (postQueue.Count > 0)
                    postQueue.Dequeue().Invoke();
            }

            postQueue.Enqueue(() =>
            {
                if (!api.IsLoggedIn)
                {
                    target.AddNewMessages(new ErrorMessage("Please sign in to participate in chat!"));
                    return;
                }

                var message = new LocalEchoMessage
                {
                    Sender = api.LocalUser.Value,
                    Timestamp = DateTimeOffset.Now,
                    ChannelId = target.Id,
                    IsAction = isAction,
                    Content = text
                };

                target.AddLocalEcho(message);

                // if this is a PM and the first message, we need to do a special request to create the PM channel
                if (target.Type == ChannelType.PM && target.Id == 0)
                {
                    var createNewPrivateMessageRequest = new CreateNewPrivateMessageRequest(target.Users.First(), message);

                    createNewPrivateMessageRequest.Success += createRes =>
                    {
                        target.Id = createRes.ChannelID;
                        target.ReplaceMessage(message, createRes.Message);
                        dequeueAndRun();
                    };

                    createNewPrivateMessageRequest.Failure += exception =>
                    {
                        Logger.Error(exception, "Posting message failed.");
                        target.ReplaceMessage(message, null);
                        dequeueAndRun();
                    };

                    api.Queue(createNewPrivateMessageRequest);
                    return;
                }

                var req = new PostMessageRequest(message);

                req.Success += m =>
                {
                    target.ReplaceMessage(message, m);
                    dequeueAndRun();
                };

                req.Failure += exception =>
                {
                    Logger.Error(exception, "Posting message failed.");
                    target.ReplaceMessage(message, null);
                    dequeueAndRun();
                };

                api.Queue(req);
            });

            // always run if the queue is empty
            if (postQueue.Count == 1)
                dequeueAndRun();
        }

        /// <summary>
        /// Posts a command locally. Commands like /help will result in a help message written in the current channel.
        /// </summary>
        /// <param name="text">the text containing the command identifier and command parameters.</param>
        /// <param name="target">An optional target channel. If null, <see cref="CurrentChannel"/> will be used.</param>
        public void PostCommand(string text, Channel target = null)
        {
            target ??= CurrentChannel.Value;

            if (target == null)
                return;

            var parameters = text.Split(' ', 2);
            string command = parameters[0];
            string content = parameters.Length == 2 ? parameters[1] : string.Empty;

            switch (command)
            {
                case "np":
                    AddInternal(new NowPlayingCommand());
                    break;

                case "me":
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        target.AddNewMessages(new ErrorMessage("Usage: /me [action]"));
                        break;
                    }

                    PostMessage(content, true);
                    break;

                case "join":
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        target.AddNewMessages(new ErrorMessage("Usage: /join [channel]"));
                        break;
                    }

                    var channel = availableChannels.FirstOrDefault(c => c.Name == content || c.Name == $"#{content}");

                    if (channel == null)
                    {
                        target.AddNewMessages(new ErrorMessage($"Channel '{content}' not found."));
                        break;
                    }

                    JoinChannel(channel);
                    break;

                case "help":
                    target.AddNewMessages(new InfoMessage("Supported commands: /help, /me [action], /join [channel], /np"));
                    break;

                default:
                    target.AddNewMessages(new ErrorMessage($@"""/{command}"" is not supported! For a list of supported commands see /help"));
                    break;
            }
        }

        private void handleChannelMessages(IEnumerable<Message> messages)
        {
            var channels = JoinedChannels.ToList();

            foreach (var group in messages.GroupBy(m => m.ChannelId))
                channels.Find(c => c.Id == group.Key)?.AddNewMessages(group.ToArray());
        }

        private void initializeChannels()
        {
            var req = new ListChannelsRequest();

            var joinDefaults = JoinedChannels.Count == 0;

            req.Success += channels =>
            {
                foreach (var channel in channels)
                {
                    var ch = getChannel(channel, addToAvailable: true);

                    // join any channels classified as "defaults"
                    if (joinDefaults && defaultChannels.Any(c => c.Equals(channel.Name, StringComparison.OrdinalIgnoreCase)))
                        joinChannel(ch);
                }
            };
            req.Failure += error =>
            {
                Logger.Error(error, "Fetching channel list failed");
                initializeChannels();
            };

            api.Queue(req);
        }

        /// <summary>
        /// Fetches inital messages of a channel
        ///
        /// TODO: remove this when the API supports returning initial fetch messages for more than one channel by specifying the last message id per channel instead of one last message id globally.
        /// right now it caps out at 50 messages and therefore only returns one channel's worth of content.
        /// </summary>
        /// <param name="channel">The channel </param>
        private void fetchInitalMessages(Channel channel)
        {
            if (channel.Id <= 0 || channel.MessagesLoaded) return;

            var fetchInitialMsgReq = new GetMessagesRequest(channel);
            fetchInitialMsgReq.Success += messages =>
            {
                handleChannelMessages(messages);
                channel.MessagesLoaded = true; // this will mark the channel as having received messages even if there were none.
            };

            api.Queue(fetchInitialMsgReq);
        }

        /// <summary>
        /// Find an existing channel instance for the provided channel. Lookup is performed basd on ID.
        /// The provided channel may be used if an existing instance is not found.
        /// </summary>
        /// <param name="lookup">A candidate channel to be used for lookup or permanently on lookup failure.</param>
        /// <param name="addToAvailable">Whether the channel should be added to <see cref="AvailableChannels"/> if not already.</param>
        /// <param name="addToJoined">Whether the channel should be added to <see cref="JoinedChannels"/> if not already.</param>
        /// <returns>The found channel.</returns>
        private Channel getChannel(Channel lookup, bool addToAvailable = false, bool addToJoined = false)
        {
            Channel found = null;

            bool lookupCondition(Channel ch) => lookup.Id > 0 ? ch.Id == lookup.Id : lookup.Name == ch.Name;

            var available = AvailableChannels.FirstOrDefault(lookupCondition);
            if (available != null)
                found = available;

            var joined = JoinedChannels.FirstOrDefault(lookupCondition);
            if (found == null && joined != null)
                found = joined;

            if (found == null)
            {
                found = lookup;

                // if we're using a channel object from the server, we want to remove ourselves from the users list.
                // this is because we check the first user in the channel to display a name/icon on tabs for now.
                var foundSelf = found.Users.FirstOrDefault(u => u.Id == api.LocalUser.Value.Id);
                if (foundSelf != null)
                    found.Users.Remove(foundSelf);
            }

            if (joined == null && addToJoined) joinedChannels.Add(found);
            if (available == null && addToAvailable) availableChannels.Add(found);

            return found;
        }

        /// <summary>
        /// Joins a channel if it has not already been joined.
        /// </summary>
        /// <param name="channel">The channel to join.</param>
        /// <returns>The joined channel. Note that this may not match the parameter channel as it is a backed object.</returns>
        public Channel JoinChannel(Channel channel) => joinChannel(channel, true);

        private Channel joinChannel(Channel channel, bool fetchInitialMessages = false)
        {
            if (channel == null) return null;

            channel = getChannel(channel, addToJoined: true);

            // ensure we are joined to the channel
            if (!channel.Joined.Value)
            {
                channel.Joined.Value = true;

                switch (channel.Type)
                {
                    case ChannelType.Multiplayer:
                        // join is implicit. happens when you join a multiplayer game.
                        // this will probably change in the future.
                        joinChannel(channel, fetchInitialMessages);
                        return channel;

                    case ChannelType.PM:
                        var createRequest = new CreateChannelRequest(channel);
                        createRequest.Success += resChannel =>
                        {
                            if (resChannel.ChannelID.HasValue)
                            {
                                channel.Id = resChannel.ChannelID.Value;

                                handleChannelMessages(resChannel.RecentMessages);
                                channel.MessagesLoaded = true; // this will mark the channel as having received messages even if there were none.
                            }
                        };

                        api.Queue(createRequest);
                        break;

                    default:
                        var req = new JoinChannelRequest(channel);
                        req.Success += () => joinChannel(channel, fetchInitialMessages);
                        req.Failure += ex => LeaveChannel(channel);
                        api.Queue(req);
                        return channel;
                }
            }
            else
            {
                if (fetchInitialMessages)
                    fetchInitalMessages(channel);
            }

            CurrentChannel.Value ??= channel;

            return channel;
        }

        public void LeaveChannel(Channel channel)
        {
            if (channel == null) return;

            if (channel == CurrentChannel.Value)
                CurrentChannel.Value = null;

            joinedChannels.Remove(channel);

            if (channel.Joined.Value)
            {
                api.Queue(new LeaveChannelRequest(channel));
                channel.Joined.Value = false;
            }
        }

        private long lastMessageId;

        private bool channelsInitialised;

        protected override Task Poll()
        {
            if (!api.IsLoggedIn)
                return base.Poll();

            var fetchReq = new GetUpdatesRequest(lastMessageId);

            var tcs = new TaskCompletionSource<bool>();

            fetchReq.Success += updates =>
            {
                if (updates?.Presence != null)
                {
                    foreach (var channel in updates.Presence)
                    {
                        // we received this from the server so should mark the channel already joined.
                        channel.Joined.Value = true;
                        joinChannel(channel);
                    }

                    //todo: handle left channels

                    handleChannelMessages(updates.Messages);

                    foreach (var group in updates.Messages.GroupBy(m => m.ChannelId))
                        JoinedChannels.FirstOrDefault(c => c.Id == group.Key)?.AddNewMessages(group.ToArray());

                    lastMessageId = updates.Messages.LastOrDefault()?.Id ?? lastMessageId;
                }

                if (!channelsInitialised)
                {
                    channelsInitialised = true;
                    // we want this to run after the first presence so we can see if the user is in any channels already.
                    initializeChannels();
                }

                tcs.SetResult(true);
            };

            fetchReq.Failure += _ => tcs.SetResult(false);

            api.Queue(fetchReq);

            return tcs.Task;
        }

        /// <summary>
        /// Marks the <paramref name="channel"/> as read
        /// </summary>
        /// <param name="channel">The channel that will be marked as read</param>
        public void MarkChannelAsRead(Channel channel)
        {
            if (channel.LastMessageId == channel.LastReadId)
                return;

            var message = channel.Messages.LastOrDefault();

            if (message == null)
                return;

            var req = new MarkChannelAsReadRequest(channel, message);

            req.Success += () => channel.LastReadId = message.Id;
            req.Failure += e => Logger.Error(e, $"Failed to mark channel {channel} up to '{message}' as read");

            api.Queue(req);
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
