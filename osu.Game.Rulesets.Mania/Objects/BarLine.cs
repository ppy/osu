using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Mania.Objects
{
    public class BarLine : ManiaHitObject
    {
        /// <summary>
        /// The control point which this bar line is part of.
        /// </summary>
        public TimingControlPoint ControlPoint;

        /// <summary>
        /// The index of the beat which this bar line represents within the control point.
        /// This is a "major" bar line if <see cref="BeatIndex"/> % <see cref="TimingControlPoint.TimeSignature"/> == 0.
        /// </summary>
        public int BeatIndex;
    }
}