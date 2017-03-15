// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        /// <param name="beatmap">The Beatmap to extract the default values from.</param>
        void SetDefaults(T hitObject, Beatmap<T> beatmap);

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
