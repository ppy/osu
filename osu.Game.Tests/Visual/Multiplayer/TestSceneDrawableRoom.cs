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
    public class TestSceneDrawableRoom : OsuTestScene
    {
        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

        private readonly Bindable<Room> selectedRoom = new Bindable<Room>();

        [Test]
        public void TestMultipleStatuses()
        {
            AddStep("create rooms", () =>
            {
                Child = new FillFlowContainer
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
                            Name = { Value = "Multiplayer room" },
                            Status = { Value = new RoomStatusOpen() },
                            EndDate = { Value = DateTimeOffset.Now.AddDays(1) },
                            Type = { Value = MatchType.HeadToHead },
                            Playlist =
                            {
                                new PlaylistItem
                                {
                                    Beatmap =
                                    {
                                        Value = new TestBeatmap(new OsuRuleset().RulesetInfo)
                                        {
                                            BeatmapInfo =
                                            {
                                                StarRating = 2.5
                                            }
                                        }.BeatmapInfo,
                                    }
                                }
                            }
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = { Value = "Multiplayer room" },
                            Status = { Value = new RoomStatusOpen() },
                            EndDate = { Value = DateTimeOffset.Now.AddDays(1) },
                            Type = { Value = MatchType.HeadToHead },
                            Playlist =
                            {
                                new PlaylistItem
                                {
                                    Beatmap =
                                    {
                                        Value = new TestBeatmap(new OsuRuleset().RulesetInfo)
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
                                        }.BeatmapInfo,
                                    }
                                }
                            }
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = { Value = "Playlist room with multiple beatmaps" },
                            Status = { Value = new RoomStatusPlaying() },
                            EndDate = { Value = DateTimeOffset.Now.AddDays(1) },
                            Playlist =
                            {
                                new PlaylistItem
                                {
                                    Beatmap =
                                    {
                                        Value = new TestBeatmap(new OsuRuleset().RulesetInfo)
                                        {
                                            BeatmapInfo =
                                            {
                                                StarRating = 2.5
                                            }
                                        }.BeatmapInfo,
                                    }
                                },
                                new PlaylistItem
                                {
                                    Beatmap =
                                    {
                                        Value = new TestBeatmap(new OsuRuleset().RulesetInfo)
                                        {
                                            BeatmapInfo =
                                            {
                                                StarRating = 4.5
                                            }
                                        }.BeatmapInfo,
                                    }
                                }
                            }
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = { Value = "Finished room" },
                            Status = { Value = new RoomStatusEnded() },
                            EndDate = { Value = DateTimeOffset.Now },
                        }),
                        createLoungeRoom(new Room
                        {
                            Name = { Value = "Spotlight room" },
                            Status = { Value = new RoomStatusOpen() },
                            Category = { Value = RoomCategory.Spotlight },
                        }),
                    }
                };
            });
        }

        [Test]
        public void TestEnableAndDisablePassword()
        {
            DrawableRoom drawableRoom = null;
            Room room = null;

            AddStep("create room", () => Child = drawableRoom = createLoungeRoom(room = new Room
            {
                Name = { Value = "Room with password" },
                Status = { Value = new RoomStatusOpen() },
                Type = { Value = MatchType.HeadToHead },
            }));

            AddUntilStep("wait for panel load", () => drawableRoom.ChildrenOfType<DrawableRoomParticipantsList>().Any());

            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));

            AddStep("set password", () => room.Password.Value = "password");
            AddAssert("password icon visible", () => Precision.AlmostEquals(1, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));

            AddStep("unset password", () => room.Password.Value = string.Empty);
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
                        Name = { Value = "A host-only room" },
                        QueueMode = { Value = QueueMode.HostOnly },
                        Type = { Value = MatchType.HeadToHead }
                    }),
                    new DrawableMatchRoom(new Room
                    {
                        Name = { Value = "An all-players, team-versus room" },
                        QueueMode = { Value = QueueMode.AllPlayers },
                        Type = { Value = MatchType.TeamVersus }
                    }),
                    new DrawableMatchRoom(new Room
                    {
                        Name = { Value = "A round-robin room" },
                        QueueMode = { Value = QueueMode.AllPlayersRoundRobin },
                        Type = { Value = MatchType.HeadToHead }
                    }),
                }
            });
        }

        private DrawableRoom createLoungeRoom(Room room)
        {
            room.Host.Value ??= new APIUser { Username = "peppy", Id = 2 };

            if (room.RecentParticipants.Count == 0)
            {
                room.RecentParticipants.AddRange(Enumerable.Range(0, 20).Select(i => new APIUser
                {
                    Id = i,
                    Username = $"User {i}"
                }));
            }

            return new DrawableLoungeRoom(room)
            {
                MatchingFilter = true,
                SelectedRoom = { BindTarget = selectedRoom }
            };
        }
    }
}
