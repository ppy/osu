// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChatLineTruncation : OsuTestScene
    {
        private readonly TestChatLineContainer textContainer;

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

        private void addMessageWithChecks(string text, bool isAction = false, bool isImportant = false, string username = null, Colour4? color = null)
        {
            int index = textContainer.Count + 1;

            var newLine = color != null
                ? new ChatLine(new DummyMessage(text, isAction, isImportant, index, username))
                {
                    UsernameColour = color.Value
                }
                : new ChatLine(new DummyMessage(text, isAction, isImportant, index, username));

            textContainer.Add(newLine);
        }

        private void testFormatting()
        {
            for (int a = 0; a < 25; a++)
                addMessageWithChecks($"Wide {a} character username.", username: new string('w', a));
            addMessageWithChecks("Short name with spaces.", username: "sho rt name");
            addMessageWithChecks("Long name with spaces.", username: "long name with s p a c e s");
            addMessageWithChecks("message with custom color", username: "I have custom color", color: Colour4.Green);
        }

        private class DummyMessage : Message
        {
            private static long messageCounter;

            internal static readonly APIUser TEST_SENDER_BACKGROUND = new APIUser
            {
                Username = @"i-am-important",
                Id = 42,
                Colour = "#250cc9",
            };

            internal static readonly APIUser TEST_SENDER = new APIUser
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
                Sender = new APIUser
                {
                    Username = username ?? $"user {number}",
                    Id = number,
                    Colour = isImportant ? "#250cc9" : null,
                };
            }
        }

        private partial class TestChatLineContainer : FillFlowContainer<ChatLine>
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
