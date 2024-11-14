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
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Tests.Beatmaps;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneDrawableRoom : OsuTestScene
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
                PlaylistItem item1 = new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo)
                {
                    BeatmapInfo = { StarRating = 2.5 }
                }.BeatmapInfo);

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
                        createLoungeRoom(new Room
                        {
                            Name = "Multiplayer room",
                            Status = new RoomStatusOpen(),
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Type = MatchType.HeadToHead,
                            Playlist = [item1],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Private room",
                            Status = new RoomStatusOpenPrivate(),
                            Password = "*",
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Type = MatchType.HeadToHead,
                            Playlist = [item3],
                            CurrentPlaylistItem = item3
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Playlist room with multiple beatmaps",
                            Status = new RoomStatusPlaying(),
                            EndDate = DateTimeOffset.Now.AddDays(1),
                            Playlist = [item1, item2],
                            CurrentPlaylistItem = item1
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Finished room",
                            Status = new RoomStatusEnded(),
                            EndDate = DateTimeOffset.Now,
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Spotlight room",
                            Status = new RoomStatusOpen(),
                            Category = RoomCategory.Spotlight,
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = "Featured artist room",
                            Status = new RoomStatusOpen(),
                            Category = RoomCategory.FeaturedArtist,
                        }),
                    }
                };
            });

            AddUntilStep("wait for panel load", () => rooms.Count == 6);
            AddUntilStep("correct status text", () => rooms.ChildrenOfType<OsuSpriteText>().Count(s => s.Text.ToString().StartsWith("Currently playing", StringComparison.Ordinal)) == 2);
            AddUntilStep("correct status text", () => rooms.ChildrenOfType<OsuSpriteText>().Count(s => s.Text.ToString().StartsWith("Ready to play", StringComparison.Ordinal)) == 4);
        }

        [Test]
        public void TestEnableAndDisablePassword()
        {
            DrawableRoom drawableRoom = null!;
            Room room = null!;

            AddStep("create room", () => Child = drawableRoom = createLoungeRoom(room = new Room
            {
                Name = "Room with password",
                Status = new RoomStatusOpen(),
                Type = MatchType.HeadToHead,
            }));

            AddUntilStep("wait for panel load", () => drawableRoom.ChildrenOfType<DrawableRoomParticipantsList>().Any());

            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));

            AddStep("set password", () => room.Password = "password");
            AddAssert("password icon visible", () => Precision.AlmostEquals(1, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));

            AddStep("unset password", () => room.Password = string.Empty);
            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));
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
                    new DrawableMatchRoom(new Room
                    {
                        Name = "A host-only room",
                        QueueMode = QueueMode.HostOnly,
                        Type = MatchType.HeadToHead,
                    })
                    {
                        SelectedItem = new Bindable<PlaylistItem?>()
                    },
                    new DrawableMatchRoom(new Room
                    {
                        Name = "An all-players, team-versus room",
                        QueueMode = QueueMode.AllPlayers,
                        Type = MatchType.TeamVersus
                    })
                    {
                        SelectedItem = new Bindable<PlaylistItem?>()
                    },
                    new DrawableMatchRoom(new Room
                    {
                        Name = "A round-robin room",
                        QueueMode = QueueMode.AllPlayersRoundRobin,
                        Type = MatchType.HeadToHead
                    })
                    {
                        SelectedItem = new Bindable<PlaylistItem?>()
                    },
                }
            });
        }

        private DrawableRoom createLoungeRoom(Room room)
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

            return new DrawableLoungeRoom(room)
            {
                MatchingFilter = true,
                SelectedRoom = selectedRoom
            };
        }
    }
}
