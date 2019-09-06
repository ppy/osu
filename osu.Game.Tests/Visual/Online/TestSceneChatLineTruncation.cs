// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChatLineTruncation : OsuTestScene
    {
        private readonly TestChatLineContainer textContainer;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatLine),
            typeof(Message),
            typeof(LinkFlowContainer),
            typeof(MessageFormatter)
        };

        public TestSceneChatLineTruncation()
        {
            Add(textContainer = new TestChatLineContainer
            {
                Padding = new MarginPadding { Left = 20, Right = 20 },
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            testFormatting();
        }

        private void clear() => AddStep("clear messages", textContainer.Clear);

        private void addMessageWithChecks(string text, bool isAction = false, bool isImportant = false, string username = null)
        {
            int index = textContainer.Count + 1;
            var newLine = new ChatLine(new DummyMessage(text, isAction, isImportant, index, username));
            textContainer.Add(newLine);
        }

        private void testFormatting()
        {
            for (int a = 0; a < 25; a++)
                addMessageWithChecks($"Wide {a} character username.", username: new string('w', a));
            addMessageWithChecks("Short name with spaces.", username: "sho rt name");
            addMessageWithChecks("Long name with spaces.", username: "long name with s p a c e s");
        }

        private class DummyMessage : Message
        {
            private static long messageCounter;

            internal static readonly User TEST_SENDER_BACKGROUND = new User
            {
                Username = @"i-am-important",
                Id = 42,
                Colour = "#250cc9",
            };

            internal static readonly User TEST_SENDER = new User
            {
                Username = @"Somebody",
                Id = 1,
            };

            public new DateTimeOffset Timestamp = DateTimeOffset.Now;

            public DummyMessage(string text, bool isAction = false, bool isImportant = false, int number = 0, string username = null)
                : base(messageCounter++)
            {
                Content = text;
                IsAction = isAction;
                Sender = new User
                {
                    Username = username ?? $"user {number}",
                    Id = number,
                    Colour = isImportant ? "#250cc9" : null,
                };
            }
        }

        private class TestChatLineContainer : FillFlowContainer<ChatLine>
        {
            protected override int Compare(Drawable x, Drawable y)
            {
                var xC = (ChatLine)x;
                var yC = (ChatLine)y;

                return xC.Message.CompareTo(yC.Message);
            }
        }
    }
}
