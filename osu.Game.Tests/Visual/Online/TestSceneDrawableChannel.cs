// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneDrawableChannel : OsuTestScene
    {
        private Channel channel = null!;
        private DrawableChannel drawableChannel = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create channel", () => channel = new Channel
            {
                Id = 1,
                Name = "Test channel"
            });
            AddStep("create drawable channel", () => Child = drawableChannel = new DrawableChannel(channel)
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        [Test]
        public void TestMention()
        {
            AddStep("add normal message", () => channel.AddNewMessages(
                new Message(1)
                {
                    Sender = new APIUser
                    {
                        Id = 2,
                        Username = "TestUser2"
                    },
                    Content = "Hello how are you today?",
                    Timestamp = new DateTimeOffset(2021, 12, 11, 13, 33, 24, TimeSpan.Zero)
                }));

            AddStep("add mention", () => channel.AddNewMessages(
                new Message(2)
                {
                    Sender = new APIUser
                    {
                        Id = 2,
                        Username = "TestUser2"
                    },
                    Content = $"Hello {API.LocalUser.Value.Username} how are you today?",
                    Timestamp = new DateTimeOffset(2021, 12, 11, 13, 33, 25, TimeSpan.Zero)
                }));
        }

        [Test]
        public void TestDaySeparators()
        {
            var localUser = new APIUser
            {
                Id = 3,
                Username = "LocalUser"
            };

            string uuid = Guid.NewGuid().ToString();
            AddStep("add local echo message", () => channel.AddLocalEcho(new LocalEchoMessage
            {
                Sender = localUser,
                Content = "Hi there all!",
                Timestamp = new DateTimeOffset(2022, 11, 21, 20, 11, 13, TimeSpan.Zero),
                Uuid = uuid
            }));
            AddUntilStep("one day separator present", () => drawableChannel.ChildrenOfType<DaySeparator>().Count() == 1);

            AddStep("add two prior messages to channel", () => channel.AddNewMessages(
                new Message(1)
                {
                    Sender = new APIUser
                    {
                        Id = 1,
                        Username = "TestUser"
                    },
                    Content = "This is a message",
                    Timestamp = new DateTimeOffset(2021, 10, 10, 13, 33, 23, TimeSpan.Zero),
                },
                new Message(2)
                {
                    Sender = new APIUser
                    {
                        Id = 2,
                        Username = "TestUser2"
                    },
                    Content = "This is another message",
                    Timestamp = new DateTimeOffset(2021, 10, 11, 13, 33, 23, TimeSpan.Zero)
                }));
            AddUntilStep("three day separators present", () => drawableChannel.ChildrenOfType<DaySeparator>().Count() == 3);

            AddStep("resolve pending message", () => channel.ReplaceMessage(channel.Messages.OfType<LocalEchoMessage>().Single(), new Message(3)
            {
                Sender = localUser,
                Content = "Hi there all!",
                Timestamp = new DateTimeOffset(2022, 11, 22, 20, 11, 16, TimeSpan.Zero),
                Uuid = uuid
            }));
            AddUntilStep("three day separators present", () => drawableChannel.ChildrenOfType<DaySeparator>().Count() == 3);
            AddAssert("last day separator is from correct day", () => drawableChannel.ChildrenOfType<DaySeparator>().Last().Date.Date == new DateTime(2022, 11, 22));
        }

        [Test]
        public void TestBackgroundAlternating()
        {
            int messageCount = 1;

            AddRepeatStep("add messages", () =>
            {
                channel.AddNewMessages(new Message(messageCount)
                {
                    Sender = new APIUser
                    {
                        Id = 3,
                        Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                    },
                    Content = "Hi there all!",
                    Timestamp = new DateTimeOffset(2022, 11, 21, 20, messageCount, 13, TimeSpan.Zero),
                    Uuid = Guid.NewGuid().ToString(),
                });
                messageCount++;
            }, 10);

            AddUntilStep("10 message present", () => drawableChannel.ChildrenOfType<ChatLine>().Count() == 10);

            int checkCount = 0;

            AddRepeatStep("check background", () =>
            {
                // +1 because the day separator take one index
                Assert.AreEqual((checkCount + 1) % 2 == 0, drawableChannel.ChildrenOfType<ChatLine>().ToList()[checkCount].AlternatingBackground);
                checkCount++;
            }, 10);
        }

        [Test]
        public void TestAlternatingBackgroundDoesNotChangeAtMaxHistory()
        {
            AddStep("fill up the channel", () =>
            {
                for (int i = 0; i < Channel.MAX_HISTORY; i++)
                {
                    channel.AddNewMessages(new Message
                    {
                        ChannelId = channel.Id,
                        Content = $"Message {i}",
                        Timestamp = DateTimeOffset.Now,
                        Sender = new APIUser
                        {
                            Id = 3,
                            Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                        }
                    });
                }
            });

            AddUntilStep($"{Channel.MAX_HISTORY} messages present", () => drawableChannel.ChildrenOfType<ChatLine>().Count(), () => Is.EqualTo(Channel.MAX_HISTORY));

            ChatLine? lastLine = null;
            bool lastLineAlternatingBackground = false;

            AddStep("grab last line", () =>
            {
                lastLine = drawableChannel.ChildrenOfType<ChatLine>().Last();
                lastLineAlternatingBackground = lastLine.AlternatingBackground;
            });

            AddStep("add another message", () => channel.AddNewMessages(new Message
            {
                ChannelId = channel.Id,
                Content = "One final message",
                Timestamp = DateTimeOffset.Now,
                Sender = new APIUser
                {
                    Id = 3,
                    Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                }
            }));

            AddAssert("second-last message has same background", () => lastLine!.AlternatingBackground, () => Is.EqualTo(lastLineAlternatingBackground));
        }

        [Test]
        public void TestAlternatingBackgroundUpdatedOnRemoval()
        {
            AddStep("add 3 messages", () =>
            {
                for (int i = 0; i < 3; i++)
                {
                    channel.AddNewMessages(new Message
                    {
                        ChannelId = channel.Id,
                        Content = $"Message {i}",
                        Timestamp = DateTimeOffset.Now,
                        Sender = new APIUser
                        {
                            Id = i,
                            Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                        }
                    });
                }
            });

            AddUntilStep("3 messages present", () => drawableChannel.ChildrenOfType<ChatLine>().Count(), () => Is.EqualTo(3));
            assertAlternatingBackground(0, false);
            assertAlternatingBackground(1, true);
            assertAlternatingBackground(2, false);

            AddStep("remove middle message", () => channel.RemoveMessagesFromUser(1));
            AddUntilStep("2 messages present", () => drawableChannel.ChildrenOfType<ChatLine>().Count(), () => Is.EqualTo(2));
            assertAlternatingBackground(0, true);
            assertAlternatingBackground(1, false);

            void assertAlternatingBackground(int lineIndex, bool shouldBeAlternating)
                => AddAssert($"line {lineIndex} {(shouldBeAlternating ? "has" : "does not have")} alternating background",
                    () => drawableChannel.ChildrenOfType<ChatLine>().ElementAt(lineIndex).AlternatingBackground,
                    () => Is.EqualTo(shouldBeAlternating));
        }

        [Test]
        public void TestTimestampsUpdateOnRemoval()
        {
            AddStep("add 3 messages", () =>
            {
                channel.AddNewMessages(
                    new Message
                    {
                        ChannelId = channel.Id,
                        Content = "Message 0",
                        Timestamp = new DateTimeOffset(2022, 11, 21, 20, 0, 0, TimeSpan.Zero),
                        Sender = new APIUser
                        {
                            Id = 0,
                            Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                        }
                    },
                    new Message
                    {
                        ChannelId = channel.Id,
                        Content = "Message 1",
                        Timestamp = new DateTimeOffset(2022, 11, 21, 20, 0, 0, TimeSpan.Zero).AddSeconds(1),
                        Sender = new APIUser
                        {
                            Id = 1,
                            Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                        }
                    },
                    new Message
                    {
                        ChannelId = channel.Id,
                        Content = "Message 2",
                        Timestamp = new DateTimeOffset(2022, 11, 21, 20, 0, 0, TimeSpan.Zero).AddMinutes(1),
                        Sender = new APIUser
                        {
                            Id = 2,
                            Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                        }
                    },
                    new Message
                    {
                        ChannelId = channel.Id,
                        Content = "Message 3",
                        Timestamp = new DateTimeOffset(2022, 11, 21, 20, 0, 0, TimeSpan.Zero).AddMinutes(1).AddSeconds(1),
                        Sender = new APIUser
                        {
                            Id = 3,
                            Username = "LocalUser " + RNG.Next(0, int.MaxValue - 100).ToString("N")
                        }
                    }
                );
            });

            AddUntilStep("4 messages present", () => drawableChannel.ChildrenOfType<ChatLine>().Count(), () => Is.EqualTo(4));
            assertTimestamp(0, true);
            assertTimestamp(1, false);
            assertTimestamp(2, true);
            assertTimestamp(3, false);

            AddStep("remove message 0", () => channel.RemoveMessagesFromUser(0));
            AddUntilStep("3 messages present", () => drawableChannel.ChildrenOfType<ChatLine>().Count(), () => Is.EqualTo(3));
            assertTimestamp(0, true);
            assertTimestamp(1, true);
            assertTimestamp(2, false);

            AddStep("remove message 2", () => channel.RemoveMessagesFromUser(2));
            AddUntilStep("2 messages present", () => drawableChannel.ChildrenOfType<ChatLine>().Count(), () => Is.EqualTo(2));
            assertTimestamp(0, true);
            assertTimestamp(1, true);

            void assertTimestamp(int lineIndex, bool shouldHaveTimestamp)
                => AddAssert($"line {lineIndex} {(shouldHaveTimestamp ? "has" : "does not have")} timestamp",
                    () => drawableChannel.ChildrenOfType<ChatLine>().ElementAt(lineIndex).RequiresTimestamp,
                    () => Is.EqualTo(shouldHaveTimestamp));
        }
    }
}
