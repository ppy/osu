// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Chat;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneChatTextBox : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        [Cached]
        private readonly Bindable<Channel> currentChannel = new Bindable<Channel>();

        private OsuSpriteText commitText;
        private OsuSpriteText searchText;
        private ChatTextBar bar;

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

        private static Channel createPublicChannel(string name)
            => new Channel { Name = name, Type = ChannelType.Public, Id = 1234 };

        private static Channel createPrivateChannel(string username, int id)
            => new Channel(new APIUser { Id = id, Username = username });
    }
}
