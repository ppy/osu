// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Beatmaps
{
    /// <summary>
    /// Processes a post-converted Beatmap.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained in the Beatmap.</typeparam>
    public class BeatmapProcessor<TObject>
        where TObject : HitObject
    {
        /// <summary>
        /// Post-processes a Beatmap to add mode-specific components that aren't added during conversion.
        /// <para>
        /// An example of such a usage is for combo colours.
        /// </para>
        /// </summary>
        /// <param name="beatmap">The Beatmap to process.</param>
        public virtual void PostProcess(Beatmap<TObject> beatmap) { }
    }
}
