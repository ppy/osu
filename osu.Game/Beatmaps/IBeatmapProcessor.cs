using osu.Game.Modes.Objects;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Processes a post-converted Beatmap.
    /// </summary>
    /// <typeparam name="T">The type of HitObject contained in the Beatmap.</typeparam>
    public interface IBeatmapProcessor<T>
        where T : HitObject
    {
        /// <summary>
        /// Sets default values for a HitObject.
        /// </summary>
        /// <param name="hitObject">The HitObject to set default values for.</param>
        void SetDefaults(T hitObject);

        /// <summary>
        /// Post-processes a Beatmap to add mode-specific components that aren't added during conversion.
        /// <para>
        /// An example of such a usage is for combo colours.
        /// </para>
        /// </summary>
        /// <param name="beatmap">The Beatmap to process.</param>
        void PostProcess(Beatmap<T> beatmap);
    }
}
