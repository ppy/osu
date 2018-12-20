// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
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
            Info info = new Info(new Room());
            Add(info);

            AddStep(@"set name", () => info.Name.Value = @"Room Name?");
            AddStep(@"set availability", () => info.Availability.Value = RoomAvailability.FriendsOnly);
            AddStep(@"set status", () => info.Status.Value = new RoomStatusPlaying());
            AddStep(@"set beatmap", () => info.Beatmap.Value = new BeatmapInfo
            {
                StarDifficulty = 2.4,
                Ruleset = rulesets.GetRuleset(0),
                Metadata = new BeatmapMetadata
                {
                    Title = @"My Song",
                    Artist = @"VisualTests",
                    AuthorString = @"osu!lazer",
                },
            });

            AddStep(@"change name", () => info.Name.Value = @"Room Name!");
            AddStep(@"change availability", () => info.Availability.Value = RoomAvailability.InviteOnly);
            AddStep(@"change status", () => info.Status.Value = new RoomStatusOpen());
            AddStep(@"null beatmap", () => info.Beatmap.Value = null);
            AddStep(@"change beatmap", () => info.Beatmap.Value = new BeatmapInfo
            {
                StarDifficulty = 4.2,
                Ruleset = rulesets.GetRuleset(3),
                Metadata = new BeatmapMetadata
                {
                    Title = @"Your Song",
                    Artist = @"Tester",
                    AuthorString = @"Someone",
                },
            });
        }
    }
}
