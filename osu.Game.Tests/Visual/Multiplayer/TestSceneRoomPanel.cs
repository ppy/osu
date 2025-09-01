// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneRoomPanel : OsuTestScene
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private readonly Bindable<Room?> selectedRoom = new Bindable<Room?>();

        [Test]
        public void TestMultipleStatuses()
        {
            FillFlowContainer rooms = null!;

            AddStep("create rooms", () =>
            {
                PlaylistItem item1 = new PlaylistItem(new APIBeatmap
                {
                    OnlineBeatmapSetID = 173612,
                    OnlineID = 502132,
                });

                PlaylistItem item2 = new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo = { StarRating = 4.5 }
                }.BeatmapInfo);

                PlaylistItem item3 = new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo =
                    {
                        StarRating = 2.5,
                        Metadata =
                        {
                            Artist = "very very very very very very very very very long artist",
                            ArtistUnicode = "very very very very very very very very very long artist",
                            Title = "very very very very very very very very very very very long title",
                            TitleUnicode = "very very very very very very very very very very very long title",
                        }
                    }
                }.BeatmapInfo);

                Child = rooms = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(0.9f),
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        createMultiplayerPanel(new Room
                        {
                            Name = "Multiplayer room",
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Type = MatchType.HeadToHead,
                            Playlist = [item1],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Multiplayer room",
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Type = MatchType.HeadToHead,
                            Playlist = [item1],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Pinned room",
                            Pinned = true,
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Type = MatchType.HeadToHead,
                            Playlist = [item1],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Private room",
                            Password = "*",
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Type = MatchType.HeadToHead,
                            Playlist = [item3],
                            CurrentPlaylistItem = item3
                        }),
                        createPlaylistRoomPanel(new Room
                        {
                            Name = "Playlist room with multiple beatmaps",
                            Status = RoomStatus.Playing,
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Playlist = [item1, item2],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Playlist room with multiple beatmaps",
                            Status = RoomStatus.Playing,
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Playlist = [item1, item2],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Closing soon",
                            EndDate = DateTimeOffset.Now.AddSeconds(5),
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Closed room",
                            EndDate = DateTimeOffset.Now,
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Spotlight room",
                            Category = RoomCategory.Spotlight,
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Featured artist room",
                            Category = RoomCategory.FeaturedArtist,
                        }),
                    }
                };
            });

            AddUntilStep("wait for panel load", () => rooms.Count, () => Is.EqualTo(10));
            AddUntilStep("\"currently playing\" room count correct",
                () => rooms.ChildrenOfType<OsuSpriteText>().Count(s => s.Text.ToString().StartsWith("Currently playing", StringComparison.Ordinal)), () => Is.EqualTo(4));
            AddUntilStep("\"ready to play\" room count correct", () => rooms.ChildrenOfType<OsuSpriteText>().Count(s => s.Text.ToString().StartsWith("Ready to play", StringComparison.Ordinal)),
                () => Is.EqualTo(5));
        }

        [Test]
        public void TestEnableAndDisablePassword()
        {
            RoomPanel panel = null!;
            Room room = null!;

            AddStep("create room", () => Child = panel = createLoungeRoom(room = new Room
            {
                Name = "Room with password",
                Type = MatchType.HeadToHead,
            }));

            AddUntilStep("wait for panel load", () => panel.ChildrenOfType<DrawableRoomParticipantsList>().Any());

            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, panel.ChildrenOfType<RoomPanel.CornerIcon>().First().Alpha));

            AddStep("set password", () => room.Password = "password");
            AddAssert("password icon visible", () => Precision.AlmostEquals(1, panel.ChildrenOfType<RoomPanel.CornerIcon>().First().Alpha));

            AddStep("unset password", () => room.Password = string.Empty);
            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, panel.ChildrenOfType<RoomPanel.CornerIcon>().First().Alpha));
        }

        [Test]
        public void TestMultiplayerRooms()
        {
            AddStep("create rooms", () => Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new[]
                {
                    new MultiplayerRoomPanel(new Room
                    {
                        Name = "A host-only room",
                        QueueMode = QueueMode.HostOnly,
                        Type = MatchType.HeadToHead,
                        RoomID = 1337,
                    }),
                    new MultiplayerRoomPanel(new Room
                    {
                        Name = "An all-players, team-versus room",
                        QueueMode = QueueMode.AllPlayers,
                        Type = MatchType.TeamVersus,
                        RoomID = 1338,
                    }),
                    new MultiplayerRoomPanel(new Room
                    {
                        Name = "A round-robin room",
                        QueueMode = QueueMode.AllPlayersRoundRobin,
                        Type = MatchType.HeadToHead,
                        RoomID = 1339,
                    }),
                }
            });
        }

        [Test]
        public void TestRoomWithLongTitle()
        {
            AddStep("create rooms", () => Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new[]
                {
                    new MultiplayerRoomPanel(new Room
                    {
                        Name =
                            "This room has a very very long title enough to make the external link button reach the participants list on the right side unless the test window is very wide, at which point I don't know, hi.",
                        QueueMode = QueueMode.HostOnly,
                        Type = MatchType.HeadToHead,
                        RoomID = 1337,
                    }),
                }
            });
        }

        [Test]
        public void TestRoomWithUpdatedRoomID()
        {
            Room room = null!;

            AddStep("create rooms", () => Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Y,
                RelativeSizeAxes = Axes.X,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new[]
                {
                    new MultiplayerRoomPanel(room = new Room
                    {
                        Name =
                            "This room has a very very long title enough to make the external link button reach the participants list on the right side unless the test window is very wide, at which point I don't know, hi.",
                        QueueMode = QueueMode.HostOnly,
                        Type = MatchType.HeadToHead,
                    }),
                }
            });
            AddWaitStep("wait", 3);
            AddStep("set room ID", () => room.RoomID = 1337);
            AddWaitStep("wait", 3);
            AddStep("clear room ID", () => room.RoomID = null);
        }

        private RoomPanel createPlaylistRoomPanel(Room room)
        {
            room.Host ??= new APIUser { Username = "peppy", Id = 2 };

            if (room.RecentParticipants.Count == 0)
            {
                room.RecentParticipants = Enumerable.Range(0, 20).Select(i => new APIUser
                {
                    Id = i,
                    Username = $"User {i}"
                }).ToArray();
            }

            return new PlaylistsRoomPanel(room)
            {
                SelectedItem = new Bindable<PlaylistItem?>(room.CurrentPlaylistItem),
            };
        }

        private RoomPanel createMultiplayerPanel(Room room)
        {
            room.Host ??= new APIUser { Username = "peppy", Id = 2 };

            if (room.RecentParticipants.Count == 0)
            {
                room.RecentParticipants = Enumerable.Range(0, 20).Select(i => new APIUser
                {
                    Id = i,
                    Username = $"User {i}"
                }).ToArray();
            }

            return new MultiplayerRoomPanel(room);
        }

        private RoomPanel createLoungeRoom(Room room)
        {
            room.Host ??= new APIUser { Username = "peppy", Id = 2 };

            if (room.RecentParticipants.Count == 0)
            {
                room.RecentParticipants = Enumerable.Range(0, 20).Select(i => new APIUser
                {
                    Id = i,
                    Username = $"User {i}"
                }).ToArray();
            }

            return new LoungeRoomPanel(room)
            {
                MatchingFilter = true,
                SelectedRoom = selectedRoom
            };
        }
    }
}
