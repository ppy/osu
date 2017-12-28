// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Users;
using System;
using System.Linq;

namespace osu.Game.Tests.Visual
{
    public class TestCaseChatLink : OsuTestCase
    {
        private readonly BeatmapSetOverlay beatmapSetOverlay;
        private readonly ChatOverlay chat;

        private DependencyContainer dependencies;

        private readonly TestChatLineContainer textContainer;

        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) => dependencies = new DependencyContainer(parent);

        public TestCaseChatLink()
        {
            chat = new ChatOverlay();
            Add(beatmapSetOverlay = new BeatmapSetOverlay { Depth = float.MinValue });

            Add(textContainer = new TestChatLineContainer
            {
                Padding = new MarginPadding { Left = 20, Right = 20 },
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
            });

            testLinksGeneral();
            testAddingLinks();
            testEcho();
        }

        private void clear() => AddStep("clear messages", textContainer.Clear);

        private void addMessageWithChecks(string text, int linkAmount = 0, bool isAction = false)
        {
            var newLine = new ChatLine(new DummyMessage(text, isAction));
            textContainer.Add(newLine);

            AddAssert($"msg #{textContainer.Count} has {linkAmount} link(s)", () => newLine.Message.Links.Count == linkAmount);
            AddAssert($"msg #{textContainer.Count} is " + (isAction ? "italic" : "not italic"), () => newLine.ContentFlow.Any() && isAction == isItalic(newLine.ContentFlow));
            AddAssert($"msg #{textContainer.Count} shows link(s)", () => isShowingLinks(newLine.ContentFlow));

            bool isItalic(OsuTextFlowContainer c) => c.Cast<ChatLink>().All(sprite => sprite.Font == @"Exo2.0-MediumItalic");

            bool isShowingLinks(OsuTextFlowContainer c) => c.Cast<ChatLink>().All(sprite => sprite.HandleInput && !sprite.TextColour.Equals((SRGBColour)Color4.White)
                                                                                        || !sprite.HandleInput && sprite.TextColour.Equals((SRGBColour)Color4.White));
        }

        private void testLinksGeneral()
        {
            addMessageWithChecks("test!");
            addMessageWithChecks("osu.ppy.sh!");
            addMessageWithChecks("https://osu.ppy.sh!", 1);
            addMessageWithChecks("00:12:345 (1,2) - Test?", 1);
            addMessageWithChecks("Wiki link for tasty [[Performance Points]]", 1);
            addMessageWithChecks("(osu forums)[https://osu.ppy.sh/forum] (old link format)", 1);
            addMessageWithChecks("[https://osu.ppy.sh/home New site] (new link format)", 1);
            addMessageWithChecks("[https://osu.ppy.sh/home This is only a link to the new osu webpage but this is supposed to test word wrap.]", 1);
            addMessageWithChecks("is now listening to [https://osu.ppy.sh/s/93523 IMAGE -MATERIAL- <Version 0>]", 1, true);
            addMessageWithChecks("is now playing [https://osu.ppy.sh/b/252238 IMAGE -MATERIAL- <Version 0>]", 1, true);
            addMessageWithChecks("Let's (try)[https://osu.ppy.sh/home] [https://osu.ppy.sh/home multiple links] https://osu.ppy.sh/home", 3);
            // note that there's 0 links here (they get removed if a channel is not found)
            addMessageWithChecks("#lobby or #osu would be blue (and work) in the ChatDisplay test (when a proper ChatOverlay is present).");
        }

        private void testAddingLinks()
        {
            const int count = 5;

            for (int i = 1; i <= count; i++)
                AddStep($"add long msg #{i}", () => textContainer.Add(new ChatLine(new DummyMessage("alright let's just put a really long text here to see if it loads in correctly rather than adding the text sprites individually after the chat line appearing!"))));

            clear();
        }

        private void testEcho()
        {
            int echoCounter = 0;

            addEchoWithWait("sent!", "received!");
            addEchoWithWait("https://osu.ppy.sh/home", null, 500);
            addEchoWithWait("[https://osu.ppy.sh/forum let's try multiple words too!]");
            addEchoWithWait("(long loading times! clickable while loading?)[https://osu.ppy.sh/home]", null, 5000);

            void addEchoWithWait(string text, string completeText = null, double delay = 250)
            {
                var newLine = new ChatLine(new DummyEchoMessage(text));

                AddStep($"send msg #{++echoCounter} after {delay}ms", () =>
                {
                    textContainer.Add(newLine);
                    Scheduler.AddDelayed(() => newLine.Message = new DummyMessage(completeText ?? text), delay);
                });

                AddUntilStep(() => textContainer.All(line => line.Message is DummyMessage), $"wait for msg #{echoCounter}");
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(chat);
            dependencies.Cache(beatmapSetOverlay);
        }

        private class DummyEchoMessage : LocalEchoMessage
        {
            public DummyEchoMessage(string text)
            {
                Content = text;
                Timestamp = DateTimeOffset.Now;
                Sender = DummyMessage.TEST_SENDER;
            }
        }

        private class DummyMessage : Message
        {
            private static long messageCounter;
            internal static readonly User TEST_SENDER = new User
            {
                Username = @"Somebody",
                Id = 1,
                Country = new Country { FullName = @"Alien" },
                CoverUrl = @"https://osu.ppy.sh/images/headers/profile-covers/c1.jpg",
                JoinDate = DateTimeOffset.Now.AddDays(-1),
                LastVisit = DateTimeOffset.Now,
                Age = 1,
                ProfileOrder = new[] { "me" },
                CountryRank = 1,
                Statistics = new UserStatistics
                {
                    Rank = 2148,
                    PP = 4567.89m
                },
                RankHistory = new User.RankHistoryData
                {
                    Mode = @"osu",
                    Data = Enumerable.Range(2345, 45).Concat(Enumerable.Range(2109, 40)).ToArray(),
                }
            };

            public new DateTimeOffset Timestamp = DateTimeOffset.Now;

            public DummyMessage(string text, bool isAction = false)
                : base(messageCounter++)
            {
                Content = text;
                IsAction = isAction;
                Sender = TEST_SENDER;
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
