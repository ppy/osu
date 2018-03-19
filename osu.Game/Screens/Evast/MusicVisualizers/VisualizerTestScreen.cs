// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Screens.Evast.MusicVisualizers
{
    public class VisualizerTestScreen : BeatmapScreen
    {
        public VisualizerTestScreen()
        {
            Children = new Drawable[]
            {
                new LinearVisualizer()
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new LinearVisualizer()
                {
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                },
                new CircularVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    BarsAmount = 30,
                    DegreeValue = 180,
                    BarWidth = 5,
                    CircleSize = 150,
                    X = -400,
                },
                new CircularVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 180,
                    CircleSize = 150,
                    BarsAmount = 30,
                    BarWidth = 5,
                    X = -400,
                    Rotation = 180,
                    IsReversed = true,
                },
                new CircularVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 180,
                    BarsAmount = 50,
                    BarWidth = 2,
                },
                new CircularVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    DegreeValue = 180,
                    BarsAmount = 50,
                    BarWidth = 2,
                    Rotation = 180,
                },
                new CircularVisualizer()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    ValueMultiplier = 200,
                    BarsAmount = 200,
                    CircleSize = 250,
                    BarWidth = 1,
                    X = 400,
                }
            };
        }
    }
}
