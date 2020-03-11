// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Online.Chat;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Chat.Tabs
{
    public class PrivateChannelTabItem : ChannelTabItem
    {
        protected override IconUsage DisplayIcon => FontAwesome.Solid.At;

        public PrivateChannelTabItem(Channel value)
            : base(value)
        {
            if (value.Type != ChannelType.PM)
                throw new ArgumentException("Argument value needs to have the targettype user!");

            DrawableAvatar avatar;

            AddRange(new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Margin = new MarginPadding
                    {
                        Horizontal = 3
                    },
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Children = new Drawable[]
                    {
                        new CircularContainer
                        {
                            Scale = new Vector2(0.95f),
                            Size = new Vector2(ChatOverlay.TAB_AREA_HEIGHT),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Masking = true,
                            Child = new DelayedLoadWrapper(avatar = new DrawableAvatar(value.Users.First())
                            {
                                RelativeSizeAxes = Axes.Both,
                                OpenOnClick = { Value = false },
                            })
                            {
                                RelativeSizeAxes = Axes.Both,
                            }
                        },
                    }
                },
            });

            avatar.OnLoadComplete += d => d.FadeInFromZero(300, Easing.OutQuint);
        }

        protected override float LeftTextPadding => base.LeftTextPadding + ChatOverlay.TAB_AREA_HEIGHT;

        protected override bool ShowCloseOnHover => false;

        protected override void FadeActive()
        {
            base.FadeActive();

            this.ResizeWidthTo(200, TRANSITION_LENGTH, Easing.OutQuint);
            CloseButton.FadeIn(TRANSITION_LENGTH, Easing.OutQuint);
        }

        protected override void FadeInactive()
        {
            base.FadeInactive();

            this.ResizeWidthTo(ChatOverlay.TAB_AREA_HEIGHT + 10, TRANSITION_LENGTH, Easing.OutQuint);
            CloseButton.FadeOut(TRANSITION_LENGTH, Easing.OutQuint);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            var user = Value.Users.First();

            BackgroundActive = user.Colour != null ? Color4Extensions.FromHex(user.Colour) : colours.BlueDark;
            BackgroundInactive = BackgroundActive.Darken(0.5f);
        }
    }
}
