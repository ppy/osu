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

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public class TestSceneMatchInfo : MultiplayerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Info),
            typeof(HeaderButton),
            typeof(ReadyButton),
            typeof(MatchBeatmapPanel)
        };

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Add(new Info());

            AddStep(@"set name", () => Room.Name.Value = @"Room Name?");
            AddStep(@"set availability", () => Room.Availability.Value = RoomAvailability.FriendsOnly);
            AddStep(@"set status", () => Room.Status.Value = new RoomStatusPlaying());
            AddStep(@"set beatmap", () =>
            {
                Room.Playlist.Clear();
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap =
                    {
                        Value = new BeatmapInfo
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
                    }
                });
            });

            AddStep(@"change name", () => Room.Name.Value = @"Room Name!");
            AddStep(@"change availability", () => Room.Availability.Value = RoomAvailability.InviteOnly);
            AddStep(@"change status", () => Room.Status.Value = new RoomStatusOpen());
            AddStep(@"null beatmap", () => Room.Playlist.Clear());
            AddStep(@"change beatmap", () =>
            {
                Room.Playlist.Clear();
                Room.Playlist.Add(new PlaylistItem
                {
                    Beatmap =
                    {
                        Value = new BeatmapInfo
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
                    }
                });
            });
        }
    }
}
