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

namespace osu.Game.Overlays.NewChat
{
    public class ChannelListingItem : OsuClickableContainer, IFilterable
    {
        public event Action<Channel>? OnRequestJoin;
        public event Action<Channel>? OnRequestLeave;

        public bool FilteringActive { get; set; }
        public IEnumerable<string> FilterTerms => new[] { channel.Name, channel.Topic ?? string.Empty };
        public bool MatchingFilter
        {
            set => this.FadeTo(value ? 1f : 0f, 100);
        }

        private readonly float TEXT_SIZE = 18;
        private readonly float ICON_SIZE = 14;
        private readonly Channel channel;

        private Colour4 selectedColour;
        private Colour4 normalColour;

        private Box hoverBox = null!;
        private SpriteIcon checkbox = null!;
        private OsuSpriteText channelText = null!;
        private IBindable<bool> channelJoined = null!;

        [Resolved]
        private OverlayColourProvider overlayColours { get; set; } = null!;

        public ChannelListingItem(Channel channel)
        {
            this.channel = channel;

            Masking = true;
            CornerRadius = 5;
            RelativeSizeAxes = Axes.X;
            Height = 20;
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverBox.Show();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverBox.Hide();
            base.OnHoverLost(e);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Set colours
            normalColour = overlayColours.Light3;
            selectedColour = Colour4.White;

            // Set handlers for state display
            channelJoined = channel.Joined.GetBoundCopy();
            channelJoined.BindValueChanged(change =>
            {
                if (change.NewValue)
                {
                    checkbox.Show();
                    channelText.Colour = selectedColour;
                }
                else
                {
                    checkbox.Hide();
                    channelText.Colour = normalColour;
                }
            }, true);

            // Set action on click
            Action = () => (channelJoined.Value ? OnRequestLeave : OnRequestJoin)?.Invoke(channel);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                hoverBox = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = overlayColours.Background3,
                    Alpha = 0f,
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new Dimension[]
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
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Left = 15 },
                                Icon = FontAwesome.Solid.Check,
                                Size = new Vector2(ICON_SIZE),
                            },
                            channelText = new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = $"# {channel.Name.Substring(1)}",
                                Font = OsuFont.Torus.With(size: TEXT_SIZE, weight: FontWeight.Medium),
                                Margin = new MarginPadding { Bottom = 2 },
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = channel.Topic,
                                Font = OsuFont.Torus.With(size: TEXT_SIZE),
                                Margin = new MarginPadding { Bottom = 2 },
                                Colour = Colour4.White,
                            },
                            new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Icon = FontAwesome.Solid.User,
                                Size = new Vector2(ICON_SIZE),
                                Margin = new MarginPadding { Right = 5 },
                                Colour = overlayColours.Light3,
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Text = "0",
                                Font = OsuFont.Numeric.With(size: TEXT_SIZE, weight: FontWeight.Medium),
                                Margin = new MarginPadding { Bottom = 2 },
                                Colour = overlayColours.Light3,
                            },
                        },
                    },
                },
            };
        }
    }
}
