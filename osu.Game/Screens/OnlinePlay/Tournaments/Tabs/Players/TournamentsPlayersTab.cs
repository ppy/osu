// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Containers.Draggable;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Players.Components;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Players
{
    public partial class TournamentsPlayersTab : TournamentsBaseTab
    {
        public override TournamentsTab TabType => TournamentsTab.Players;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            Origin = Anchor.Centre;
            Anchor = Anchor.Centre;
            Masking = true;
            MaskingSmoothness = 0.5f;
            InternalChild = new OsuDraggableSharingContainer<TournamentUser>()
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Child = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new Container // Will be sub-tab manager
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.33f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Colour = new Colour4(34,34,34,255)
                                },
                                new TournamentsTeamBlockCopy()
                                {
                                    Items = {
                                        new TournamentUser() { OnlineID = 21 },
                                        new TournamentUser() { OnlineID = 22 },
                                        new TournamentUser() { OnlineID = 23 },
                                        new TournamentUser() { OnlineID = 24 },
                                    }
                                },
                            }
                        },
                        new Container // Will be where sub-tab info is displayed.
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.66f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Colour = new Colour4(255,34,34,128)
                                },

                                // todo : on some resolutions the children of this container will stick out on the top or bottom.
                                new OsuScrollContainer(Direction.Vertical)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Origin = Anchor.Centre,
                                    Anchor = Anchor.Centre,
                                    Child = new FillFlowContainer()
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                        Direction = FillDirection.Full,
                                        Spacing = new Vector2(25),
                                        Padding = new MarginPadding(10),
                                        Children = new Drawable[]
                                        {
                                            new TournamentsTeamBlock()
                                            {
                                                Items = {
                                                    new TournamentUser() { OnlineID = 1 },
                                                    new TournamentUser() { OnlineID = 2 },
                                                    new TournamentUser() { OnlineID = 3 },
                                                    new TournamentUser() { OnlineID = 4 },
                                                }
                                            },
                                            new TournamentsTeamBlock()
                                            {
                                                Items = {
                                                    new TournamentUser() { OnlineID = 10 },
                                                    new TournamentUser() { OnlineID = 20 },
                                                    new TournamentUser() { OnlineID = 30 },
                                                    new TournamentUser() { OnlineID = 40 },
                                                    new TournamentUser() { OnlineID = 50 },
                                                    new TournamentUser() { OnlineID = 60 },
                                                }
                                            },
                                            new TournamentsTeamBlock(),
                                            new TournamentsTeamBlock(),
                                            new TournamentsTeamBlock(),
                                            new TournamentsTeamBlock()
                                            {
                                                Items = {
                                                    new TournamentUser() { OnlineID = 100 },
                                                    new TournamentUser() { OnlineID = 200 },
                                                    new TournamentUser() { OnlineID = 300 },
                                                    new TournamentUser() { OnlineID = 400 },
                                                    new TournamentUser() { OnlineID = 500 },
                                                }
                                            },
                                            new TournamentsTeamBlock(),
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
