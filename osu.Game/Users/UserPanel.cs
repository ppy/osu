// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics.Containers;
using JetBrains.Annotations;

namespace osu.Game.Users
{
    public abstract class UserPanel : OsuClickableContainer, IHasContextMenu
    {
        public readonly User User;

        public new Action Action;

        protected Action ViewProfile { get; private set; }

        protected DelayedLoadUnloadWrapper Background { get; private set; }

        protected UserPanel(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            User = user;
        }

        [Resolved(canBeNull: true)]
        private UserProfileOverlay profileOverlay { get; set; }

        [Resolved(canBeNull: true)]
        protected OverlayColourProvider ColourProvider { get; private set; }

        [Resolved]
        protected OsuColour Colours { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;

            AddRange(new[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider?.Background5 ?? Colours.Gray1
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

            base.Action = ViewProfile = () =>
            {
                Action?.Invoke();
                profileOverlay?.ShowUser(User);
            };
        }

        [NotNull]
        protected abstract Drawable CreateLayout();

        protected OsuSpriteText CreateUsername() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
            Shadow = false,
            Text = User.Username,
        };

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("View Profile", MenuItemType.Highlighted, ViewProfile),
        };
    }
}
