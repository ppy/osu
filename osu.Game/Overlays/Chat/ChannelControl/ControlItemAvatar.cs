// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Online.Chat;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Overlays.Chat.ChannelControl
{
    public class ControlItemAvatar : CircularContainer
    {
        private readonly Channel channel;

        private SpriteIcon? placeholder;
        private DrawableAvatar? avatar;

        public ControlItemAvatar(Channel channel)
        {
            this.channel = channel;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Size = new Vector2(20);
            Margin = new MarginPadding { Right = 5 };
            Masking = true;

            Children = new Drawable[]
            {
                placeholder = new SpriteIcon
                {
                    Icon = FontAwesome.Solid.At,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Colour4.White,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.5f,
                },
                new DelayedLoadWrapper(avatar = new DrawableAvatar(channel.Users.First())
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            avatar!.OnLoadComplete += _ => placeholder!.FadeOut(250);
        }
    }
}
