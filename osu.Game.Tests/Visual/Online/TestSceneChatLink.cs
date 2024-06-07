// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays.Chat;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChatLink : OsuTestScene
    {
        private readonly TestChatLineContainer textContainer;
        private Color4 linkColour;

        public TestSceneChatLink()
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
            linkColour = colours.Blue;

            var chatManager = new ChannelManager(API);
            BindableList<Channel> availableChannels = (BindableList<Channel>)chatManager.AvailableChannels;
            availableChannels.Add(new Channel { Name = "#english" });
            availableChannels.Add(new Channel { Name = "#japanese" });
            Dependencies.Cache(chatManager);

            Add(chatManager);
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            textContainer.Clear();
        });

        [TestCase("test!")]
        [TestCase("dev.ppy.sh!")]
        [TestCase("https://dev.ppy.sh!", LinkAction.External)]
        [TestCase("http://dev.ppy.sh!", LinkAction.External)]
        [TestCase("forgothttps://dev.ppy.sh!", LinkAction.External)]
        [TestCase("forgothttp://dev.ppy.sh!", LinkAction.External)]
        [TestCase("00:12:345 - Test?", LinkAction.OpenEditorTimestamp)]
        [TestCase("00:12:345 (1,2) - Test?", LinkAction.OpenEditorTimestamp)]
        [TestCase($"{OsuGameBase.OSU_PROTOCOL}edit/00:12:345 - Test?", LinkAction.OpenEditorTimestamp)]
        [TestCase($"{OsuGameBase.OSU_PROTOCOL}edit/00:12:345 (1,2) - Test?", LinkAction.OpenEditorTimestamp)]
        [TestCase($"{OsuGameBase.OSU_PROTOCOL}00:12:345 - not an editor timestamp", LinkAction.External)]
        [TestCase("Wiki link for tasty [[Performance Points]]", LinkAction.OpenWiki)]
        [TestCase("(osu forums)[https://dev.ppy.sh/forum] (old link format)", LinkAction.External)]
        [TestCase("[https://dev.ppy.sh/home New site] (new link format)", LinkAction.External)]
        [TestCase("[osu forums](https://dev.ppy.sh/forum) (new link format 2)", LinkAction.External)]
        [TestCase("[https://dev.ppy.sh/home This is only a link to the new osu webpage but this is supposed to test word wrap.]", LinkAction.External)]
        [TestCase("Let's (try)[https://dev.ppy.sh/home] [https://dev.ppy.sh/b/252238 multiple links] https://dev.ppy.sh/home", LinkAction.External, LinkAction.OpenBeatmap, LinkAction.External)]
        [TestCase("[https://dev.ppy.sh/home New link format with escaped [and \\[ paired] braces]", LinkAction.External)]
        [TestCase("[Markdown link format with escaped [and \\[ paired] braces](https://dev.ppy.sh/home)", LinkAction.External)]
        [TestCase("(Old link format with escaped (and \\( paired) parentheses)[https://dev.ppy.sh/home] and [[also a rogue wiki link]]", LinkAction.External, LinkAction.OpenWiki)]
        [TestCase("#lobby or #osu would be blue (and work) in the ChatDisplay test (when a proper ChatOverlay is present).")] // note that there's 0 links here (they get removed if a channel is not found)
        [TestCase("Join my multiplayer game osump://12346.", LinkAction.JoinMultiplayerMatch)]
        [TestCase("Join my multiplayer gameosump://12346.", LinkAction.JoinMultiplayerMatch)]
        [TestCase("Join my [multiplayer game](osump://12346).", LinkAction.JoinMultiplayerMatch)]
        [TestCase($"Join my [#english]({OsuGameBase.OSU_PROTOCOL}chan/#english).", LinkAction.OpenChannel)]
        [TestCase($"Join my {OsuGameBase.OSU_PROTOCOL}chan/#english.", LinkAction.OpenChannel)]
        [TestCase($"Join my{OsuGameBase.OSU_PROTOCOL}chan/#english.", LinkAction.OpenChannel)]
        [TestCase("Join my #english or #japanese channels.", LinkAction.OpenChannel, LinkAction.OpenChannel)]
        [TestCase("Join my #english or #nonexistent #hashtag channels.", LinkAction.OpenChannel)]
        [TestCase("Hello world\uD83D\uDE12(<--This is an emoji). There are more:\uD83D\uDE10\uD83D\uDE00,\uD83D\uDE20")]
        public void TestLinksGeneral(string text, params LinkAction[] actions)
        {
            addMessageWithChecks(text, expectedActions: actions);
        }

        [TestCase("is now listening to [https://dev.ppy.sh/s/93523 IMAGE -MATERIAL- <Version 0>]", true, false, LinkAction.OpenBeatmapSet)]
        [TestCase("is now playing [https://dev.ppy.sh/b/252238 IMAGE -MATERIAL- <Version 0>]", true, false, LinkAction.OpenBeatmap)]
        [TestCase("I am important!", false, true)]
        [TestCase("feels important", true, true)]
        [TestCase("likes to post this [https://dev.ppy.sh/home link].", true, true, LinkAction.External)]
        public void TestActionAndImportantLinks(string text, bool isAction, bool isImportant, params LinkAction[] expectedActions)
        {
            addMessageWithChecks(text, isAction, isImportant, expectedActions);
        }

        private void addMessageWithChecks(string text, bool isAction = false, bool isImportant = false, params LinkAction[] expectedActions)
        {
            ChatLine newLine = null!;

            AddStep("add message", () =>
            {
                newLine = new ChatLine(new DummyMessage(text, isAction, isImportant));
                textContainer.Add(newLine);
            });

            AddAssert("msg has the right action", () => newLine.Message.Links.Select(l => l.Action), () => Is.EqualTo(expectedActions));
            AddAssert($"msg shows {expectedActions.Length} link(s)", isShowingLinks);

            bool isShowingLinks()
            {
                bool hasBackground = !string.IsNullOrEmpty(newLine.Message.Sender.Colour);

                Color4 textColour = isAction && hasBackground ? Color4Extensions.FromHex(newLine.Message.Sender.Colour) : Color4.White;

                var linkCompilers = newLine.DrawableContentFlow.Where(d => d is DrawableLinkCompiler).ToList();
                var linkSprites = linkCompilers.SelectMany(comp => ((DrawableLinkCompiler)comp).Parts);

                return linkSprites.All(d => d.Colour == linkColour)
                       && newLine.DrawableContentFlow.Except(linkSprites.Concat(linkCompilers)).All(d => d.Colour == textColour)
                       && linkCompilers.Count == expectedActions.Length;
            }
        }

        [Test]
        public void TestEcho()
        {
            int messageIndex = 0;

            addEchoWithWait("sent!", "received!");
            addEchoWithWait("https://dev.ppy.sh/home", null, 500);
            addEchoWithWait("[https://dev.ppy.sh/forum let's try multiple words too!]");
            addEchoWithWait("(long loading times! clickable while loading?)[https://dev.ppy.sh/home]", null, 5000);

            void addEchoWithWait(string text, string? completeText = null, double delay = 250)
            {
                int index = messageIndex++;

                AddStep($"send msg #{index} after {delay}ms", () =>
                {
                    ChatLine newLine = new ChatLine(new DummyEchoMessage(text));
                    textContainer.Add(newLine);
                    Scheduler.AddDelayed(() => newLine.Message = new DummyMessage(completeText ?? text), delay);
                });

                AddUntilStep($"wait for msg #{index}", () => textContainer.All(line => line.Message is DummyMessage));
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

            internal static readonly APIUser TEST_SENDER = new APIUser
            {
                Username = @"Somebody",
                Id = 1,
            };

            public DummyMessage(string text, bool isAction = false, bool isImportant = false, int number = 0)
                : base(messageCounter++)
            {
                Content = text;
                IsAction = isAction;
                Sender = new APIUser
                {
                    Username = $"User {number}",
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
