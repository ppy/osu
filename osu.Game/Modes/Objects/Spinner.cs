using osu.Game.Modes.Objects.Types;

namespace osu.Game.Modes.Objects
{
    internal class Spinner : HitObject, IHasEndTime
    {
        public double EndTime { get; set; }

        public double Duration => EndTime - StartTime;
    }
}
