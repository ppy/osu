// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Containers;
using osu.Game.Users;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using System.Collections.Generic;
using osu.Framework.Input.Events;
using osu.Framework.Graphics.Shapes;
using JetBrains.Annotations;
using osu.Game.Users.Drawables;
using osuTK;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using osuTK.Graphics;
using osu.Game.Online.Chat;

namespace osu.Game.Graphics.UserInterfaceV2.Users
{
    public abstract class UserCard : OsuHoverContainer, IHasContextMenu
    {
        public User User { get; }

        protected override IEnumerable<Drawable> EffectTargets => null;

        protected DelayedLoadUnloadWrapper Background;

        protected UserCard(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            User = user;
        }

        [Resolved(canBeNull: true)]
        private UserProfileOverlay profileOverlay { get; set; }

        [Resolved(canBeNull: true)]
        private ChannelManager channelManager { get; set; }

        [Resolved(canBeNull: true)]
        private ChatOverlay chatOverlay { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Action = () => profileOverlay?.ShowUser(User);

            Masking = true;
            BorderColour = colours.GreyVioletLighter;

            AddRange(new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5
                },
                Background = new DelayedLoadUnloadWrapper(() => new UserCoverBackground
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    User = User,
                }, 300, 5000)
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    RelativeSizeAxes = Axes.Both,
                },
                CreateLayout()
            });
        }

        [NotNull]
        protected abstract Drawable CreateLayout();

        protected UpdateableAvatar CreateAvatar() => new UpdateableAvatar
        {
            User = User,
            OpenOnClick = { Value = false }
        };

        protected UpdateableFlag CreateFlag() => new UpdateableFlag(User.Country)
        {
            Size = new Vector2(39, 26)
        };

        protected OsuSpriteText CreateUsername() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold, italics: true),
            Text = User.Username,
        };

        protected SpriteIcon CreateStatusIcon() => new SpriteIcon
        {
            Icon = FontAwesome.Regular.Circle,
            Size = new Vector2(25),
            Colour = User.IsOnline ? colours.GreenLight : Color4.Black
        };

        protected FillFlowContainer CreateStatusMessage(bool rightAlignedChildren)
        {
            var status = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical
            };

            var alignment = rightAlignedChildren ? Anchor.x2 : Anchor.x0;

            if (!User.IsOnline && User.LastVisit.HasValue)
            {
                status.Add(new TextFlowContainer(t => t.Font = OsuFont.GetFont(size: 15)).With(text =>
                {
                    text.Anchor = Anchor.y1 | alignment;
                    text.Origin = Anchor.y1 | alignment;
                    text.AutoSizeAxes = Axes.Both;
                    text.AddText(@"Last seen ");
                    text.AddText(new DrawableDate(User.LastVisit.Value, italic: false));
                }));
            }

            status.Add(new OsuSpriteText
            {
                Anchor = Anchor.y1 | alignment,
                Origin = Anchor.y1 | alignment,
                Font = OsuFont.GetFont(size: 17),
                Text = User.IsOnline ? @"Online" : @"Offline"
            });

            return status;
        }

        protected override bool OnHover(HoverEvent e)
        {
            BorderThickness = 2;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            BorderThickness = 0;
            base.OnHoverLost(e);
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("View Profile", MenuItemType.Highlighted, Action),
            new OsuMenuItem("Send message", MenuItemType.Standard, () =>
            {
                channelManager?.OpenPrivateChannel(User);
                chatOverlay?.Show();
            })
        };
    }
}
