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

namespace osu.Game.Graphics.UserInterfaceV2.Users
{
    public abstract class UserCard : OsuHoverContainer, IHasContextMenu
    {
        [Resolved(canBeNull:true)]
        private UserProfileOverlay profileOverlay { get; set; }

        protected override IEnumerable<Drawable> EffectTargets => null;

        public User User { get; }

        public UserCard(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            User = user;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Action = () => profileOverlay?.ShowUser(User);

            Masking = true;
            BorderColour = colours.GreyVioletLighter;

            Add(new DelayedLoadUnloadWrapper(() => new UserCoverBackground
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                User = User,
            }, 300, 5000)
            {
                RelativeSizeAxes = Axes.Both,
            });
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
        };
    }
}
