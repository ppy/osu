// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Catch.Objects;

namespace osu.Game.Rulesets.Catch.Beatmaps
{
    internal class CatchBeatmapProcessor : BeatmapProcessor<CatchHitObject>
    {
        public override void PostProcess(Beatmap<CatchHitObject> beatmap)
        {
            if (beatmap.ComboColors.Count == 0)
                return;

            int comboIndex = 0;
            int colourIndex = 0;

            CatchHitObject lastObj = null;

            foreach (var obj in beatmap.HitObjects)
            {
                if (obj.NewCombo)
                {
                    if (lastObj != null) lastObj.LastInCombo = true;

                    comboIndex = 0;
                    colourIndex = (colourIndex + 1) % beatmap.ComboColors.Count;
                }

                obj.ComboIndex = comboIndex++;
                obj.ComboColour = beatmap.ComboColors[colourIndex];

                lastObj = obj;
            }
        }
    }
}
