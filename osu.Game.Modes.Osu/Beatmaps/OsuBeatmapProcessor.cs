// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Modes.Osu.Objects;

namespace osu.Game.Modes.Osu.Beatmaps
{
    internal class OsuBeatmapProcessor : IBeatmapProcessor<OsuHitObject>
    {
        public void SetDefaults(OsuHitObject hitObject, Beatmap<OsuHitObject> beatmap)
        {
            hitObject.SetDefaultsFromBeatmap(beatmap);
        }

        public void PostProcess(Beatmap<OsuHitObject> beatmap)
        {
            if (beatmap.ComboColors.Count == 0)
                return;

            int comboIndex = 0;
            int colourIndex = 0;

            foreach (var obj in beatmap.HitObjects)
            {
                if (obj.NewCombo)
                {
                    comboIndex = 0;
                    colourIndex = (colourIndex + 1) % beatmap.ComboColors.Count;
                }

                obj.ComboIndex = comboIndex++;
                obj.ComboColour = beatmap.ComboColors[colourIndex];
            }
        }
    }
}
