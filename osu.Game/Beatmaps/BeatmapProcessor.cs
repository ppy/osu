// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Beatmaps
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
        public virtual void PostProcess(Beatmap<TObject> beatmap)
        {
            IHasComboInformation lastObj = null;

            foreach (var obj in beatmap.HitObjects.OfType<IHasComboInformation>())
            {
                if (obj.NewCombo)
                {
                    obj.IndexInCurrentCombo = 0;
                    if (lastObj != null)
                    {
                        lastObj.LastInCombo = true;
                        obj.ComboIndex = lastObj.ComboIndex + 1;
                    }
                }
                else if (lastObj != null)
                {
                    obj.IndexInCurrentCombo = lastObj.IndexInCurrentCombo + 1;
                    obj.ComboIndex = lastObj.ComboIndex;
                }

                lastObj = obj;
            }
        }
    }
}
