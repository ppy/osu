// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
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

            testAddLinks();
        }

        private void testAddLinks()
        {
            int msgCounter = 0;
            void addMessage(string text, bool isAction = false) => AddStep($"Add message #{++msgCounter}", () => textContainer.Add(new ChatLine(new DummyMessage(text, isAction))));

            addMessage("Test!");
            addMessage("osu.ppy.sh!");
            addMessage("https://osu.ppy.sh!");
            addMessage("00:12:345 (1,2) - Test?");
            addMessage("Wiki link for tasty [[Performance Points]]");
            addMessage("(osu forums)[https://osu.ppy.sh/forum] (old link format)");
            addMessage("[https://osu.ppy.sh/home New site] (new link format)");
            addMessage("[https://osu.ppy.sh/home This is only a link to the new osu webpage but this is supposed to test word wrap.]");
            addMessage("is now listening to [https://osu.ppy.sh/s/93523 IMAGE -MATERIAL- <Version 0>]", true);
            addMessage("is now playing [https://osu.ppy.sh/b/252238 IMAGE -MATERIAL- <Version 0>]", true);
            addMessage("#lobby or #osu would be blue (and work) in the ChatDisplay test (when a proper ChatOverlay is present).");
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(chat);
            dependencies.Cache(beatmapSetOverlay);
        }

        private class DummyMessage : Message
        {
            private static long messageCounter;
            private static readonly User sender = new User
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
                Sender = sender;
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
