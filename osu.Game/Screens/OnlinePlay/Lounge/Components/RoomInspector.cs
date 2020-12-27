// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Screens.OnlinePlay.Components;
using osuTK.Graphics;

namespace osu.Game.Screens.OnlinePlay.Lounge.Components
{
    public class RoomInspector : OnlinePlayComposite
    {
        private const float transition_duration = 100;

        private readonly MarginPadding contentPadding = new MarginPadding { Horizontal = 20, Vertical = 10 };

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            OverlinedHeader participantsHeader;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.25f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = 30 },
                    Child = new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Children = new Drawable[]
                                    {
                                        new RoomInfo
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Margin = new MarginPadding { Vertical = 60 },
                                        },
                                        participantsHeader = new OverlinedHeader("Recent Participants"),
                                        new ParticipantsDisplay(Direction.Vertical)
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = ParticipantsList.TILE_SIZE * 3,
                                            Details = { BindTarget = participantsHeader.Details }
                                        }
                                    }
                                }
                            },
                            new Drawable[] { new OverlinedPlaylistHeader(), },
                            new Drawable[]
                            {
                                new DrawableRoomPlaylist(false, false)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Items = { BindTarget = Playlist }
                                },
                            },
                        },
                        RowDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(),
                        }
                    }
                }
            };
        }
    }
}
