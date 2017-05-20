// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Chat;

namespace osu.Game.Overlays.Chat
{
    public class ChannelListItem : ClickableContainer
    {
        private const float width_padding = 5;
        private const float channel_width = 150;
        private const float topic_width = 380;
        private const float text_size = 15;
        private const float transition_duration = 100;

        private readonly OsuSpriteText topic;
        private readonly OsuSpriteText name;
        private readonly TextAwesome joinedCheckmark;

        private Color4? joinedColour;
        private Color4? topicColour;

        private Channel channel;
        public Channel Channel
        {
            get { return channel; }
            set
            {
                if (value == channel) return;
                channel = value;

                name.Text = Channel.ToString();
                topic.Text = Channel.Topic;
                updateColour();
            }
        }

        public ChannelListItem()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

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
                                joinedCheckmark = new TextAwesome
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Icon = FontAwesome.fa_check_circle,
                                    TextSize = text_size,
                                    Margin = new MarginPadding { Right = 10f },
                                    Alpha = 0f,
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
                                    TextSize = text_size,
                                    Font = @"Exo2.0-Bold",
                                },
                            },
                        },
                        new Container
                        {
                            Width = topic_width,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Left = width_padding },
                            Children = new[]
                            {
                                topic = new OsuSpriteText
                                {
                                    TextSize = text_size,
                                    Font = @"Exo2.0-SemiBold",
                                    Alpha = 0.8f,
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
                                new TextAwesome
                                {
                                    Icon = FontAwesome.fa_user,
                                    TextSize = text_size - 2,
                                    Margin = new MarginPadding { Top = 1 },
                                },
                                new OsuSpriteText
                                {
                                    Text = @"0",
                                    TextSize = text_size,
                                    Font = @"Exo2.0-SemiBold",
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

            updateColour();
        }

        private void updateColour()
        {
            joinedCheckmark.FadeTo(Channel.Joined ? 1f : 0f, transition_duration);
            topic.FadeTo(Channel.Joined ? 0.8f : 1f, transition_duration);
            topic.FadeColour(Channel.Joined ? Color4.White : topicColour ?? Color4.White, transition_duration);
            FadeColour(Channel.Joined ? joinedColour ?? Color4.White : Color4.White, transition_duration);
        }
    }
}
