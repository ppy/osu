namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that spans some length.
    /// </summary>
    public interface IHasRepeats
    {
        /// <summary>
        /// The amount of times the HitObject repeats.
        /// </summary>
        int RepeatCount { get; }
    }
}
