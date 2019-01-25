// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Chat.Selection
{
    public class ChannelListItem : OsuClickableContainer, IFilterable
    {
        private const float width_padding = 5;
        private const float channel_width = 150;
        private const float text_size = 15;
        private const float transition_duration = 100;

        private readonly Channel channel;

        private readonly Bindable<bool> joinedBind = new Bindable<bool>();
        private readonly OsuSpriteText name;
        private readonly OsuSpriteText topic;
        private readonly SpriteIcon joinedCheckmark;

        private Color4 joinedColour;
        private Color4 topicColour;
        private Color4 hoverColour;

        public IEnumerable<string> FilterTerms => new[] { channel.Name };
        public bool MatchingFilter
        {
            set
            {
                this.FadeTo(value ? 1f : 0f, 100);
            }
        }

        public Action<Channel> OnRequestJoin;
        public Action<Channel> OnRequestLeave;

        public ChannelListItem(Channel channel)
        {
            this.channel = channel;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Action = () => { (channel.Joined ? OnRequestLeave : OnRequestJoin)?.Invoke(channel); };

            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Children = new[]
                            {
                                joinedCheckmark = new SpriteIcon
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Icon = FontAwesome.fa_check_circle,
                                    Size = new Vector2(text_size),
                                    Shadow = false,
                                    Margin = new MarginPadding { Right = 10f },
                                },
                            },
                        },
                        new Container
                        {
                            Width = channel_width,
                            AutoSizeAxes = Axes.Y,
                            Children = new[]
                            {
                                name = new OsuSpriteText
                                {
                                    Text = channel.ToString(),
                                    TextSize = text_size,
                                    Font = @"Exo2.0-Bold",
                                    Shadow = false,
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = 0.7f,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = width_padding },
                            Children = new[]
                            {
                                topic = new OsuSpriteText
                                {
                                    Text = channel.Topic,
                                    TextSize = text_size,
                                    Font = @"Exo2.0-SemiBold",
                                    Shadow = false,
                                },
                            },
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Margin = new MarginPadding { Left = width_padding },
                            Spacing = new Vector2(3f, 0f),
                            Children = new Drawable[]
                            {
                                new SpriteIcon
                                {
                                    Icon = FontAwesome.fa_user,
                                    Size = new Vector2(text_size - 2),
                                    Shadow = false,
                                    Margin = new MarginPadding { Top = 1 },
                                },
                                new OsuSpriteText
                                {
                                    Text = @"0",
                                    TextSize = text_size,
                                    Font = @"Exo2.0-SemiBold",
                                    Shadow = false,
                                },
                            },
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            topicColour = colours.Gray9;
            joinedColour = colours.Blue;
            hoverColour = colours.Yellow;

            joinedBind.ValueChanged += updateColour;
            joinedBind.BindTo(channel.Joined);

            joinedBind.TriggerChange();
            FinishTransforms(true);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (!channel.Joined.Value)
                name.FadeColour(hoverColour, 50, Easing.OutQuint);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (!channel.Joined.Value)
                name.FadeColour(Color4.White, transition_duration);
        }

        private void updateColour(bool joined)
        {
            if (joined)
            {
                name.FadeColour(Color4.White, transition_duration);
                joinedCheckmark.FadeTo(1f, transition_duration);
                topic.FadeTo(0.8f, transition_duration);
                topic.FadeColour(Color4.White, transition_duration);
                this.FadeColour(joinedColour, transition_duration);
            }
            else
            {
                joinedCheckmark.FadeTo(0f, transition_duration);
                topic.FadeTo(1f, transition_duration);
                topic.FadeColour(topicColour, transition_duration);
                this.FadeColour(Color4.White, transition_duration);
            }
        }
    }
}
