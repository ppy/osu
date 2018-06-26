// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMatchInfo : OsuTestCase
    {
        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets)
        {
            Info info = new Info();
            Add(info);

            AddStep(@"set name", () => info.Name = @"Room Name?");
            AddStep(@"set availability", () => info.Availability = RoomAvailability.FriendsOnly);
            AddStep(@"set status", () => info.Status = new RoomStatusPlaying());
            AddStep(@"set beatmap", () => info.Beatmap = new BeatmapInfo
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

            AddStep(@"set type", () => info.Type = new GameTypeTagTeam());

            AddStep(@"change name", () => info.Name = @"Room Name!");
            AddStep(@"change availability", () => info.Availability = RoomAvailability.InviteOnly);
            AddStep(@"change status", () => info.Status = new RoomStatusOpen());
            AddStep(@"null beatmap", () => info.Beatmap = null);
            AddStep(@"change type", () => info.Type = new GameTypeTeamVersus());
            AddStep(@"change beatmap", () => info.Beatmap = new BeatmapInfo
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
