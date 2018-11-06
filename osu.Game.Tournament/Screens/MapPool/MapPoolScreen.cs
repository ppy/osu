// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Screens;
using osu.Game.Tournament.Components;
using osu.Game.Tournament.Screens.Ladder.Components;
using OpenTK;

namespace osu.Game.Tournament.Screens.MapPool
{
    public class MapPoolScreen : OsuScreen
    {
        public MapPoolScreen(TournamentGrouping round)
        {
            FillFlowContainer maps;

            InternalChildren = new Drawable[]
            {
                maps = new FillFlowContainer
                {
                    Spacing = new Vector2(20),
                    Padding = new MarginPadding(50),
                    Direction = FillDirection.Full,
                    RelativeSizeAxes = Axes.Both,
                }
            };

            foreach (var b in round.Beatmaps)
                maps.Add(new TournamentBeatmapPanel(b.BeatmapInfo)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                });
        }
    }
}
