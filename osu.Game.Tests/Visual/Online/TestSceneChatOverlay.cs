// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Overlays.Chat.Listing;
using osu.Game.Overlays.Chat.ChannelList;
using osuTK;
using osuTK.Input;
using osu.Game.Graphics.UserInterfaceV2;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChatOverlay : OsuManualInputManagerTestScene
    {
        private TestChatOverlay chatOverlay;
        private ChannelManager channelManager;

        private readonly APIUser testUser = new APIUser { Username = "test user", Id = 5071479 };
        private readonly APIUser testUser1 = new APIUser { Username = "test user", Id = 5071480 };

        private Channel[] testChannels;
        private Message[] initialMessages;

        private Channel testChannel1 => testChannels[0];
        private Channel testChannel2 => testChannels[1];

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private int currentMessageId;

        private DummyAPIAccess dummyAPI => (DummyAPIAccess)API;
        private readonly ManualResetEventSlim requestLock = new ManualResetEventSlim();

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            currentMessageId = 0;
            testChannels = Enumerable.Range(1, 10).Select(createPublicChannel).ToArray();
            initialMessages = testChannels.SelectMany(createChannelMessages).ToArray();

            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ChannelManager), channelManager = new ChannelManager(API)),
                },
                Children = new Drawable[]
                {
                    channelManager,
                    chatOverlay = new TestChatOverlay(),
                },
            };
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Setup request handler", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case CreateChannelRequest createRequest:
                            createRequest.TriggerSuccess(new APIChatChannel
                            {
                                ChannelID = ((int)createRequest.Channel.Id),
                                RecentMessages = new List<Message>()
                            });
                            return true;

                        case GetUpdatesRequest getUpdates:
                            getUpdates.TriggerFailure(new WebException());
                            return true;

                        case JoinChannelRequest joinChannel:
                            joinChannel.TriggerSuccess();
                            return true;

                        case LeaveChannelRequest leaveChannel:
                            leaveChannel.TriggerSuccess();
                            return true;

                        case GetMessagesRequest getMessages:
                            getMessages.TriggerSuccess(initialMessages.ToList());
                            return true;

                        case GetUserRequest getUser:
                            if (getUser.Lookup == testUser.Username)
                                getUser.TriggerSuccess(testUser);
                            else
                                getUser.TriggerFailure(new WebException());
                            return true;

                        case PostMessageRequest postMessage:
                            postMessage.TriggerSuccess(new Message(RNG.Next(0, 10000000))
                            {
                                Content = postMessage.Message.Content,
                                ChannelId = postMessage.Message.ChannelId,
                                Sender = postMessage.Message.Sender,
                                Timestamp = new DateTimeOffset(DateTime.Now),
                            });
                            return true;

                        default:
                            Logger.Log($"Unhandled Request Type: {req.GetType()}");
                            return false;
                    }
                };
            });

            AddStep("Add test channels", () =>
            {
                (channelManager.AvailableChannels as BindableList<Channel>)?.AddRange(testChannels);
            });
        }

        [Test]
        public void TestBasic()
        {
            AddStep("Show overlay with channel", () =>
            {
                chatOverlay.Show();
                Channel joinedChannel = channelManager.JoinChannel(testChannel1);
                channelManager.CurrentChannel.Value = joinedChannel;
            });
            AddAssert("Overlay is visible", () => chatOverlay.State.Value == Visibility.Visible);
            waitForChannel1Visible();
        }

        [Test]
        public void TestShowHide()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("Overlay is visible", () => chatOverlay.State.Value == Visibility.Visible);
            AddStep("Hide overlay", () => chatOverlay.Hide());
            AddAssert("Overlay is hidden", () => chatOverlay.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestChatHeight()
        {
            BindableFloat configChatHeight = new BindableFloat();

            float newHeight = 0;

            AddStep("Reset config chat height", () =>
            {
                config.BindWith(OsuSetting.ChatDisplayHeight, configChatHeight);
                configChatHeight.SetDefault();
            });
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("Overlay uses config height", () => chatOverlay.Height == configChatHeight.Default);
            AddStep("Click top bar", () =>
            {
                InputManager.MoveMouseTo(chatOverlayTopBar);
                InputManager.PressButton(MouseButton.Left);
            });
            AddStep("Drag overlay to new height", () => InputManager.MoveMouseTo(chatOverlayTopBar, new Vector2(0, -300)));
            AddStep("Stop dragging", () => InputManager.ReleaseButton(MouseButton.Left));
            AddStep("Store new height", () => newHeight = chatOverlay.Height);
            AddAssert("Config height changed", () => !configChatHeight.IsDefault && configChatHeight.Value == newHeight);
            AddStep("Hide overlay", () => chatOverlay.Hide());
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("Overlay uses new height", () => chatOverlay.Height == newHeight);
        }

        [Test]
        public void TestChannelSelection()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("Listing is visible", () => listingIsVisible);
            joinTestChannel(0);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            waitForChannel1Visible();
        }

        [Test]
        public void TestSearchInListing()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("Listing is visible", () => listingIsVisible);
            AddStep("Search for 'number 2'", () => chatOverlayTextBox.Text = "number 2");
            AddUntilStep("Only channel 2 visible", () =>
            {
                IEnumerable<ChannelListingItem> listingItems = chatOverlay.ChildrenOfType<ChannelListingItem>()
                                                                          .Where(item => item.IsPresent);
                return listingItems.Count() == 1 && listingItems.Single().Channel == testChannel2;
            });
        }

        [Test]
        public void TestChannelCloseButton()
        {
            var testPMChannel = new Channel(testUser);

            AddStep("Show overlay", () => chatOverlay.Show());
            joinTestChannel(0);
            joinChannel(testPMChannel);
            AddStep("Select PM channel", () => clickDrawable(getChannelListItem(testPMChannel)));
            AddStep("Click close button", () =>
            {
                ChannelListItemCloseButton closeButton = getChannelListItem(testPMChannel).ChildrenOfType<ChannelListItemCloseButton>().Single();
                clickDrawable(closeButton);
            });
            AddAssert("PM channel closed", () => !channelManager.JoinedChannels.Contains(testPMChannel));
            AddStep("Select normal channel", () => clickDrawable(getChannelListItem(testChannel1)));
            AddStep("Click close button", () =>
            {
                ChannelListItemCloseButton closeButton = getChannelListItem(testChannel1).ChildrenOfType<ChannelListItemCloseButton>().Single();
                clickDrawable(closeButton);
            });
            AddAssert("Normal channel closed", () => !channelManager.JoinedChannels.Contains(testChannel1));
        }

        [Test]
        public void TestChatCommand()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            joinTestChannel(0);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddStep("Open chat with user", () => channelManager.PostCommand($"chat {testUser.Username}"));
            AddAssert("PM channel is selected", () =>
                channelManager.CurrentChannel.Value.Type == ChannelType.PM && channelManager.CurrentChannel.Value.Users.Single() == testUser);
            AddStep("Open chat with non-existent user", () => channelManager.PostCommand("chat user_doesnt_exist"));
            AddAssert("Last message is error", () => channelManager.CurrentChannel.Value.Messages.Last() is ErrorMessage);

            // Make sure no unnecessary requests are made when the PM channel is already open.
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddStep("Unregister request handling", () => ((DummyAPIAccess)API).HandleRequest = null);
            AddStep("Open chat with user", () => channelManager.PostCommand($"chat {testUser.Username}"));
            AddAssert("PM channel is selected", () =>
                channelManager.CurrentChannel.Value.Type == ChannelType.PM && channelManager.CurrentChannel.Value.Users.Single() == testUser);
        }

        [Test]
        public void TestMultiplayerChannelIsNotShown()
        {
            Channel multiplayerChannel;

            AddStep("Show overlay", () => chatOverlay.Show());

            joinChannel(multiplayerChannel = new Channel(new APIUser())
            {
                Name = "#mp_1",
                Type = ChannelType.Multiplayer,
            });

            AddAssert("Channel is joined", () => channelManager.JoinedChannels.Contains(multiplayerChannel));
            AddUntilStep("Channel not present in listing", () => !chatOverlay.ChildrenOfType<ChannelListingItem>()
                                                                             .Where(item => item.IsPresent)
                                                                             .Select(item => item.Channel)
                                                                             .Contains(multiplayerChannel));
        }

        [Test]
        public void TestHighlightOnCurrentChannel()
        {
            Message message = null;

            AddStep("Show overlay", () => chatOverlay.Show());
            joinTestChannel(0);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddStep("Send message in channel 1", () =>
            {
                testChannel1.AddNewMessages(message = new Message
                {
                    ChannelId = testChannel1.Id,
                    Content = "Message to highlight!",
                    Timestamp = DateTimeOffset.Now,
                    Sender = testUser,
                });
            });
            AddStep("Highlight message", () => chatOverlay.HighlightMessage(message, testChannel1));
            waitForChannel1Visible();
        }

        [Test]
        public void TestHighlightOnAnotherChannel()
        {
            Message message = null;

            AddStep("Show overlay", () => chatOverlay.Show());
            joinTestChannel(0);
            joinTestChannel(1);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddStep("Send message in channel 2", () =>
            {
                testChannel2.AddNewMessages(message = new Message
                {
                    ChannelId = testChannel2.Id,
                    Content = "Message to highlight!",
                    Timestamp = DateTimeOffset.Now,
                    Sender = testUser,
                });
            });
            AddStep("Highlight message", () => chatOverlay.HighlightMessage(message, testChannel2));
            waitForChannel2Visible();
        }

        [Test]
        public void TestHighlightOnLeftChannel()
        {
            Message message = null;

            AddStep("Show overlay", () => chatOverlay.Show());
            joinTestChannel(0);
            joinTestChannel(1);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddStep("Send message in channel 2", () =>
            {
                testChannel2.AddNewMessages(message = new Message
                {
                    ChannelId = testChannel2.Id,
                    Content = "Message to highlight!",
                    Timestamp = DateTimeOffset.Now,
                    Sender = testUser,
                });
            });
            AddStep("Leave channel 2", () => channelManager.LeaveChannel(testChannel2));
            AddStep("Highlight message", () => chatOverlay.HighlightMessage(message, testChannel2));
            waitForChannel2Visible();
        }

        [Test]
        public void TestHighlightWhileChatNeverOpen()
        {
            Message message = null;

            joinTestChannel(0);
            AddStep("Send message in channel 1", () =>
            {
                testChannel1.AddNewMessages(message = new Message
                {
                    ChannelId = testChannel1.Id,
                    Content = "Message to highlight!",
                    Timestamp = DateTimeOffset.Now,
                    Sender = testUser,
                });
            });
            AddStep("Highlight message", () => chatOverlay.HighlightMessage(message, testChannel1));
            waitForChannel1Visible();
        }

        [Test]
        public void TestHighlightWithNullChannel()
        {
            Message message = null;

            joinTestChannel(0);
            AddStep("Send message in channel 1", () =>
            {
                testChannel1.AddNewMessages(message = new Message
                {
                    ChannelId = testChannel1.Id,
                    Content = "Message to highlight!",
                    Timestamp = DateTimeOffset.Now,
                    Sender = testUser,
                });
            });
            AddStep("Set null channel", () => channelManager.CurrentChannel.Value = null);
            AddStep("Highlight message", () => chatOverlay.HighlightMessage(message, testChannel1));
            waitForChannel1Visible();
        }

        [Test]
        public void TestTextBoxRetainsFocus()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            joinTestChannel(0);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            waitForChannel1Visible();
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Click drawable channel", () => clickDrawable(currentDrawableChannel));
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Click selector", () => clickDrawable(channelSelectorButton));
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Click listing", () => clickDrawable(chatOverlay.ChildrenOfType<ChannelListing>().Single()));
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Click channel list", () => clickDrawable(chatOverlay.ChildrenOfType<ChannelList>().Single()));
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Click top bar", () => clickDrawable(chatOverlay.ChildrenOfType<ChatOverlayTopBar>().Single()));
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Hide overlay", () => chatOverlay.Hide());
            AddAssert("TextBox is not focused", () => InputManager.FocusedDrawable == null);
        }

        [Test]
        public void TestSlowLoadingChannel()
        {
            AddStep("Show overlay (slow-loading)", () =>
            {
                chatOverlay.Show();
                chatOverlay.SlowLoading = true;
            });
            joinTestChannel(0);
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddUntilStep("Channel 1 loading", () => !channelIsVisible && chatOverlay.GetSlowLoadingChannel(testChannel1).LoadState == LoadState.Loading);

            joinTestChannel(1);
            AddStep("Select channel 2", () => clickDrawable(getChannelListItem(testChannel2)));
            AddUntilStep("Channel 2 loading", () => !channelIsVisible && chatOverlay.GetSlowLoadingChannel(testChannel2).LoadState == LoadState.Loading);

            AddStep("Finish channel 1 load", () => chatOverlay.GetSlowLoadingChannel(testChannel1).LoadEvent.Set());
            AddUntilStep("Channel 1 ready", () => chatOverlay.GetSlowLoadingChannel(testChannel1).LoadState == LoadState.Ready);
            AddAssert("Channel 1 not displayed", () => !channelIsVisible);

            AddStep("Finish channel 2 load", () => chatOverlay.GetSlowLoadingChannel(testChannel2).LoadEvent.Set());
            AddUntilStep("Channel 2 loaded", () => chatOverlay.GetSlowLoadingChannel(testChannel2).IsLoaded);
            waitForChannel2Visible();

            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddUntilStep("Channel 1 loaded", () => chatOverlay.GetSlowLoadingChannel(testChannel1).IsLoaded);
            waitForChannel1Visible();
        }

        [Test]
        public void TestKeyboardCloseAndRestoreChannel()
        {
            AddStep("Show overlay with channel 1", () =>
            {
                channelManager.JoinChannel(testChannel1);
                chatOverlay.Show();
            });
            waitForChannel1Visible();
            AddStep("Press document close keys", () => InputManager.Keys(PlatformAction.DocumentClose));
            AddAssert("Listing is visible", () => listingIsVisible);

            AddStep("Press tab restore keys", () => InputManager.Keys(PlatformAction.TabRestore));
            waitForChannel1Visible();
        }

        [Test]
        public void TestKeyboardNewChannel()
        {
            AddStep("Show overlay with channel 1", () =>
            {
                channelManager.JoinChannel(testChannel1);
                chatOverlay.Show();
            });
            waitForChannel1Visible();
            AddStep("Press tab new keys", () => InputManager.Keys(PlatformAction.TabNew));
            AddAssert("Listing is visible", () => listingIsVisible);
        }

        [Test]
        public void TestKeyboardNextChannel()
        {
            Channel announceChannel = createAnnounceChannel();
            Channel pmChannel1 = createPrivateChannel();
            Channel pmChannel2 = createPrivateChannel();

            joinTestChannel(0);
            joinTestChannel(1);
            joinChannel(pmChannel1);
            joinChannel(pmChannel2);
            joinChannel(announceChannel);

            AddStep("Show overlay", () => chatOverlay.Show());

            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            waitForChannel1Visible();

            AddStep("Press document next keys", () => InputManager.Keys(PlatformAction.DocumentNext));
            waitForChannel2Visible();

            AddStep("Press document next keys", () => InputManager.Keys(PlatformAction.DocumentNext));
            AddUntilStep("PM Channel 1 displayed", () => channelIsVisible && currentDrawableChannel?.Channel == pmChannel1);

            AddStep("Press document next keys", () => InputManager.Keys(PlatformAction.DocumentNext));
            AddUntilStep("PM Channel 2 displayed", () => channelIsVisible && currentDrawableChannel?.Channel == pmChannel2);

            AddStep("Press document next keys", () => InputManager.Keys(PlatformAction.DocumentNext));
            AddUntilStep("Announce channel displayed", () => channelIsVisible && currentDrawableChannel?.Channel == announceChannel);

            AddStep("Press document next keys", () => InputManager.Keys(PlatformAction.DocumentNext));
            waitForChannel1Visible();
        }

        [Test]
        public void TestRemoveMessages()
        {
            AddStep("Show overlay with channel", () =>
            {
                chatOverlay.Show();
                channelManager.CurrentChannel.Value = channelManager.JoinChannel(testChannel1);
            });

            AddAssert("Overlay is visible", () => chatOverlay.State.Value == Visibility.Visible);
            waitForChannel1Visible();

            AddStep("Send message from another user", () =>
            {
                testChannel1.AddNewMessages(new Message
                {
                    ChannelId = testChannel1.Id,
                    Content = "Message from another user",
                    Timestamp = DateTimeOffset.Now,
                    Sender = testUser1,
                });
            });

            AddStep("Remove messages from other user", () =>
            {
                testChannel1.RemoveMessagesFromUser(testUser.Id);
            });
        }

        [Test]
        public void TestTextBoxSavePerChannel()
        {
            var testPMChannel = new Channel(testUser);

            AddStep("show overlay", () => chatOverlay.Show());
            joinTestChannel(0);
            joinChannel(testPMChannel);

            AddAssert("listing is visible", () => listingIsVisible);
            AddStep("search for 'number 2'", () => chatOverlayTextBox.Text = "number 2");
            AddAssert("'number 2' saved to selector", () => channelManager.CurrentChannel.Value.TextBoxMessage.Value == "number 2");

            AddStep("select normal channel", () => clickDrawable(getChannelListItem(testChannel1)));
            AddAssert("text box cleared on normal channel", () => chatOverlayTextBox.Text == string.Empty);
            AddAssert("nothing saved on normal channel", () => channelManager.CurrentChannel.Value.TextBoxMessage.Value == string.Empty);
            AddStep("type '727'", () => chatOverlayTextBox.Text = "727");
            AddAssert("'727' saved to normal channel", () => channelManager.CurrentChannel.Value.TextBoxMessage.Value == "727");

            AddStep("select PM channel", () => clickDrawable(getChannelListItem(testPMChannel)));
            AddAssert("text box cleared on PM channel", () => chatOverlayTextBox.Text == string.Empty);
            AddAssert("nothing saved on PM channel", () => channelManager.CurrentChannel.Value.TextBoxMessage.Value == string.Empty);
            AddStep("type 'hello'", () => chatOverlayTextBox.Text = "hello");
            AddAssert("'hello' saved to PM channel", () => channelManager.CurrentChannel.Value.TextBoxMessage.Value == "hello");

            AddStep("select normal channel", () => clickDrawable(getChannelListItem(testChannel1)));
            AddAssert("text box contains '727'", () => chatOverlayTextBox.Text == "727");

            AddStep("select PM channel", () => clickDrawable(getChannelListItem(testPMChannel)));
            AddAssert("text box contains 'hello'", () => chatOverlayTextBox.Text == "hello");
            AddStep("click close button", () =>
            {
                ChannelListItemCloseButton closeButton = getChannelListItem(testPMChannel).ChildrenOfType<ChannelListItemCloseButton>().Single();
                clickDrawable(closeButton);
            });

            AddAssert("listing is visible", () => listingIsVisible);
            AddAssert("text box contains 'channel 2'", () => chatOverlayTextBox.Text == "number 2");
            AddUntilStep("only channel 2 visible", () =>
            {
                IEnumerable<ChannelListingItem> listingItems = chatOverlay.ChildrenOfType<ChannelListingItem>()
                                                                          .Where(item => item.IsPresent);
                return listingItems.Count() == 1 && listingItems.Single().Channel == testChannel2;
            });
        }

        [Test]
        public void TestChatReport()
        {
            ChatReportRequest request = null;

            AddStep("Show overlay with channel", () =>
            {
                chatOverlay.Show();
                channelManager.CurrentChannel.Value = channelManager.JoinChannel(testChannel1);
            });

            AddAssert("Overlay is visible", () => chatOverlay.State.Value == Visibility.Visible);
            waitForChannel1Visible();

            AddStep("Setup request handling", () =>
            {
                requestLock.Reset();

                dummyAPI.HandleRequest = r =>
                {
                    if (!(r is ChatReportRequest req))
                        return false;

                    Task.Run(() =>
                    {
                        request = req;
                        requestLock.Wait(10000);
                        req.TriggerSuccess();
                    });

                    return true;
                };
            });

            AddStep("Show report popover", () => this.ChildrenOfType<ChatLine>().First().ShowPopover());

            AddStep("Set report reason to other", () =>
            {
                var reason = this.ChildrenOfType<OsuEnumDropdown<ChatReportReason>>().Single();
                reason.Current.Value = ChatReportReason.Other;
            });

            AddStep("Try to report", () =>
            {
                var btn = this.ChildrenOfType<ReportChatPopover>().Single().ChildrenOfType<RoundedButton>().Single();
                InputManager.MoveMouseTo(btn);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Nothing happened", () => this.ChildrenOfType<ReportChatPopover>().Any());
            AddStep("Set report data", () =>
            {
                var field = this.ChildrenOfType<ReportChatPopover>().Single().ChildrenOfType<OsuTextBox>().Single();
                field.Current.Value = "test other";
            });

            AddStep("Try to report", () =>
            {
                var btn = this.ChildrenOfType<ReportChatPopover>().Single().ChildrenOfType<RoundedButton>().Single();
                InputManager.MoveMouseTo(btn);
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Overlay closed", () => !this.ChildrenOfType<ReportChatPopover>().Any());
            AddStep("Complete request", () => requestLock.Set());
            AddUntilStep("Request sent", () => request != null);
            AddUntilStep("Info message displayed", () => channelManager.CurrentChannel.Value.Messages.Last(), () => Is.InstanceOf(typeof(InfoMessage)));
        }

        private void joinTestChannel(int i)
        {
            AddStep($"Join test channel {i}", () => channelManager.JoinChannel(testChannels[i]));
            AddUntilStep("wait for join completed", () => testChannels[i].Joined.Value);
        }

        private void joinChannel(Channel channel)
        {
            AddStep($"Join channel {channel}", () => channelManager.JoinChannel(channel));
            AddUntilStep("wait for join completed", () => channel.Joined.Value);
        }

        private void waitForChannel1Visible() =>
            AddUntilStep("Channel 1 is visible", () => channelIsVisible && currentDrawableChannel?.Channel == testChannel1);

        private void waitForChannel2Visible() =>
            AddUntilStep("Channel 2 is visible", () => channelIsVisible && currentDrawableChannel?.Channel == testChannel2);

        private bool listingIsVisible =>
            chatOverlay.ChildrenOfType<ChannelListing>().Single().State.Value == Visibility.Visible;

        private bool loadingIsVisible =>
            chatOverlay.ChildrenOfType<LoadingLayer>().Single().State.Value == Visibility.Visible;

        private bool channelIsVisible =>
            !listingIsVisible && !loadingIsVisible;

        [CanBeNull]
        private DrawableChannel currentDrawableChannel =>
            chatOverlay.ChildrenOfType<DrawableChannel>().SingleOrDefault();

        private ChannelListItem getChannelListItem(Channel channel) =>
            chatOverlay.ChildrenOfType<ChannelListItem>().Single(item => item.Channel == channel);

        private ChatTextBox chatOverlayTextBox =>
            chatOverlay.ChildrenOfType<ChatTextBox>().Single();

        private ChatOverlayTopBar chatOverlayTopBar =>
            chatOverlay.ChildrenOfType<ChatOverlayTopBar>().Single();

        private ChannelListItem channelSelectorButton =>
            chatOverlay.ChildrenOfType<ChannelListItem>().Single(item => item.Channel is ChannelListing.ChannelListingChannel);

        private void clickDrawable(Drawable d)
        {
            InputManager.MoveMouseTo(d);
            InputManager.Click(MouseButton.Left);
        }

        private List<Message> createChannelMessages(Channel channel)
        {
            var message = new Message(currentMessageId++)
            {
                ChannelId = channel.Id,
                Content = $"Hello, this is a message in {channel.Name}",
                Sender = testUser,
                Timestamp = new DateTimeOffset(DateTime.Now),
            };
            return new List<Message> { message };
        }

        private Channel createPublicChannel(int id) => new Channel
        {
            Id = id,
            Name = $"#channel-{id}",
            Topic = $"We talk about the number {id} here",
            Type = ChannelType.Public,
        };

        private Channel createPrivateChannel()
        {
            int id = RNG.Next(0, DummyAPIAccess.DUMMY_USER_ID - 1);
            return new Channel(new APIUser
            {
                Id = id,
                Username = $"test user {id}",
            });
        }

        private Channel createAnnounceChannel()
        {
            const int announce_channel_id = 133337;

            return new Channel
            {
                Name = $"Announce {announce_channel_id}",
                Type = ChannelType.Announce,
                Id = announce_channel_id,
            };
        }

        private partial class TestChatOverlay : ChatOverlay
        {
            public bool SlowLoading { get; set; }

            public SlowLoadingDrawableChannel GetSlowLoadingChannel(Channel channel) => DrawableChannels.OfType<SlowLoadingDrawableChannel>().Single(c => c.Channel == channel);

            protected override DrawableChannel CreateDrawableChannel(Channel newChannel)
            {
                return SlowLoading
                    ? new SlowLoadingDrawableChannel(newChannel)
                    : new DrawableChannel(newChannel);
            }
        }

        private partial class SlowLoadingDrawableChannel : DrawableChannel
        {
            public readonly ManualResetEventSlim LoadEvent = new ManualResetEventSlim();

            public SlowLoadingDrawableChannel([NotNull] Channel channel)
                : base(channel)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadEvent.Wait(10000);
            }
        }
    }
}
