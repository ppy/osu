// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
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
            void UpdateNestedCombo(Rulesets.Objects.HitObject obj, int comboIndex, int indexInCurrentCombo)
            {
                if (obj is IHasComboInformation)
                {
                    var objectComboInfo = (IHasComboInformation)obj;
                    objectComboInfo.ComboIndex = comboIndex;
                    objectComboInfo.IndexInCurrentCombo = indexInCurrentCombo;
                    foreach (var nestedObjet in obj.NestedHitObjects)
                        UpdateNestedCombo(nestedObjet, comboIndex, indexInCurrentCombo);
                }
            }
            foreach (var hitObject in Beatmap.HitObjects)
                if (hitObject is IHasComboInformation)
                {
                    var objectComboInfo = (IHasComboInformation)hitObject;
                    foreach (var nested in hitObject.NestedHitObjects)
                        UpdateNestedCombo(nested, objectComboInfo.ComboIndex, objectComboInfo.IndexInCurrentCombo);
                }
        }
    }
}
