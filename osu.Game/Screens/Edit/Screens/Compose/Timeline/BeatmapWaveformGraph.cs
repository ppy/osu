// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Audio;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Edit.Screens.Compose.Timeline
{
    public class BeatmapWaveformGraph : CompositeDrawable
    {
        public WorkingBeatmap Beatmap { set => graph.Waveform = value.Waveform; }

        private readonly WaveformGraph graph;

        public BeatmapWaveformGraph()
        {
            InternalChild = graph = new WaveformGraph { RelativeSizeAxes = Axes.Both };
        }

        /// <summary>
        /// Gets or sets the <see cref="WaveformGraph.Resolution"/>.
        /// </summary>
        public float Resolution
        {
            get { return graph.Resolution; }
            set { graph.Resolution = value; }
        }
    }
}
