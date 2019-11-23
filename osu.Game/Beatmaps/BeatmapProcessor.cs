// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Beatmaps
{
    /// <summary>
    /// Provides functionality to alter a <see cref="IBeatmap"/> after it has been converted.
    /// </summary>
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
                    obj.ComboIndex = (lastObj?.ComboIndex ?? 0) + obj.ComboOffset + 1;

                    if (lastObj != null)
                        lastObj.LastInCombo = true;
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
            void updateNestedCombo(HitObject obj, int comboIndex, int indexInCurrentCombo)
            {
                if (obj is IHasComboInformation objectComboInfo)
                {
                    objectComboInfo.ComboIndex = comboIndex;
                    objectComboInfo.IndexInCurrentCombo = indexInCurrentCombo;
                    foreach (var nestedObject in obj.NestedHitObjects)
                        updateNestedCombo(nestedObject, comboIndex, indexInCurrentCombo);
                }
            }

            foreach (var hitObject in Beatmap.HitObjects)
            {
                if (hitObject is IHasComboInformation objectComboInfo)
                {
                    foreach (var nested in hitObject.NestedHitObjects)
                        updateNestedCombo(nested, objectComboInfo.ComboIndex, objectComboInfo.IndexInCurrentCombo);
                }
            }
        }
    }
}
