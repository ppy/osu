// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Beatmaps.Formats
{
    /// <summary>
    /// A <see cref="LegacyBeatmapDecoder"/> built for difficulty calculation of legacy <see cref="Beatmap"/>s
    /// <remarks>
    /// To use this, the decoder must be registered by the application through <see cref="LegacyDifficultyCalculatorBeatmapDecoder.Register"/>.
    /// Doing so will override any existing <see cref="Beatmap"/> decoders.
    /// </remarks>
    /// </summary>
    public class LegacyDifficultyCalculatorBeatmapDecoder : LegacyBeatmapDecoder
    {
        public LegacyDifficultyCalculatorBeatmapDecoder(int version = LATEST_VERSION)
            : base(version)
        {
            ApplyOffsets = false;
        }

        public new static void Register()
        {
            AddDecoder<Beatmap>(@"osu file format v", m => new LegacyDifficultyCalculatorBeatmapDecoder(int.Parse(m.Split('v').Last())));
        }

        protected override TimingControlPoint CreateTimingControlPoint()
            => new LegacyDifficultyCalculatorControlPoint();

        private class LegacyDifficultyCalculatorControlPoint : TimingControlPoint
        {
            public override double BeatLength { get; set; } = 1000;
        }
    }
}
