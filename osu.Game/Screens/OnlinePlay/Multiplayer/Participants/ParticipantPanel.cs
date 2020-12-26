// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public class ParticipantPanel : MultiplayerRoomComposite, IHasContextMenu
    {
        public readonly MultiplayerRoomUser User;

        [Resolved]
        private IAPIProvider api { get; set; }

        private StateDisplay userStateDisplay;
        private SpriteIcon crown;

        public ParticipantPanel(MultiplayerRoomUser user)
        {
            User = user;

            RelativeSizeAxes = Axes.X;
            Height = 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Debug.Assert(User.User != null);

            var backgroundColour = Color4Extensions.FromHex("#33413C");

            InternalChildren = new Drawable[]
            {
                crown = new SpriteIcon
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Icon = FontAwesome.Solid.Crown,
                    Size = new Vector2(14),
                    Colour = Color4Extensions.FromHex("#F7E65D"),
                    Alpha = 0
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Left = 24 },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 5,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = backgroundColour
                            },
                            new UserCoverBackground
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                RelativeSizeAxes = Axes.Both,
                                Width = 0.75f,
                                User = User.User,
                                Colour = ColourInfo.GradientHorizontal(Color4.White.Opacity(0), Color4.White.Opacity(0.25f))
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Spacing = new Vector2(10),
                                Direction = FillDirection.Horizontal,
                                Children = new Drawable[]
                                {
                                    new UpdateableAvatar
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        FillMode = FillMode.Fit,
                                        User = User.User
                                    },
                                    new UpdateableFlag
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Size = new Vector2(30, 20),
                                        Country = User.User.Country
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 18),
                                        Text = User.User.Username
                                    },
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Font = OsuFont.GetFont(size: 14),
                                        Text = User.User.CurrentModeRank != null ? $"#{User.User.CurrentModeRank}" : string.Empty
                                    }
                                }
                            },
                            userStateDisplay = new StateDisplay
                            {
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Margin = new MarginPadding { Right = 10 },
                            }
                        }
                    }
                }
            };
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            if (Room == null)
                return;

            const double fade_time = 50;

            userStateDisplay.Status = User.State;

            if (Room.Host?.Equals(User) == true)
                crown.FadeIn(fade_time);
            else
                crown.FadeOut(fade_time);
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (Room == null)
                    return null;

                // If the local user is targetted.
                if (User.UserID == api.LocalUser.Value.Id)
                    return null;

                // If the local user is not the host of the room.
                if (Room.Host?.UserID != api.LocalUser.Value.Id)
                    return null;

                int targetUser = User.UserID;

                return new MenuItem[]
                {
                    new OsuMenuItem("Give host", MenuItemType.Standard, () =>
                    {
                        // Ensure the local user is still host.
                        if (Room.Host?.UserID != api.LocalUser.Value.Id)
                            return;

                        Client.TransferHost(targetUser).CatchUnobservedExceptions(true);
                    })
                };
            }
        }
    }
}
