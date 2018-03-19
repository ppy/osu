using osu.Game.Beatmaps;
using osu.Game.Rulesets.Shape.Objects;

namespace osu.Game.Rulesets.Shape.Beatmaps
{
    internal class ShapeBeatmapProcessor : BeatmapProcessor<ShapeHitObject>
    {
        public override void PostProcess(Beatmap<ShapeHitObject> beatmap)
        {
            if (beatmap.ComboColours.Count == 0)
                return;

            int comboIndex = 0;
            int colourIndex = 0;

            foreach (var obj in beatmap.HitObjects)
            {
                if (obj.NewCombo)
                {
                    comboIndex = 0;
                    colourIndex = (colourIndex + 1) % beatmap.ComboColours.Count;
                }

                obj.ComboIndex = comboIndex++;
                obj.ComboColour = beatmap.ComboColours[colourIndex];
            }
        }
    }
}
