// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using JetBrains.Annotations;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChatOverlayV2 : OsuManualInputManagerTestScene
    {
        private TestChatOverlayV2 chatOverlay;
        private ChannelManager channelManager;

        private APIUser testUser;
        private Channel testPMChannel;
        private Channel[] testChannels;

        private Channel testChannel1 => testChannels[0];
        private Channel testChannel2 => testChannels[1];

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            testUser = new APIUser { Username = "test user", Id = 5071479 };
            testPMChannel = new Channel(testUser);
            testChannels = Enumerable.Range(1, 10).Select(createPublicChannel).ToArray();

            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ChannelManager), channelManager = new ChannelManager()),
                },
                Children = new Drawable[]
                {
                    channelManager,
                    chatOverlay = new TestChatOverlayV2 { RelativeSizeAxes = Axes.Both },
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
                            getMessages.TriggerSuccess(createChannelMessages(getMessages.Channel));
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
            AddUntilStep("Channel is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel1);
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
            config.BindWith(OsuSetting.ChatDisplayHeight, configChatHeight);
            float newHeight = 0;

            AddStep("Reset config chat height", () => configChatHeight.SetDefault());
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
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddUntilStep("Channel 1 is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel1);
        }

        [Test]
        public void TestSearchInListing()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("Listing is visible", () => listingIsVisible);
            AddStep("Search for 'number 2'", () => chatOverlayTextBox.Text = "number 2");
            AddUntilStep("Only channel 2 visibile", () =>
            {
                IEnumerable<ChannelListingItem> listingItems = chatOverlay.ChildrenOfType<ChannelListingItem>()
                                                                          .Where(item => item.IsPresent);
                return listingItems.Count() == 1 && listingItems.Single().Channel == testChannel2;
            });
        }

        [Test]
        public void TestChannelCloseButton()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddStep("Join PM and public channels", () =>
            {
                channelManager.JoinChannel(testChannel1);
                channelManager.JoinChannel(testPMChannel);
            });
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
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
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
            Channel multiplayerChannel = null;

            AddStep("Show overlay", () => chatOverlay.Show());
            AddStep("Join multiplayer channel", () => channelManager.JoinChannel(multiplayerChannel = new Channel(new APIUser())
            {
                Name = "#mp_1",
                Type = ChannelType.Multiplayer,
            }));
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
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
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
            AddUntilStep("Channel 1 is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel1);
        }

        [Test]
        public void TestHighlightOnAnotherChannel()
        {
            Message message = null;

            AddStep("Show overlay", () => chatOverlay.Show());
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(testChannel2));
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
            AddUntilStep("Channel 2 is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel2);
        }

        [Test]
        public void TestHighlightOnLeftChannel()
        {
            Message message = null;

            AddStep("Show overlay", () => chatOverlay.Show());
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
            AddStep("Join channel 2", () => channelManager.JoinChannel(testChannel2));
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
            AddUntilStep("Channel 2 is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel2);
        }

        [Test]
        public void TestHighlightWhileChatNeverOpen()
        {
            Message message = null;

            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
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
            AddUntilStep("Channel 1 is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel1);
        }

        [Test]
        public void TestHighlightWithNullChannel()
        {
            Message message = null;

            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
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
            AddUntilStep("Channel 1 is visible", () => channelIsVisible && currentDrawableChannel.Channel == testChannel1);
        }

        [Test]
        public void TestTextBoxRetainsFocus()
        {
            AddStep("Show overlay", () => chatOverlay.Show());
            AddAssert("TextBox is focused", () => InputManager.FocusedDrawable == chatOverlayTextBox);
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
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
            AddStep("Join channel 1", () => channelManager.JoinChannel(testChannel1));
            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddAssert("Channel 1 loading", () => !channelIsVisible && chatOverlay.GetSlowLoadingChannel(testChannel1).LoadState == LoadState.Loading);

            AddStep("Join channel 2", () => channelManager.JoinChannel(testChannel2));
            AddStep("Select channel 2", () => clickDrawable(getChannelListItem(testChannel2)));
            AddAssert("Channel 2 loading", () => !channelIsVisible && chatOverlay.GetSlowLoadingChannel(testChannel2).LoadState == LoadState.Loading);

            AddStep("Finish channel 1 load", () => chatOverlay.GetSlowLoadingChannel(testChannel1).LoadEvent.Set());
            AddAssert("Channel 1 ready", () => chatOverlay.GetSlowLoadingChannel(testChannel1).LoadState == LoadState.Ready);
            AddAssert("Channel 1 not displayed", () => !channelIsVisible);

            AddStep("Finish channel 2 load", () => chatOverlay.GetSlowLoadingChannel(testChannel2).LoadEvent.Set());
            AddAssert("Channel 2 loaded", () => chatOverlay.GetSlowLoadingChannel(testChannel2).IsLoaded);
            AddAssert("Channel 2 displayed", () => channelIsVisible && currentDrawableChannel.Channel == testChannel2);

            AddStep("Select channel 1", () => clickDrawable(getChannelListItem(testChannel1)));
            AddAssert("Channel 1 loaded", () => chatOverlay.GetSlowLoadingChannel(testChannel1).IsLoaded);
            AddAssert("Channel 1 displayed", () => channelIsVisible && currentDrawableChannel.Channel == testChannel1);
        }

        private bool listingIsVisible =>
            chatOverlay.ChildrenOfType<ChannelListing>().Single().State.Value == Visibility.Visible;

        private bool loadingIsVisible =>
            chatOverlay.ChildrenOfType<LoadingLayer>().Single().State.Value == Visibility.Visible;

        private bool channelIsVisible =>
            !listingIsVisible && !loadingIsVisible;

        private DrawableChannel currentDrawableChannel =>
            chatOverlay.ChildrenOfType<DrawableChannel>().Single();

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
            var message = new Message
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

        private class TestChatOverlayV2 : ChatOverlayV2
        {
            public bool SlowLoading { get; set; }

            public SlowLoadingDrawableChannel GetSlowLoadingChannel(Channel channel) => DrawableChannels.OfType<SlowLoadingDrawableChannel>().Single(c => c.Channel == channel);

            protected override ChatOverlayDrawableChannel CreateDrawableChannel(Channel newChannel)
            {
                return SlowLoading
                    ? new SlowLoadingDrawableChannel(newChannel)
                    : new ChatOverlayDrawableChannel(newChannel);
            }
        }

        private class SlowLoadingDrawableChannel : ChatOverlayDrawableChannel
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
