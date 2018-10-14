// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;

namespace osu.Game.Tournament.Tests
{
    public class TestCaseMapPool : LadderTestCase
    {
        public TestCaseMapPool()
        {
            var round = Ladder.Groupings.First(g => g.Name == "Finals");

            Add(new MapPoolScreen(round));
        }
    }

    public class MapPoolScreen : CompositeDrawable
    {
        private readonly FillFlowContainer maps;

        public MapPoolScreen(TournamentGrouping round)
        {
            InternalChildren = new Drawable[]
            {
                maps = new FillFlowContainer
                {
                    Direction = FillDirection.Full,
                    RelativeSizeAxes = Axes.Both,
                },
            };

            //foreach (var b in round.Beatmaps)
            //    maps.Add(new TournamentBeatmapPanel(new BeatmapInfo() { OnlineBeatmapID = b.ID }));
        }
    }
}
