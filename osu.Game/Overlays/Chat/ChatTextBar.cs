// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;

namespace osu.Game.Overlays.Chat
{
    public class ChatTextBar : Container
    {
        public readonly BindableBool ShowSearch = new BindableBool();

        public ChatTextBox TextBox => chatTextBox;

        [Resolved]
        private Bindable<Channel> currentChannel { get; set; } = null!;

        private OsuTextFlowContainer chattingTextContainer = null!;
        private Container searchIconContainer = null!;
        private ChatTextBox chatTextBox = null!;
        private Container enterContainer = null!;

        private const float chatting_text_width = 180;
        private const float search_icon_width = 40;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            Height = 60;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            chattingTextContainer = new OsuTextFlowContainer(t => t.Font = t.Font.With(size: 20))
                            {
                                Masking = true,
                                Width = chatting_text_width,
                                Padding = new MarginPadding { Left = 10 },
                                RelativeSizeAxes = Axes.Y,
                                TextAnchor = Anchor.CentreRight,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Colour = colourProvider.Background1,
                            },
                            searchIconContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = search_icon_width,
                                Child = new SpriteIcon
                                {
                                    Icon = FontAwesome.Solid.Search,
                                    Origin = Anchor.CentreRight,
                                    Anchor = Anchor.CentreRight,
                                    Size = new Vector2(20),
                                    Margin = new MarginPadding { Right = 2 },
                                },
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Right = 5 },
                                Child = chatTextBox =  new ChatTextBox
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    ShowSearch = { BindTarget = ShowSearch },
                                    HoldFocus = true,
                                    ReleaseFocusOnCommit = false,
                                },
                            },
                            enterContainer = new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Masking = true,
                                BorderColour = colourProvider.Background1,
                                BorderThickness = 2,
                                CornerRadius = 10,
                                Margin = new MarginPadding { Right = 10 },
                                Size = new Vector2(60, 30),
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = Colour4.Transparent,
                                    },
                                    new OsuSpriteText
                                    {
                                        Text = "Enter",
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        Colour = colourProvider.Background1,
                                        Font = OsuFont.Torus.With(size: 20),
                                        Margin = new MarginPadding { Bottom = 2 },
                                    },
                                },
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ShowSearch.BindValueChanged(change =>
            {
                if (change.NewValue)
                {
                    chattingTextContainer.Hide();
                    enterContainer.Hide();
                    searchIconContainer.Show();
                }
                else
                {
                    chattingTextContainer.Show();
                    enterContainer.Show();
                    searchIconContainer.Hide();
                }
            }, true);

            currentChannel.BindValueChanged(change =>
            {
                Channel newChannel = change.NewValue;
                switch (newChannel?.Type)
                {
                    case ChannelType.Public:
                        chattingTextContainer.Text = $"chatting in {newChannel.Name}";
                        break;
                    case ChannelType.PM:
                        chattingTextContainer.Text = $"chatting with {newChannel.Name}";
                        break;
                    default:
                        chattingTextContainer.Text = "";
                        break;
                }
            }, true);
        }
    }
}
