// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChatTextBox : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> currentChannel = new Bindable<Channel>();

        private OsuSpriteText commitText;
        private OsuSpriteText searchText;
        private ChatTextBar bar;

        private ChatTextBox textBox => bar.ChildrenOfType<ChatTextBox>().Single();

        [SetUp]
        public void SetUp()
        {
            Schedule(() =>
            {
                Child = new GridContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 30),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(),
                                    new Dimension(),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        commitText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Font = OsuFont.Default.With(size: 20),
                                        },
                                        searchText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Font = OsuFont.Default.With(size: 20),
                                        },
                                    },
                                },
                            },
                        },
                        new Drawable[]
                        {
                            bar = new ChatTextBar
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = 0.99f,
                            },
                        },
                    },
                };

                bar.OnChatMessageCommitted += text =>
                {
                    commitText.Text = $"{nameof(bar.OnChatMessageCommitted)}: {text}";
                    commitText.FadeOutFromOne(1000, Easing.InQuint);
                };

                bar.OnSearchTermsChanged += text =>
                {
                    searchText.Text = $"{nameof(bar.OnSearchTermsChanged)}: {text}";
                };
            });
        }

        [Test]
        public void TestVisual()
        {
            AddStep("Public Channel", () => currentChannel.Value = createPublicChannel("#osu"));
            AddStep("Public Channel Long Name", () => currentChannel.Value = createPublicChannel("#public-channel-long-name"));
            AddStep("Private Channel", () => currentChannel.Value = createPrivateChannel("peppy", 2));
            AddStep("Private Long Name", () => currentChannel.Value = createPrivateChannel("test user long name", 3));

            AddStep("Chat Mode Channel", () => bar.ShowSearch.Value = false);
            AddStep("Chat Mode Search", () => bar.ShowSearch.Value = true);
        }

        [Test]
        public void TestLengthLimit()
        {
            var firstChannel = new Channel
            {
                Name = "#test1",
                Type = ChannelType.Public,
                Id = 4567,
                MessageLengthLimit = 20
            };
            var secondChannel = new Channel
            {
                Name = "#test2",
                Type = ChannelType.Public,
                Id = 5678,
                MessageLengthLimit = 5
            };

            AddStep("switch to channel with 20 char length limit", () => currentChannel.Value = firstChannel);
            AddStep("type a message", () => textBox.Current.Value = "abcdefgh");

            AddStep("switch to channel with 5 char length limit", () => currentChannel.Value = secondChannel);
            AddAssert("text box empty", () => textBox.Current.Value, () => Is.Empty);
            AddStep("type too much", () => textBox.Current.Value = "123456");
            AddAssert("text box has 5 chars", () => textBox.Current.Value, () => Has.Length.EqualTo(5));

            AddStep("switch back to channel with 20 char length limit", () => currentChannel.Value = firstChannel);
            AddAssert("unsent message preserved without truncation", () => textBox.Current.Value, () => Is.EqualTo("abcdefgh"));
        }

        private static Channel createPublicChannel(string name)
            => new Channel { Name = name, Type = ChannelType.Public, Id = 1234 };

        private static Channel createPrivateChannel(string username, int id)
            => new Channel(new APIUser { Id = id, Username = username });
    }
}
