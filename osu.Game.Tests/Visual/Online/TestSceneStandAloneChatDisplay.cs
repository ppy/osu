// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Chat;
using osuTK;
using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.Chat;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneStandAloneChatDisplay : OsuManualInputManagerTestScene
    {
        private readonly APIUser admin = new APIUser
        {
            Username = "HappyStick",
            Id = 2,
            Colour = "f2ca34"
        };

        private readonly APIUser redUser = new APIUser
        {
            Username = "BanchoBot",
            Id = 3,
        };

        private readonly APIUser blueUser = new APIUser
        {
            Username = "Zallius",
            Id = 4,
        };

        private readonly APIUser longUsernameUser = new APIUser
        {
            Username = "Very Long Long Username",
            Id = 5,
        };

        private ChannelManager channelManager;

        private TestStandAloneChatDisplay chatDisplay;
        private TestStandAloneChatDisplay chatWithTextBox;
        private TestStandAloneChatDisplay chatWithTextBox2;
        private int messageIdSequence;

        private Channel testChannel;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var api = parent.Get<IAPIProvider>();

            Add(channelManager = new ChannelManager(api));

            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(channelManager);

            return dependencies;
        }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            messageIdSequence = 0;
            channelManager.CurrentChannel.Value = testChannel = new Channel();

            reinitialiseDrawableDisplay();
        });

        private void reinitialiseDrawableDisplay()
        {
            Children = new Drawable[]
            {
                chatDisplay = new TestStandAloneChatDisplay
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding(20),
                    Size = new Vector2(400, 80),
                    Channel = { Value = testChannel },
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Margin = new MarginPadding(20),
                    Children = new[]
                    {
                        chatWithTextBox = new TestStandAloneChatDisplay(true)
                        {
                            Margin = new MarginPadding(20),
                            Size = new Vector2(400, 150),
                            Channel = { Value = testChannel },
                        },
                        chatWithTextBox2 = new TestStandAloneChatDisplay(true)
                        {
                            Margin = new MarginPadding(20),
                            Size = new Vector2(400, 150),
                            Channel = { Value = testChannel },
                        },
                    }
                }
            };
        }

        [Test]
        public void TestSystemMessageOrdering()
        {
            var standardMessage = new Message(messageIdSequence++)
            {
                Timestamp = DateTimeOffset.Now,
                Sender = admin,
                Content = "I am a wang!"
            };

            var infoMessage1 = new InfoMessage($"the system is calling {messageIdSequence++}");
            var infoMessage2 = new InfoMessage($"the system is calling {messageIdSequence++}");

            var standardMessage2 = new Message(messageIdSequence++)
            {
                Timestamp = DateTimeOffset.Now,
                Sender = admin,
                Content = "I am a wang!"
            };

            AddStep("message from admin", () => testChannel.AddNewMessages(standardMessage));
            AddStep("message from system", () => testChannel.AddNewMessages(infoMessage1));
            AddStep("message from system", () => testChannel.AddNewMessages(infoMessage2));
            AddStep("message from admin", () => testChannel.AddNewMessages(standardMessage2));

            AddAssert("count is correct", () => testChannel.Messages.Count, () => Is.EqualTo(4));

            AddAssert("message order is correct", () => testChannel.Messages, () => Is.EqualTo(new[]
            {
                standardMessage,
                infoMessage1,
                infoMessage2,
                standardMessage2
            }));

            AddAssert("displayed order is correct", () => chatDisplay.DrawableChannel.ChildrenOfType<ChatLine>().Select(c => c.Message), () => Is.EqualTo(new[]
            {
                standardMessage,
                infoMessage1,
                infoMessage2,
                standardMessage2
            }));

            AddStep("reinit drawable channel", reinitialiseDrawableDisplay);

            AddAssert("displayed order is still correct", () => chatDisplay.DrawableChannel.ChildrenOfType<ChatLine>().Select(c => c.Message), () => Is.EqualTo(new[]
            {
                standardMessage,
                infoMessage1,
                infoMessage2,
                standardMessage2
            }));
        }

        [Test]
        public void TestManyMessages()
        {
            sendRegularMessages();
            checkScrolledToBottom();

            const int messages_per_call = 10;
            AddRepeatStep("add many messages", () =>
            {
                for (int i = 0; i < messages_per_call; i++)
                {
                    testChannel.AddNewMessages(new Message(messageIdSequence++)
                    {
                        Sender = longUsernameUser,
                        Content = "Many messages! " + Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now
                    });
                }
            }, Channel.MAX_HISTORY / messages_per_call + 5);

            AddAssert("Ensure no adjacent day separators", () =>
            {
                var indices = chatDisplay.FillFlow.OfType<DaySeparator>().Select(ds => chatDisplay.FillFlow.IndexOf(ds));

                foreach (int i in indices)
                {
                    if (i < chatDisplay.FillFlow.Count && chatDisplay.FillFlow[i + 1] is DaySeparator)
                        return false;
                }

                return true;
            });

            checkScrolledToBottom();
        }

        [Test]
        public void TestMessageHighlighting()
        {
            Message highlighted = null;

            sendRegularMessages();

            AddStep("highlight first message", () =>
            {
                highlighted = testChannel.Messages[0];
                testChannel.HighlightedMessage.Value = highlighted;
            });

            AddUntilStep("chat scrolled to first message", () =>
            {
                var line = chatDisplay.ChildrenOfType<ChatLine>().Single(c => c.Message == highlighted);
                return chatDisplay.ScrollContainer.ScreenSpaceDrawQuad.Contains(line.ScreenSpaceDrawQuad.Centre);
            });

            sendMessage();
            checkNotScrolledToBottom();

            AddStep("highlight last message", () =>
            {
                highlighted = testChannel.Messages[^1];
                testChannel.HighlightedMessage.Value = highlighted;
            });

            AddUntilStep("chat scrolled to last message", () =>
            {
                var line = chatDisplay.ChildrenOfType<ChatLine>().Single(c => c.Message == highlighted);
                return chatDisplay.ScrollContainer.ScreenSpaceDrawQuad.Contains(line.ScreenSpaceDrawQuad.Centre);
            });

            sendMessage();
            checkScrolledToBottom();

            AddRepeatStep("highlight other random messages", () =>
            {
                highlighted = testChannel.Messages[RNG.Next(0, testChannel.Messages.Count - 1)];
                testChannel.HighlightedMessage.Value = highlighted;
            }, 10);
        }

        [Test]
        public void TestMessageHighlightingOnFilledChat()
        {
            int index = 0;

            fillChat(100);

            AddStep("highlight first message", () => testChannel.HighlightedMessage.Value = testChannel.Messages[index = 0]);
            AddStep("highlight next message", () => testChannel.HighlightedMessage.Value = testChannel.Messages[index = Math.Min(index + 1, testChannel.Messages.Count - 1)]);
            AddStep("highlight last message", () => testChannel.HighlightedMessage.Value = testChannel.Messages[index = testChannel.Messages.Count - 1]);
            AddStep("highlight previous message", () => testChannel.HighlightedMessage.Value = testChannel.Messages[index = Math.Max(index - 1, 0)]);
            AddRepeatStep("highlight random messages", () => testChannel.HighlightedMessage.Value = testChannel.Messages[index = RNG.Next(0, testChannel.Messages.Count - 1)], 10);
        }

        /// <summary>
        /// Tests that when a message gets wrapped by the chat display getting contracted while scrolled to bottom, the chat will still keep scrolling down.
        /// </summary>
        [Test]
        public void TestMessageWrappingKeepsAutoScrolling()
        {
            fillChat();

            // send message with short words for text wrapping to occur when contracting chat.
            sendMessage();

            AddStep("contract chat", () => chatDisplay.Width -= 100);
            checkScrolledToBottom();

            AddStep("send another message", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = admin,
                Content = "As we were saying...",
            }));

            checkScrolledToBottom();
        }

        [Test]
        public void TestOverrideChatScrolling()
        {
            fillChat();

            sendMessage();
            checkScrolledToBottom();

            AddStep("Scroll to start", () => chatDisplay.ScrollContainer.ScrollToStart());

            checkNotScrolledToBottom();
            sendMessage();
            checkNotScrolledToBottom();

            AddStep("Scroll to bottom", () => chatDisplay.ScrollContainer.ScrollToEnd());

            checkScrolledToBottom();
            sendMessage();
            checkScrolledToBottom();
        }

        [Test]
        public void TestOverrideChatScrollingByUser()
        {
            fillChat();

            sendMessage();
            checkScrolledToBottom();

            AddStep("User scroll up", () =>
            {
                InputManager.MoveMouseTo(chatDisplay.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(chatDisplay.ScreenSpaceDrawQuad.Centre + new Vector2(0, chatDisplay.ScreenSpaceDrawQuad.Height));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            checkNotScrolledToBottom();
            sendMessage();
            checkNotScrolledToBottom();

            AddRepeatStep("User scroll to bottom", () =>
            {
                InputManager.MoveMouseTo(chatDisplay.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(chatDisplay.ScreenSpaceDrawQuad.Centre - new Vector2(0, chatDisplay.ScreenSpaceDrawQuad.Height));
                InputManager.ReleaseButton(MouseButton.Left);
            }, 5);

            checkScrolledToBottom();
            sendMessage();
            checkScrolledToBottom();
        }

        [Test]
        public void TestLocalEchoMessageResetsScroll()
        {
            fillChat();

            sendMessage();
            checkScrolledToBottom();

            AddStep("User scroll up", () =>
            {
                InputManager.MoveMouseTo(chatDisplay.ScreenSpaceDrawQuad.Centre);
                InputManager.PressButton(MouseButton.Left);
                InputManager.MoveMouseTo(chatDisplay.ScreenSpaceDrawQuad.Centre + new Vector2(0, chatDisplay.ScreenSpaceDrawQuad.Height));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            checkNotScrolledToBottom();
            sendMessage();
            checkNotScrolledToBottom();

            sendLocalMessage();
            checkScrolledToBottom();

            sendMessage();
            checkScrolledToBottom();
        }

        [Test]
        public void TestTextBoxSync()
        {
            AddStep("type 'hello' to text box 1", () => chatWithTextBox.ChildrenOfType<StandAloneChatDisplay.ChatTextBox>().Single().Text = "hello");
            AddAssert("text box 2 contains 'hello'", () => chatWithTextBox2.ChildrenOfType<StandAloneChatDisplay.ChatTextBox>().Single().Text == "hello");
        }

        private void fillChat(int count = 10)
        {
            AddStep("fill chat", () =>
            {
                for (int i = 0; i < count; i++)
                {
                    testChannel.AddNewMessages(new Message(messageIdSequence++)
                    {
                        Sender = longUsernameUser,
                        Content = $"some stuff {Guid.NewGuid()}",
                    });
                }
            });

            checkScrolledToBottom();
        }

        private void sendMessage()
        {
            AddStep("send lorem ipsum", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = longUsernameUser,
                Content = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce et bibendum velit.",
            }));
        }

        private void sendLocalMessage()
        {
            AddStep("send local echo", () => testChannel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = longUsernameUser,
                Content = "This is a local echo message.",
            }));
        }

        private void sendRegularMessages()
        {
            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = admin,
                Content = "I am a wang!"
            }));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = redUser,
                Content = "I am team red."
            }));

            AddStep("message from team red", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = redUser,
                Content = "I plan to win!"
            }));

            AddStep("message from team blue", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = blueUser,
                Content = "Not on my watch. Prepare to eat saaaaaaaaaand. Lots and lots of saaaaaaand."
            }));

            AddStep("message from admin", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = admin,
                Content = "Okay okay, calm down guys. Let's do this!"
            }));

            AddStep("message from long username", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = longUsernameUser,
                Content = "Hi guys, my new username is lit!"
            }));

            AddStep("message with new date", () => testChannel.AddNewMessages(new Message(messageIdSequence++)
            {
                Sender = longUsernameUser,
                Content = "Message from the future!",
                Timestamp = DateTimeOffset.Now
            }));
        }

        private void checkScrolledToBottom() =>
            AddUntilStep("is scrolled to bottom", () => chatDisplay.ScrolledToBottom);

        private void checkNotScrolledToBottom() =>
            AddUntilStep("not scrolled to bottom", () => !chatDisplay.ScrolledToBottom);

        private partial class TestStandAloneChatDisplay : StandAloneChatDisplay
        {
            public TestStandAloneChatDisplay(bool textBox = false)
                : base(textBox)
            {
            }

            public DrawableChannel DrawableChannel => InternalChildren.OfType<DrawableChannel>().First();

            public ChannelScrollContainer ScrollContainer => (ChannelScrollContainer)((Container)DrawableChannel.Child).Child;

            public FillFlowContainer FillFlow => (FillFlowContainer)ScrollContainer.Child;

            public bool ScrolledToBottom => ScrollContainer.IsScrolledToEnd(1);
        }
    }
}
