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

namespace osu.Game.Graphics.UserInterfaceV2.Users
{
    public abstract class UserCard : OsuHoverContainer, IHasContextMenu
    {
        public User User { get; }

        protected override IEnumerable<Drawable> EffectTargets => null;

        protected DelayedLoadUnloadWrapper Background;

        public UserCard(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            User = user;
        }

        [Resolved(canBeNull: true)]
        private UserProfileOverlay profileOverlay { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            Action = () => profileOverlay?.ShowUser(User);

            Masking = true;
            BorderColour = colours.GreyVioletLighter;

            AddRange(new Drawable[]
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
                }
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
