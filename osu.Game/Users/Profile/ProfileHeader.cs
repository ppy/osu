// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Users.Profile
{
    public class ProfileHeader : Container
    {
        private readonly User user;

        private const float cover_height = 200, avatar_size = 110, avatar_bottom_position = -20;
        public ProfileHeader(User user)
        {
            this.user = user;
            RelativeSizeAxes = Axes.X;
            Height = cover_height;

            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = cover_height,
                    Children = new Drawable[]
                    {
                        new AsyncLoadWrapper(new UserCoverBackground(user)
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                            OnLoadComplete = d => d.FadeInFromZero(200)
                        })
                        {
                            Masking = true,
                            RelativeSizeAxes = Axes.Both
                        },
                        new UpdateableAvatar
                        {
                            User = user,
                            Size = new Vector2(avatar_size),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfile.CONTENT_X_MARGIN,
                            Y = avatar_bottom_position,
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
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            X = UserProfile.CONTENT_X_MARGIN + avatar_size + 10,
                            Y = avatar_bottom_position,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = user.Username,
                                    TextSize = 25,
                                    Font = @"Exo2.0-RegularItalic",
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Y = -55
                                },
                                new DrawableFlag(user.Country?.FlagName ?? "__")
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Width = 30,
                                    Height = 20
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
