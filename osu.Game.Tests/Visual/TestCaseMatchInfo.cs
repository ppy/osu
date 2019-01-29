// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Match.Components;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMatchInfo : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Info),
            typeof(HeaderButton),
            typeof(ReadyButton),
            typeof(ViewBeatmapButton)
        };

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            var room = new Room();

            Info info = new Info(room);
            Add(info);

            AddStep(@"set name", () => room.Name.Value = @"Room Name?");
            AddStep(@"set availability", () => room.Availability.Value = RoomAvailability.FriendsOnly);
            AddStep(@"set status", () => room.Status.Value = new RoomStatusPlaying());
            AddStep(@"set beatmap", () =>
            {
                room.Playlist.Clear();
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = new BeatmapInfo
                    {
                        StarDifficulty = 2.4,
                        Ruleset = rulesets.GetRuleset(0),
                        Metadata = new BeatmapMetadata
                        {
                            Title = @"My Song",
                            Artist = @"VisualTests",
                            AuthorString = @"osu!lazer",
                        },
                    }
                });
            });

            AddStep(@"change name", () => room.Name.Value = @"Room Name!");
            AddStep(@"change availability", () => room.Availability.Value = RoomAvailability.InviteOnly);
            AddStep(@"change status", () => room.Status.Value = new RoomStatusOpen());
            AddStep(@"null beatmap", () => room.Playlist.Clear());
            AddStep(@"change beatmap", () =>
            {
                room.Playlist.Clear();
                room.Playlist.Add(new PlaylistItem
                {
                    Beatmap = new BeatmapInfo
                    {
                        StarDifficulty = 4.2,
                        Ruleset = rulesets.GetRuleset(3),
                        Metadata = new BeatmapMetadata
                        {
                            Title = @"Your Song",
                            Artist = @"Tester",
                            AuthorString = @"Someone",
                        },
                    }
                });
            });
        }
    }
}
