// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Multiplayer.Participants
{
    public partial class ParticipantPanel : MultiplayerRoomComposite, IHasContextMenu
    {
        public readonly MultiplayerRoomUser User;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        private SpriteIcon crown = null!;

        private OsuSpriteText userRankText = null!;
        private ModDisplay userModsDisplay = null!;
        private StateDisplay userStateDisplay = null!;

        private IconButton kickButton = null!;

        public ParticipantPanel(MultiplayerRoomUser user)
        {
            User = user;

            RelativeSizeAxes = Axes.X;
            Height = 40;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var user = User.User;

            var backgroundColour = Color4Extensions.FromHex("#33413C");

            InternalChild = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 18),
                    new Dimension(GridSizeMode.AutoSize),
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                },
                Content = new[]
                {
                    new Drawable[]
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
                        new TeamDisplay(User),
                        new Container
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
                                    User = user,
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
                                            User = user
                                        },
                                        new UpdateableFlag
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(28, 20),
                                            CountryCode = user?.CountryCode ?? default
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 18),
                                            Text = user?.Username ?? string.Empty
                                        },
                                        userRankText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Font = OsuFont.GetFont(size: 14),
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Right = 70 },
                                    Child = userModsDisplay = new ModDisplay
                                    {
                                        Scale = new Vector2(0.5f),
                                        ExpansionMode = ExpansionMode.AlwaysContracted,
                                    }
                                },
                                userStateDisplay = new StateDisplay
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Margin = new MarginPadding { Right = 10 },
                                }
                            }
                        },
                        kickButton = new KickButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Margin = new MarginPadding(4),
                            Action = () => Client.KickUser(User.UserID).FireAndForget(),
                        },
                    },
                }
            };
        }

        protected override void OnRoomUpdated()
        {
            base.OnRoomUpdated();

            if (Room == null || Client.LocalUser == null)
                return;

            const double fade_time = 50;

            var currentItem = Playlist.GetCurrentItem();
            Ruleset? ruleset = currentItem != null ? rulesets.GetRuleset(currentItem.RulesetID)?.CreateInstance() : null;

            int? currentModeRank = ruleset != null ? User.User?.RulesetsStatistics?.GetValueOrDefault(ruleset.ShortName)?.GlobalRank : null;
            userRankText.Text = currentModeRank != null ? $"#{currentModeRank.Value:N0}" : string.Empty;

            userStateDisplay.UpdateStatus(User.State, User.BeatmapAvailability);

            if ((User.BeatmapAvailability.State == DownloadState.LocallyAvailable) && (User.State != MultiplayerUserState.Spectating))
                userModsDisplay.FadeIn(fade_time);
            else
                userModsDisplay.FadeOut(fade_time);

            kickButton.Alpha = Client.IsHost && !User.Equals(Client.LocalUser) ? 1 : 0;
            crown.Alpha = Room.Host?.Equals(User) == true ? 1 : 0;

            // If the mods are updated at the end of the frame, the flow container will skip a reflow cycle: https://github.com/ppy/osu-framework/issues/4187
            // This looks particularly jarring here, so re-schedule the update to that start of our frame as a fix.
            Schedule(() =>
            {
                userModsDisplay.Current.Value = ruleset != null ? User.Mods.Select(m => m.ToMod(ruleset)).ToList() : Array.Empty<Mod>();
            });
        }

        public MenuItem[]? ContextMenuItems
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
                        if (!Client.IsHost)
                            return;

                        Client.TransferHost(targetUser).FireAndForget();
                    }),
                    new OsuMenuItem("Kick", MenuItemType.Destructive, () =>
                    {
                        // Ensure the local user is still host.
                        if (!Client.IsHost)
                            return;

                        Client.KickUser(targetUser).FireAndForget();
                    })
                };
            }
        }

        public partial class KickButton : IconButton
        {
            public KickButton()
            {
                Icon = FontAwesome.Solid.UserTimes;
                TooltipText = "Kick";
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconHoverColour = colours.Red;
            }
        }
    }
}
