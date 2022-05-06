// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osuTK;

namespace osu.Game.Overlays.Chat.Listing
{
    public class ChannelListingItem : OsuClickableContainer, IFilterable
    {
        public event Action<Channel>? OnRequestJoin;
        public event Action<Channel>? OnRequestLeave;

        public readonly Channel Channel;

        public bool FilteringActive { get; set; }
        public IEnumerable<string> FilterTerms => new[] { Channel.Name, Channel.Topic ?? string.Empty };
        public bool MatchingFilter { set => this.FadeTo(value ? 1f : 0f, 100); }

        private Box hoverBox = null!;
        private SpriteIcon checkbox = null!;
        private OsuSpriteText channelText = null!;
        private OsuSpriteText topicText = null!;
        private IBindable<bool> channelJoined = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private const float text_size = 18;
        private const float icon_size = 14;

        private const float vertical_margin = 1.5f;

        public ChannelListingItem(Channel channel)
        {
            Channel = channel;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;
            CornerRadius = 5;
            RelativeSizeAxes = Axes.X;
            Height = 20 + (vertical_margin * 2);

            Children = new Drawable[]
            {
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3,
                    Margin = new MarginPadding { Vertical = vertical_margin },
                    Alpha = 0f,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 40),
                        new Dimension(GridSizeMode.Absolute, 200),
                        new Dimension(GridSizeMode.Absolute, 400),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            checkbox = new SpriteIcon
                            {
                                Alpha = 0,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Icon = FontAwesome.Solid.Check,
                                Size = new Vector2(icon_size),
                            },
                            channelText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = Channel.Name,
                                Font = OsuFont.Torus.With(size: text_size, weight: FontWeight.SemiBold),
                                Margin = new MarginPadding { Bottom = 2 },
                            },
                            topicText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = Channel.Topic,
                                Font = OsuFont.Torus.With(size: text_size),
                                Margin = new MarginPadding { Bottom = 2 },
                            },
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Icon = FontAwesome.Solid.User,
                                Size = new Vector2(icon_size),
                                Margin = new MarginPadding { Right = 5 },
                                Colour = colourProvider.Light3,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = "0",
                                Font = OsuFont.Torus.With(size: text_size),
                                Margin = new MarginPadding { Bottom = 2 },
                                Colour = colourProvider.Light3,
                            },
                        },
                    },
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            channelJoined = Channel.Joined.GetBoundCopy();
            channelJoined.BindValueChanged(change =>
            {
                const double duration = 500;

                if (change.NewValue)
                {
                    checkbox.FadeIn(duration, Easing.OutQuint);
                    checkbox.ScaleTo(1f, duration, Easing.OutElastic);
                    channelText.Colour = Colour4.White;
                    topicText.Colour = Colour4.White;
                }
                else
                {
                    checkbox.FadeOut(duration, Easing.OutQuint);
                    checkbox.ScaleTo(0.8f, duration, Easing.OutQuint);
                    channelText.Colour = colourProvider.Light3;
                    topicText.Colour = colourProvider.Content2;
                }
            }, true);

            Action = () => (channelJoined.Value ? OnRequestLeave : OnRequestJoin)?.Invoke(Channel);
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBox.Show();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBox.FadeOut(300, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}
