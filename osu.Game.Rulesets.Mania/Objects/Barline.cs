using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class Barline : ManiaHitObject
    {
        /// <summary>
        /// The control point which this bar line is part of.
        /// </summary>
        public TimingControlPoint ControlPoint;

        /// <summary>
        /// The index of the beat which this bar line represents within the control point.
        /// This is a "major" beat at <see cref="BeatIndex"/> % <see cref="TimingControlPoint.TimeSignature"/> == 0.
        /// </summary>
        public int BeatIndex;
    }
}