// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Screens.Multi.Screens.Match;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseMatchHeader : OsuTestCase
    {
        public TestCaseMatchHeader()
        {
            Header header = new Header();
            Add(header);

            AddStep(@"set beatmap set", () => header.BeatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = new BeatmapSetOnlineCovers
                    {
                        Cover = @"https://assets.ppy.sh/beatmaps/760757/covers/cover.jpg?1526944540",
                    },
                },
            });

            AddStep(@"change beatmap set", () => header.BeatmapSet = new BeatmapSetInfo
            {
                OnlineInfo = new BeatmapSetOnlineInfo
                {
                    Covers = new BeatmapSetOnlineCovers
                    {
                        Cover = @"https://assets.ppy.sh/beatmaps/761883/covers/cover.jpg?1525557400",
                    },
                },
            });

            AddStep(@"null beatmap set", () => header.BeatmapSet = null);
        }
    }
}
