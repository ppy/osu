// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Beatmaps
{
    public interface IBeatmapProcessor
    {
        IBeatmap Beatmap { get; }

        /// <summary>
        /// Processes the converted <see cref="Beatmap"/> prior to <see cref="HitObject.ApplyDefaults"/> being invoked.
        /// <para>
        /// Nested <see cref="HitObject"/>s generated during <see cref="HitObject.ApplyDefaults"/> will not be present by this point,
        /// and no mods will have been applied to the <see cref="HitObject"/>s.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This can only be used to add alterations to <see cref="HitObject"/>s generated directly through the conversion process.
        /// </remarks>
        void PreProcess();

        /// <summary>
        /// Processes the converted <see cref="Beatmap"/> after <see cref="HitObject.ApplyDefaults"/> has been invoked.
        /// <para>
        /// Nested <see cref="HitObject"/>s generated during <see cref="HitObject.ApplyDefaults"/> will be present by this point,
        /// and mods will have been applied to all <see cref="HitObject"/>s.
        /// </para>
        /// </summary>
        /// <remarks>
        /// This should be used to add alterations to <see cref="HitObject"/>s while they are in their most playable state.
        /// </remarks>
        void PostProcess();
    }

    /// <summary>
    /// Processes a post-converted Beatmap.
    /// </summary>
    /// <typeparam name="TObject">The type of HitObject contained in the Beatmap.</typeparam>
    public class BeatmapProcessor : IBeatmapProcessor
    {
        public IBeatmap Beatmap { get; }

        public BeatmapProcessor(IBeatmap beatmap)
        {
            Beatmap = beatmap;
        }

        public virtual void PreProcess()
        {
            IHasComboInformation lastObj = null;

            foreach (var obj in Beatmap.HitObjects.OfType<IHasComboInformation>())
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

        public virtual void PostProcess()
        {
        }
    }
}
