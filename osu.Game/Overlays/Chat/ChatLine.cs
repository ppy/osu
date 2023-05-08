// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public partial class ChatLine : CompositeDrawable, IHasPopover
    {
        private Message message = null!;

        public Message Message
        {
            get => message;
            set
            {
                if (message == value) return;

                message = MessageFormatter.FormatMessage(value);

                if (!IsLoaded)
                    return;

                updateMessageContent();
            }
        }

        public IReadOnlyCollection<Drawable> DrawableContentFlow => drawableContentFlow;

        protected virtual float FontSize => 20;

        protected virtual float Spacing => 15;

        protected virtual float UsernameWidth => 130;

        [Resolved]
        private ChannelManager? chatManager { get; set; }

        [Resolved]
        private OverlayColourProvider? colourProvider { get; set; }

        private readonly OsuSpriteText drawableTimestamp;

        private readonly DrawableChatUsername drawableUsername;

        private readonly LinkFlowContainer drawableContentFlow;

        private readonly Bindable<bool> prefer24HourTime = new Bindable<bool>();

        private Container? highlight;

        public ChatLine(Message message)
        {
            Message = message;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(GridSizeMode.Absolute, Spacing + UsernameWidth + Spacing),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        drawableTimestamp = new OsuSpriteText
                        {
                            Shadow = false,
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(size: FontSize * 0.75f, weight: FontWeight.SemiBold, fixedWidth: true),
                            AlwaysPresent = true,
                        },
                        drawableUsername = new DrawableChatUsername(message.Sender)
                        {
                            Width = UsernameWidth,
                            FontSize = FontSize,
                            AutoSizeAxes = Axes.Y,
                            Origin = Anchor.TopRight,
                            Anchor = Anchor.TopRight,
                            Margin = new MarginPadding { Horizontal = Spacing },
                            ReportRequested = this.ShowPopover,
                        },
                        drawableContentFlow = new LinkFlowContainer(styleMessageContent)
                        {
                            AutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.X,
                        }
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager configManager)
        {
            configManager.BindWith(OsuSetting.Prefer24HourTime, prefer24HourTime);
            prefer24HourTime.BindValueChanged(_ => updateTimestamp());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            drawableTimestamp.Colour = colourProvider?.Background1 ?? Colour4.White;

            updateMessageContent();
            FinishTransforms(true);
        }

        public Popover GetPopover() => new ReportChatPopover(message);

        /// <summary>
        /// Performs a highlight animation on this <see cref="ChatLine"/>.
        /// </summary>
        public void Highlight()
        {
            if (highlight?.IsAlive != true)
            {
                AddInternal(highlight = new Container
                {
                    CornerRadius = 2f,
                    Masking = true,
                    RelativeSizeAxes = Axes.Both,
                    Colour = drawableUsername.AccentColour.Darken(1f),
                    Depth = float.MaxValue,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                });
            }

            highlight.FadeTo(0.5f).FadeOut(1500, Easing.InQuint);
            highlight.Expire();
        }

        private void styleMessageContent(SpriteText text)
        {
            text.Shadow = false;
            text.Font = text.Font.With(size: FontSize, italics: Message.IsAction);

            bool messageHasColour = Message.IsAction && !string.IsNullOrEmpty(message.Sender.Colour);
            text.Colour = messageHasColour ? Color4Extensions.FromHex(message.Sender.Colour) : colourProvider?.Content1 ?? Colour4.White;
        }

        private void updateMessageContent()
        {
            this.FadeTo(message is LocalEchoMessage ? 0.4f : 1.0f, 500, Easing.OutQuint);
            drawableTimestamp.FadeTo(message is LocalEchoMessage ? 0 : 1, 500, Easing.OutQuint);

            updateTimestamp();
            drawableUsername.Text = $@"{message.Sender.Username}";

            // remove non-existent channels from the link list
            message.Links.RemoveAll(link => link.Action == LinkAction.OpenChannel && chatManager?.AvailableChannels.Any(c => c.Name == link.Argument.ToString()) != true);

            drawableContentFlow.Clear();
            drawableContentFlow.AddLinks(message.DisplayContent, message.Links);
        }

        private void updateTimestamp()
        {
            drawableTimestamp.Text = message.Timestamp.LocalDateTime.ToLocalisableString(prefer24HourTime.Value ? @"HH:mm:ss" : @"hh:mm:ss tt");
        }
    }
}
