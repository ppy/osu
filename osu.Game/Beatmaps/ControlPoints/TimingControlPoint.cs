using osu.Game.Beatmaps.Timing;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint
    {
        public TimeSignatures TimeSignature = TimeSignatures.SimpleQuadruple;
        public double BeatLength = 500;
    }
}