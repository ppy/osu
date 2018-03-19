using osu.Game.Beatmaps;
using osu.Game.Rulesets.Vitaru.Objects;

namespace osu.Game.Rulesets.Vitaru.Beatmaps
{
    internal class VitaruBeatmapProcessor : BeatmapProcessor<VitaruHitObject>
    {
        public override void PostProcess(Beatmap<VitaruHitObject> beatmap)
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
