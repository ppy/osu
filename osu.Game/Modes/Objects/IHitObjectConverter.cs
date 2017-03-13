namespace osu.Game.Modes.Objects
{
    /// <summary>
    /// Converts HitObjects to another mode.
    /// </summary>
    /// <typeparam name="T">The type of HitObject to be converted to.</typeparam>
    public interface IHitObjectConverter<T>
        where T : HitObject
    {
        /// <summary>
        /// Converts a <see cref="HitObject"/> to another mode.
        /// </summary>
        /// <param name="hitObject">The base HitObject to convert.</param>
        /// <returns>The converted HitObject.</returns>
        T Convert(HitObject hitObject);
    }
}
