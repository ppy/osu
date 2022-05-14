// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;

namespace osu.Game.Overlays.Chat
{
    public class ChatTextBar : Container
    {
        public readonly BindableBool ShowSearch = new BindableBool();

        public event Action<string>? OnChatMessageCommitted;

        public event Action<string>? OnSearchTermsChanged;

        public void TextBoxTakeFocus() => chatTextBox.TakeFocus();

        public void TextBoxKillFocus() => chatTextBox.KillFocus();

        [Resolved]
        private Bindable<Channel> currentChannel { get; set; } = null!;

        private Container chattingTextContainer = null!;
        private OsuSpriteText chattingText = null!;
        private Container searchIconContainer = null!;
        private ChatTextBox chatTextBox = null!;

        private const float chatting_text_width = 220;
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
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            chattingTextContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Y,
                                Width = chatting_text_width,
                                Masking = true,
                                Padding = new MarginPadding { Right = 5 },
                                Child = chattingText = new OsuSpriteText
                                {
                                    Font = OsuFont.Torus.With(size: 20),
                                    Colour = colourProvider.Background1,
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Truncate = true,
                                },
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
                                Child = chatTextBox = new ChatTextBox
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RelativeSizeAxes = Axes.X,
                                    ShowSearch = { BindTarget = ShowSearch },
                                    HoldFocus = true,
                                    ReleaseFocusOnCommit = false,
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

            chatTextBox.Current.ValueChanged += chatTextBoxChange;
            chatTextBox.OnCommit += chatTextBoxCommit;

            ShowSearch.BindValueChanged(change =>
            {
                bool showSearch = change.NewValue;

                chattingTextContainer.FadeTo(showSearch ? 0 : 1);
                searchIconContainer.FadeTo(showSearch ? 1 : 0);

                // Clear search terms if any exist when switching back to chat mode
                if (!showSearch)
                    OnSearchTermsChanged?.Invoke(string.Empty);
            }, true);

            currentChannel.BindValueChanged(change =>
            {
                Channel newChannel = change.NewValue;

                switch (newChannel?.Type)
                {
                    case ChannelType.Public:
                        chattingText.Text = $"chatting in {newChannel.Name}";
                        break;

                    case ChannelType.PM:
                        chattingText.Text = $"chatting with {newChannel.Name}";
                        break;

                    default:
                        chattingText.Text = string.Empty;
                        break;
                }
            }, true);
        }

        private void chatTextBoxChange(ValueChangedEvent<string> change)
        {
            if (ShowSearch.Value)
                OnSearchTermsChanged?.Invoke(change.NewValue);
        }

        private void chatTextBoxCommit(TextBox sender, bool newText)
        {
            if (ShowSearch.Value)
                return;

            OnChatMessageCommitted?.Invoke(sender.Text);
            sender.Text = string.Empty;
        }
    }
}
