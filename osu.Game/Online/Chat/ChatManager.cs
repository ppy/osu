// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    /// <summary>
    /// Manages everything chat related
    /// </summary>
    public sealed class ChatManager : IOnlineComponent
    {
        /// <summary>
        /// The channels the player joins on startup
        /// </summary>
        private readonly string[] defaultChannels =
        {
            @"#lazer", @"#osu", @"#lobby"
        };

        /// <summary>
        /// The currently opened chat
        /// </summary>
        public Bindable<ChatBase> CurrentChat { get; } = new Bindable<ChatBase>();
        /// <summary>
        /// The Channels the player has joined
        /// </summary>
        public ObservableCollection<ChannelChat> JoinedChannels { get; } = new ObservableCollection<ChannelChat>();
        /// <summary>
        /// The channels available for the player to join
        /// </summary>
        public ObservableCollection<ChannelChat> AvailableChannels { get; } = new ObservableCollection<ChannelChat>();
        /// <summary>
        /// The user chats opened.
        /// </summary>
        public ObservableCollection<UserChat> OpenedUserChats { get; } = new ObservableCollection<UserChat>();

        private APIAccess api;
        private readonly Scheduler scheduler;
        private ScheduledDelegate fetchMessagesScheduleder;
        private GetChannelMessagesRequest fetchChannelMsgReq;
        private GetUserMessagesRequest fetchUserMsgReq;
        private long? lastChannelMsgId;
        private long? lastUserMsgId;

        public ChatManager(Scheduler scheduler)
        {
            this.scheduler = scheduler ?? throw new ArgumentNullException(nameof(scheduler));
            CurrentChat.ValueChanged += currentChatChanged;
        }

        private void currentChatChanged(ChatBase chatBase)
        {
            if (chatBase is ChannelChat channel && !JoinedChannels.Contains(channel))
                JoinedChannels.Add(channel);
        }

        /// <summary>
        /// Posts a message to the currently opened chat.
        /// </summary>
        /// <param name="text">The message text that is going to be posted</param>
        /// <param name="isAction">Is true if the message is an action, e.g.: user is currently eating </param>
        public void PostMessage(string text, bool isAction = false)
        {
            if (CurrentChat.Value == null)
                return;

            if (!api.IsLoggedIn)
            {
                CurrentChat.Value.AddNewMessages(new ErrorMessage("Please sign in to participate in chat!"));
                return;
            }

            var message = new LocalEchoMessage
            {
                Sender = api.LocalUser.Value,
                Timestamp = DateTimeOffset.Now,
                TargetType = CurrentChat.Value.Target,
                TargetId = CurrentChat.Value.ChatID,
                IsAction = isAction,
                Content = text
            };

            CurrentChat.Value.AddLocalEcho(message);

            var req = new PostMessageRequest(message);
            req.Failure += e => CurrentChat.Value?.ReplaceMessage(message, null);
            req.Success += m => CurrentChat.Value?.ReplaceMessage(message, m);
            api.Queue(req);
        }

        public void PostCommand(string text)
        {
            if (CurrentChat.Value == null)
                return;

            var parameters = text.Split(new[] { ' ' }, 2);
            string command = parameters[0];
            string content = parameters.Length == 2 ? parameters[1] : string.Empty;

            switch (command)
            {
                case "me":
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        CurrentChat.Value.AddNewMessages(new ErrorMessage("Usage: /me [action]"));
                        break;
                    }
                    PostMessage(content, true);
                    break;

                case "help":
                    CurrentChat.Value.AddNewMessages(new InfoMessage("Supported commands: /help, /me [action]"));
                    break;

                default:
                    CurrentChat.Value.AddNewMessages(new ErrorMessage($@"""/{command}"" is not supported! For a list of supported commands see /help"));
                    break;
            }
        }

        private void fetchNewMessages()
        {
            if (fetchChannelMsgReq == null)
                fetchNewChannelMessages();

            if (fetchUserMsgReq == null)
                fetchNewUserMessages();
        }

        private void fetchNewUserMessages()
        {
            fetchUserMsgReq = new GetUserMessagesRequest(lastUserMsgId);

            fetchUserMsgReq.Success += messages =>
            {
                handleUserMessages(messages);
                lastUserMsgId = messages.LastOrDefault()?.Id ?? lastUserMsgId;
                fetchUserMsgReq = null;
            };
            fetchUserMsgReq.Failure += exception => Logger.Error(exception, "Fetching user messages failed.");

            api.Queue(fetchUserMsgReq);
        }

        private void handleUserMessages(IEnumerable<Message> messages)
        {
            var outgoingMessages = messages.Where(m => m.Sender.Id == api.LocalUser.Value.Id);
            var outgoingMessagesGroups = outgoingMessages.GroupBy(m => m.TargetId);
            var incomingMessagesGroups = messages.Except(outgoingMessages).GroupBy(m => m.UserId);

            foreach (var messageGroup in incomingMessagesGroups)
            {
                var targetUser = messageGroup.First().Sender;
                var chat = OpenedUserChats.FirstOrDefault(c => c.User.Id == targetUser.Id);

                if (chat == null)
                {
                    chat = new UserChat(targetUser);
                    OpenedUserChats.Add(chat);
                }

                chat.AddNewMessages(messageGroup.ToArray());
                var outgoingTargetMessages = outgoingMessagesGroups.FirstOrDefault(g => g.Key == targetUser.Id);
                chat.AddNewMessages(outgoingTargetMessages.ToArray());
            }

            var withoutReplyGroups = outgoingMessagesGroups.Where(g => OpenedUserChats.All(m => m.ChatID != g.Key));

            foreach (var withoutReplyGroup in withoutReplyGroups)
            {
                var getUserRequest = new GetUserRequest(withoutReplyGroup.First().TargetId);
                getUserRequest.Success += user =>
                {
                    var chat = new UserChat(user);

                    chat.AddNewMessages(withoutReplyGroup.ToArray());
                    OpenedUserChats.Add(chat);
                };

                api.Queue(getUserRequest);
            }
        }

        private void fetchNewChannelMessages()
        {
            fetchChannelMsgReq = new GetChannelMessagesRequest(JoinedChannels, lastChannelMsgId);

            fetchChannelMsgReq.Success += messages =>
            {
                if (messages == null)
                    return;
                handleChannelMessages(messages);
                lastChannelMsgId = messages.LastOrDefault()?.Id ?? lastChannelMsgId;
                fetchChannelMsgReq = null;
            };
            fetchChannelMsgReq.Failure += exception => Logger.Error(exception, "Fetching channel messages failed.");

            api.Queue(fetchChannelMsgReq);
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
                channels.Where(channel => AvailableChannels.All(c => c.ChatID != channel.ChatID))
                        .ForEach(channel => AvailableChannels.Add(channel));

                channels.Where(channel => defaultChannels.Contains(channel.Name))
                        .Where(channel => JoinedChannels.All(c => c.ChatID != channel.ChatID))
                        .ForEach(channel =>
                        {
                            JoinedChannels.Add(channel);
                            var fetchInitialMsgReq = new GetChannelMessagesRequest(new[] {channel}, null);
                            fetchInitialMsgReq.Success += handleChannelMessages;
                            api.Queue(fetchInitialMsgReq);
                        });

                fetchNewMessages();
            };
            req.Failure += error => Logger.Error(error, "Fetching channels failed");

            api.Queue(req);
        }

        public void APIStateChanged(APIAccess api, APIState state)
        {
            this.api = api ?? throw new ArgumentNullException(nameof(api));

            switch (state)
            {
                case APIState.Online:
                    if (JoinedChannels.Count == 0)
                        initializeDefaultChannels();
                    fetchMessagesScheduleder = scheduler.AddDelayed(fetchNewMessages, 1000, true);
                    break;
                default:
                    fetchChannelMsgReq?.Cancel();
                    fetchChannelMsgReq = null;
                    fetchMessagesScheduleder?.Cancel();

                    break;
            }
        }
    }
}
