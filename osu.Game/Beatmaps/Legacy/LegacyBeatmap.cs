// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Legacy
{
    /// <summary>
    /// A type of Beatmap loaded from a legacy .osu beatmap file (version &lt;=15).
    /// </summary>
    public class LegacyBeatmap : Beatmap
    {
        /// <summary>
        /// Constructs a new beatmap.
        /// </summary>
        /// <param name="original">The original beatmap to use the parameters of.</param>
        internal LegacyBeatmap(Beatmap original = null)
            : base(original)
        {
            HitObjects = original?.HitObjects;
        }
    }
}
