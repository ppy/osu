namespace osu.Game.Modes.Objects.Types
{
    /// <summary>
    /// A HitObject that has a distance.
    /// </summary>
    public interface IHasDistance
    {
        /// <summary>
        /// The distance of the HitObject.
        /// </summary>
        double Distance { get; }
    }
}
