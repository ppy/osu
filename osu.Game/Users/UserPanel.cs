// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics.UserInterface;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Screens;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Localisation;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Users.Drawables;
using osuTK;

namespace osu.Game.Users
{
    public abstract partial class UserPanel : OsuClickableContainer, IHasContextMenu
    {
        public readonly APIUser User;

        /// <summary>
        /// Perform an action in addition to showing the user's profile.
        /// This should be used to perform auxiliary tasks and not as a primary action for clicking a user panel (to maintain a consistent UX).
        /// </summary>
        public new Action? Action;

        protected Action ViewProfile { get; private set; } = null!;

        protected Drawable Background { get; private set; } = null!;

        protected UserPanel(APIUser user)
            : base(HoverSampleSet.Button)
        {
            ArgumentNullException.ThrowIfNull(user);

            User = user;
        }

        [Resolved]
        private UserProfileOverlay? profileOverlay { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private ChannelManager? channelManager { get; set; }

        [Resolved]
        private ChatOverlay? chatOverlay { get; set; }

        [Resolved]
        protected OverlayColourProvider? ColourProvider { get; private set; }

        [Resolved]
        private IPerformFromScreenRunner? performer { get; set; }

        [Resolved]
        protected OsuColour Colours { get; private set; } = null!;

        [Resolved]
        private MultiplayerClient? multiplayerClient { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            Masking = true;

            Add(new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = ColourProvider?.Background5 ?? Colours.Gray1
            });

            var background = CreateBackground();
            if (background != null)
                Add(background);

            Add(CreateLayout());

            base.Action = ViewProfile = () =>
            {
                Action?.Invoke();
                profileOverlay?.ShowUser(User);
            };
        }

        protected abstract Drawable CreateLayout();

        /// <summary>
        /// Panel background container. Can be null if a panel doesn't want a background under it's layout
        /// </summary>
        protected virtual Drawable? CreateBackground() => Background = new UserCoverBackground
        {
            RelativeSizeAxes = Axes.Both,
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            User = User
        };

        protected OsuSpriteText CreateUsername() => new OsuSpriteText
        {
            Font = OsuFont.GetFont(size: 16, weight: FontWeight.Bold),
            Shadow = false,
            Text = User.Username,
        };

        protected UpdateableAvatar CreateAvatar() => new UpdateableAvatar(User, false);

        protected UpdateableFlag CreateFlag() => new UpdateableFlag(User.CountryCode)
        {
            Size = new Vector2(36, 26),
            Action = Action,
        };

        public MenuItem[] ContextMenuItems
        {
            get
            {
                List<MenuItem> items = new List<MenuItem>
                {
                    new OsuMenuItem(ContextMenuStrings.ViewProfile, MenuItemType.Highlighted, ViewProfile)
                };

                if (User.Equals(api.LocalUser.Value))
                    return items.ToArray();

                items.Add(new OsuMenuItem(UsersStrings.CardSendMessage, MenuItemType.Standard, () =>
                {
                    channelManager?.OpenPrivateChannel(User);
                    chatOverlay?.Show();
                }));

                if (User.IsOnline)
                {
                    items.Add(new OsuMenuItem(ContextMenuStrings.SpectatePlayer, MenuItemType.Standard, () =>
                    {
                        performer?.PerformFromScreen(s => s.Push(new SoloSpectatorScreen(User)));
                    }));

                    if (multiplayerClient?.Room?.Users.All(u => u.UserID != User.Id) == true)
                    {
                        items.Add(new OsuMenuItem(ContextMenuStrings.InvitePlayer, MenuItemType.Standard, () => multiplayerClient.InvitePlayer(User.Id)));
                    }
                }

                return items.ToArray();
            }
        }
    }
}
