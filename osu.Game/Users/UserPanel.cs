// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Users
{
    public class UserPanel : Container
    {
        private const float height = 100;
        private const float content_padding = 10;
        private const float status_height = 30;

        private OsuColour colours;

        private readonly Container statusBar;
        private readonly Box statusBg;
        private readonly OsuSpriteText statusMessage;

        public readonly Bindable<UserStatus> Status = new Bindable<UserStatus>();

        public UserPanel(User user)
        {
            Height = height - status_height;
            Masking = true;
            CornerRadius = 5;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Color4.Black.Opacity(0.25f),
                Radius = 4,
            };

            Children = new Drawable[]
            {
                new AsyncLoadWrapper(new CoverBackgroundSprite(user)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    FillMode = FillMode.Fill,
                    OnLoadComplete = d => d.FadeInFromZero(200),
                }) { RelativeSizeAxes = Axes.Both },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.7f),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding { Top = content_padding, Left = content_padding, Right = content_padding },
                    Children = new Drawable[]
                    {
                        new UpdateableAvatar
                        {
                            Size = new Vector2(height - status_height - content_padding * 2),
                            User = user,
                            Masking = true,
                            CornerRadius = 5,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(0.25f),
                                Radius = 4,
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = height - status_height - content_padding },
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = user.Username,
                                    TextSize = 18,
                                    Font = @"Exo2.0-SemiBoldItalic",
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    AutoSizeAxes = Axes.X,
                                    Height = 20f,
                                    Direction = FillDirection.Horizontal,
                                    Spacing = new Vector2(5f, 0f),
                                    Children = new Drawable[]
                                    {
                                        new DrawableFlag(user.Country?.FlagName ?? @"__")
                                        {
                                            Width = 30f,
                                            RelativeSizeAxes = Axes.Y,
                                        },
                                        new Container
                                        {
                                            Width = 40f,
                                            RelativeSizeAxes = Axes.Y,
                                        },
                                        new CircularContainer
                                        {
                                            Width = 20f,
                                            RelativeSizeAxes = Axes.Y,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                statusBar = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Alpha = 0f,
                    Children = new Drawable[]
                    {
                        statusBg = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.5f,
                        },
                        new FillFlowContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Both,
                            Spacing = new Vector2(5f, 0f),
                            Children = new[]
                            {
                                new TextAwesome
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Icon = FontAwesome.fa_circle_o,
                                    Shadow = true,
                                    TextSize = 14,
                                },
                                statusMessage = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Font = @"Exo2.0-Semibold",
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
            this.colours = colours;
            Status.ValueChanged += displayStatus;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Status.TriggerChange();
        }

        private void displayStatus(UserStatus status)
        {
            const float transition_duration = 500;

            if (status == null)
            {
                statusBar.ResizeHeightTo(0f, transition_duration, EasingTypes.OutQuint);
                statusBar.FadeOut(transition_duration, EasingTypes.OutQuint);
                ResizeHeightTo(height - status_height, transition_duration, EasingTypes.OutQuint);
            }
            else
            {
                statusBar.ResizeHeightTo(status_height, transition_duration, EasingTypes.OutQuint);
                statusBar.FadeIn(transition_duration, EasingTypes.OutQuint);
                ResizeHeightTo(height, transition_duration, EasingTypes.OutQuint);

                statusBg.FadeColour(status.GetAppropriateColour(colours), 500, EasingTypes.OutQuint);
                statusMessage.Text = status.Message;
            }
        }

        private class CoverBackgroundSprite : Sprite
        {
            private readonly User user;

            public CoverBackgroundSprite(User user)
            {
                this.user = user;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                if (!string.IsNullOrEmpty(user.CoverUrl))
                    Texture = textures.Get(user.CoverUrl);
            }
        }
    }
}
