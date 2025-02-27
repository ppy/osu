// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Logging;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
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
    public partial class ParticipantPanel : CompositeDrawable, IHasContextMenu
    {
        public readonly MultiplayerRoomUser User;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private IRulesetStore rulesets { get; set; } = null!;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private SpriteIcon crown = null!;

        private OsuSpriteText userRankText = null!;
        private StyleDisplayIcon userStyleDisplay = null!;
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
                                        new UpdateableTeamFlag(user?.Team)
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Size = new Vector2(40, 20),
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
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    AutoSizeAxes = Axes.Both,
                                    Margin = new MarginPadding { Right = 70 },
                                    Spacing = new Vector2(2),
                                    Children = new Drawable[]
                                    {
                                        userStyleDisplay = new StyleDisplayIcon
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                        },
                                        userModsDisplay = new ModDisplay
                                        {
                                            Anchor = Anchor.CentreLeft,
                                            Origin = Anchor.CentreLeft,
                                            Scale = new Vector2(0.5f),
                                            ExpansionMode = ExpansionMode.AlwaysContracted,
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
                        },
                        kickButton = new KickButton
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Alpha = 0,
                            Margin = new MarginPadding(4),
                            Action = () => client.KickUser(User.UserID).FireAndForget(),
                        },
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.RoomUpdated += onRoomUpdated;
            updateState();
        }

        private void onRoomUpdated() => Scheduler.AddOnce(updateState);

        private void updateState()
        {
            if (client.Room == null || client.LocalUser == null)
                return;

            const double fade_time = 50;

            if (client.Room.GetCurrentItem() is MultiplayerPlaylistItem currentItem)
            {
                int userBeatmapId = User.BeatmapId ?? currentItem.BeatmapID;
                int userRulesetId = User.RulesetId ?? currentItem.RulesetID;
                Ruleset? userRuleset = rulesets.GetRuleset(userRulesetId)?.CreateInstance();

                int? currentModeRank = userRuleset == null ? null : User.User?.RulesetsStatistics?.GetValueOrDefault(userRuleset.ShortName)?.GlobalRank;
                userRankText.Text = currentModeRank != null ? $"#{currentModeRank.Value:N0}" : string.Empty;

                if (userBeatmapId == currentItem.BeatmapID && userRulesetId == currentItem.RulesetID)
                    userStyleDisplay.Style = null;
                else
                    userStyleDisplay.Style = (userBeatmapId, userRulesetId);

                // If the mods are updated at the end of the frame, the flow container will skip a reflow cycle: https://github.com/ppy/osu-framework/issues/4187
                // This looks particularly jarring here, so re-schedule the update to that start of our frame as a fix.
                Schedule(() => userModsDisplay.Current.Value = userRuleset == null ? Array.Empty<Mod>() : User.Mods.Select(m => m.ToMod(userRuleset)).ToList());
            }

            userStateDisplay.UpdateStatus(User.State, User.BeatmapAvailability);

            if (User.BeatmapAvailability.State == DownloadState.LocallyAvailable && User.State != MultiplayerUserState.Spectating)
            {
                userModsDisplay.FadeIn(fade_time);
                userStyleDisplay.FadeIn(fade_time);
            }
            else
            {
                userModsDisplay.FadeOut(fade_time);
                userStyleDisplay.FadeOut(fade_time);
            }

            kickButton.Alpha = client.IsHost && !User.Equals(client.LocalUser) ? 1 : 0;
            crown.Alpha = client.Room.Host?.Equals(User) == true ? 1 : 0;
        }

        public MenuItem[]? ContextMenuItems
        {
            get
            {
                if (client.Room == null)
                    return null;

                // If the local user is targetted.
                if (User.UserID == api.LocalUser.Value.Id)
                    return null;

                // If the local user is not the host of the room.
                if (client.Room.Host?.UserID != api.LocalUser.Value.Id)
                    return null;

                int targetUser = User.UserID;

                return new MenuItem[]
                {
                    new OsuMenuItem("Give host", MenuItemType.Standard, () =>
                    {
                        // Ensure the local user is still host.
                        if (!client.IsHost)
                            return;

                        client.TransferHost(targetUser).FireAndForget();
                    }),
                    new OsuMenuItem("Kick", MenuItemType.Destructive, () =>
                    {
                        // Ensure the local user is still host.
                        if (!client.IsHost)
                            return;

                        client.KickUser(targetUser).FireAndForget();
                    })
                };
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client.IsNotNull())
                client.RoomUpdated -= onRoomUpdated;
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

        private partial class StyleDisplayIcon : CompositeComponent
        {
            [Resolved]
            private BeatmapLookupCache beatmapLookupCache { get; set; } = null!;

            [Resolved]
            private RulesetStore rulesets { get; set; } = null!;

            public StyleDisplayIcon()
            {
                AutoSizeAxes = Axes.Both;
            }

            private (int beatmap, int ruleset)? style;

            public (int beatmap, int ruleset)? Style
            {
                get => style;
                set
                {
                    if (style == value)
                        return;

                    style = value;
                    Scheduler.Add(refresh);
                }
            }

            private CancellationTokenSource? cancellationSource;

            private void refresh()
            {
                cancellationSource?.Cancel();
                cancellationSource?.Dispose();
                cancellationSource = null;

                if (Style == null)
                {
                    ClearInternal();
                    return;
                }

                cancellationSource = new CancellationTokenSource();
                CancellationToken token = cancellationSource.Token;

                int localBeatmap = Style.Value.beatmap;
                int localRuleset = Style.Value.ruleset;

                Task.Run(async () =>
                {
                    try
                    {
                        var beatmap = await beatmapLookupCache.GetBeatmapAsync(localBeatmap, token).ConfigureAwait(false);
                        if (beatmap == null)
                            return;

                        Schedule(() =>
                        {
                            if (token.IsCancellationRequested)
                                return;

                            InternalChild = new DifficultyIcon(beatmap, rulesets.GetRuleset(localRuleset))
                            {
                                Size = new Vector2(20),
                                TooltipType = DifficultyIconTooltipType.Extended,
                            };
                        });
                    }
                    catch (Exception e)
                    {
                        Logger.Log($"Error while populating participant style icon {e}");
                    }
                }, token);
            }
        }
    }
}
