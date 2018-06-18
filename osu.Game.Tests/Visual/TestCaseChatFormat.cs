// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osu.Game.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseChatFormat : OsuTestCase
    {
        private readonly TestChatLineContainer textContainer;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatLine),
            typeof(Message),
            typeof(LinkFlowContainer),
            typeof(MessageFormatter)
        };

        public TestCaseChatFormat()
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
        private void load(OsuColour colours)
        {
            testLinksGeneral();
        }

        private void clear() => AddStep("clear messages", textContainer.Clear);

        private void addMessageWithChecks(string text, int linkAmount = 0, bool isAction = false, bool isImportant = false, bool isTruncated = false, string username = null, params LinkAction[] expectedActions)
        {
            int index = textContainer.Count + 1;
            var newLine = new ChatLine(new DummyMessage(text, isAction, isImportant, index, username));
            textContainer.Add(newLine);

            //if (isTruncated)
            //    AddAssert($"msg #{index} username is truncated", () => );
            //else
            //    AddAssert($"msg #{index} username is not truncated", () => );
        }

        private void testLinksGeneral()
        {
            for(int a = 0; a < 15; a++)
            {
                bool isTruncated = a > 6 ? true : false;
                addMessageWithChecks($"Wide {a} character username.", username: new string('w', a), isTruncated: isTruncated);

            }
            addMessageWithChecks("Short name with spaces.", username: "sho rt name");
            addMessageWithChecks("Long name with spaces.", username: "long name with s p a c e s", isTruncated: true);
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
                if (username == null)
                {
                    username = $"user {number}";
                }

                Content = text;
                IsAction = isAction;
                Sender = new User
                {
                    Username = username,
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
