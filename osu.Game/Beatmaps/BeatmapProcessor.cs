// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        }

        public virtual void PostProcess()
        {
            IHasComboInformation? lastObj = null;

            foreach (var obj in Beatmap.HitObjects.OfType<IHasComboInformation>())
            {
                obj.UpdateComboInformation(lastObj);
                lastObj = obj;
            }
        }
    }
}
