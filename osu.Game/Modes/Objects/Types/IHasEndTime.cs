namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that ends at a different time than its start time.
    /// </summary>
    public interface IHasEndTime
    {
        /// <summary>
        /// The time at which the HitObject ends.
        /// </summary>
        double EndTime { get; }

        /// <summary>
        /// The duration of the HitObject.
        /// </summary>
        double Duration { get; }
    }
}
