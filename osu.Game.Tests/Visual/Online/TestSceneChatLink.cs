// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;
using osu.Game.Users;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneChatLink : OsuTestScene
    {
        private readonly TestChatLineContainer textContainer;
        private readonly DialogOverlay dialogOverlay;
        private Color4 linkColour;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(ChatLine),
            typeof(Message),
            typeof(LinkFlowContainer),
            typeof(DummyEchoMessage),
            typeof(LocalEchoMessage),
            typeof(MessageFormatter)
        };

        public TestSceneChatLink()
        {
            Add(dialogOverlay = new DialogOverlay { Depth = float.MinValue });
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
            linkColour = colours.Blue;

            var chatManager = new ChannelManager();
            BindableList<Channel> availableChannels = (BindableList<Channel>)chatManager.AvailableChannels;
            availableChannels.Add(new Channel { Name = "#english" });
            availableChannels.Add(new Channel { Name = "#japanese" });
            Dependencies.Cache(chatManager);

            Dependencies.Cache(new ChatOverlay());
            Dependencies.Cache(dialogOverlay);

            testLinksGeneral();
            testEcho();
        }

        private void clear() => AddStep("clear messages", textContainer.Clear);

        private void addMessageWithChecks(string text, int linkAmount = 0, bool isAction = false, bool isImportant = false, params LinkAction[] expectedActions)
        {
            int index = textContainer.Count + 1;
            var newLine = new ChatLine(new DummyMessage(text, isAction, isImportant, index));
            textContainer.Add(newLine);

            AddAssert($"msg #{index} has {linkAmount} link(s)", () => newLine.Message.Links.Count == linkAmount);
            AddAssert($"msg #{index} has the right action", hasExpectedActions);
            AddAssert($"msg #{index} is " + (isAction ? "italic" : "not italic"), () => newLine.ContentFlow.Any() && isAction == isItalic());
            AddAssert($"msg #{index} shows {linkAmount} link(s)", isShowingLinks);

            bool hasExpectedActions()
            {
                var expectedActionsList = expectedActions.ToList();

                if (expectedActionsList.Count != newLine.Message.Links.Count)
                    return false;

                for (int i = 0; i < newLine.Message.Links.Count; i++)
                {
                    var action = newLine.Message.Links[i].Action;
                    if (action != expectedActions[i]) return false;
                }

                return true;
            }

            bool isItalic() => newLine.ContentFlow.Where(d => d is OsuSpriteText).Cast<OsuSpriteText>().All(sprite => sprite.Font.Italics);

            bool isShowingLinks()
            {
                bool hasBackground = !string.IsNullOrEmpty(newLine.Message.Sender.Colour);

                Color4 textColour = isAction && hasBackground ? Color4Extensions.FromHex(newLine.Message.Sender.Colour) : Color4.White;

                var linkCompilers = newLine.ContentFlow.Where(d => d is DrawableLinkCompiler).ToList();
                var linkSprites = linkCompilers.SelectMany(comp => ((DrawableLinkCompiler)comp).Parts);

                return linkSprites.All(d => d.Colour == linkColour)
                       && newLine.ContentFlow.Except(linkSprites.Concat(linkCompilers)).All(d => d.Colour == textColour);
            }
        }

        private void testLinksGeneral()
        {
            addMessageWithChecks("test!");
            addMessageWithChecks("osu.ppy.sh!");
            addMessageWithChecks("https://osu.ppy.sh!", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("00:12:345 (1,2) - Test?", 1, expectedActions: LinkAction.OpenEditorTimestamp);
            addMessageWithChecks("Wiki link for tasty [[Performance Points]]", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("(osu forums)[https://osu.ppy.sh/forum] (old link format)", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("[https://osu.ppy.sh/home New site] (new link format)", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("[osu forums](https://osu.ppy.sh/forum) (new link format 2)", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("[https://osu.ppy.sh/home This is only a link to the new osu webpage but this is supposed to test word wrap.]", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("is now listening to [https://osu.ppy.sh/s/93523 IMAGE -MATERIAL- <Version 0>]", 1, true, expectedActions: LinkAction.OpenBeatmapSet);
            addMessageWithChecks("is now playing [https://osu.ppy.sh/b/252238 IMAGE -MATERIAL- <Version 0>]", 1, true, expectedActions: LinkAction.OpenBeatmap);
            addMessageWithChecks("Let's (try)[https://osu.ppy.sh/home] [https://osu.ppy.sh/b/252238 multiple links] https://osu.ppy.sh/home", 3,
                expectedActions: new[] { LinkAction.External, LinkAction.OpenBeatmap, LinkAction.External });
            addMessageWithChecks("[https://osu.ppy.sh/home New link format with escaped [and \\[ paired] braces]", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("[Markdown link format with escaped [and \\[ paired] braces](https://osu.ppy.sh/home)", 1, expectedActions: LinkAction.External);
            addMessageWithChecks("(Old link format with escaped (and \\( paired) parentheses)[https://osu.ppy.sh/home] and [[also a rogue wiki link]]", 2, expectedActions: new[] { LinkAction.External, LinkAction.External });
            // note that there's 0 links here (they get removed if a channel is not found)
            addMessageWithChecks("#lobby or #osu would be blue (and work) in the ChatDisplay test (when a proper ChatOverlay is present).");
            addMessageWithChecks("I am important!", 0, false, true);
            addMessageWithChecks("feels important", 0, true, true);
            addMessageWithChecks("likes to post this [https://osu.ppy.sh/home link].", 1, true, true, expectedActions: LinkAction.External);
            addMessageWithChecks("Join my multiplayer game osump://12346.", 1, expectedActions: LinkAction.JoinMultiplayerMatch);
            addMessageWithChecks("Join my [multiplayer game](osump://12346).", 1, expectedActions: LinkAction.JoinMultiplayerMatch);
            addMessageWithChecks("Join my [#english](osu://chan/#english).", 1, expectedActions: LinkAction.OpenChannel);
            addMessageWithChecks("Join my osu://chan/#english.", 1, expectedActions: LinkAction.OpenChannel);
            addMessageWithChecks("Join my #english or #japanese channels.", 2, expectedActions: new[] { LinkAction.OpenChannel, LinkAction.OpenChannel });
            addMessageWithChecks("Join my #english or #nonexistent #hashtag channels.", 1, expectedActions: LinkAction.OpenChannel);
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

                AddUntilStep($"wait for msg #{echoCounter}", () => textContainer.All(line => line.Message is DummyMessage));
            }
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

            public DummyMessage(string text, bool isAction = false, bool isImportant = false, int number = 0)
                : base(messageCounter++)
            {
                Content = text;
                IsAction = isAction;
                Sender = new User
                {
                    Username = $"User {number}",
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
