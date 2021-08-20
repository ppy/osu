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
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Overlays;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoom : OsuTestScene
    {
        [Cached]
        private readonly Bindable<Room> selectedRoom = new Bindable<Room>();

        [Cached]
        protected readonly OverlayColourProvider ColourProvider = new OverlayColourProvider(OverlayColourScheme.Plum);

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
                        createDrawableRoom(new Room
                        {
                            Name = { Value = "Flyte's Trash Playlist" },
                            Status = { Value = new RoomStatusOpen() },
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
                                                StarDifficulty = 2.5
                                            }
                                        }.BeatmapInfo,
                                    }
                                }
                            }
                        }),
                        createDrawableRoom(new Room
                        {
                            Name = { Value = "Room 2" },
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
                                                StarDifficulty = 2.5
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
                                                StarDifficulty = 4.5
                                            }
                                        }.BeatmapInfo,
                                    }
                                }
                            }
                        }),
                        createDrawableRoom(new Room
                        {
                            Name = { Value = "Room 3" },
                            Status = { Value = new RoomStatusEnded() },
                            EndDate = { Value = DateTimeOffset.Now },
                        }),
                        createDrawableRoom(new Room
                        {
                            Name = { Value = "Room 4 (spotlight)" },
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

            AddStep("create room", () => Child = drawableRoom = createDrawableRoom(room = new Room
            {
                Name = { Value = "Room with password" },
                Status = { Value = new RoomStatusOpen() },
                Type = { Value = MatchType.HeadToHead },
            }));

            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));

            AddStep("set password", () => room.Password.Value = "password");
            AddAssert("password icon visible", () => Precision.AlmostEquals(1, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));

            AddStep("unset password", () => room.Password.Value = string.Empty);
            AddAssert("password icon hidden", () => Precision.AlmostEquals(0, drawableRoom.ChildrenOfType<DrawableRoom.PasswordProtectedIcon>().Single().Alpha));
        }

        private DrawableRoom createDrawableRoom(Room room)
        {
            room.Host.Value ??= new User { Username = "peppy", Id = 2 };

            if (room.RecentParticipants.Count == 0)
            {
                room.RecentParticipants.AddRange(Enumerable.Range(0, 20).Select(i => new User
                {
                    Id = i,
                    Username = $"User {i}"
                }));
            }

            return new DrawableLoungeRoom(room) { MatchingFilter = true };
        }
    }
}
