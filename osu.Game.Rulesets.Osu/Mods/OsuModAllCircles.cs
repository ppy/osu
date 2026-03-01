// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAllCircles : ModAllCircles, IApplicableToBeatmap
    {
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap is not OsuBeatmap osuBeatmap)
                return;

            for (int index = 0; index < osuBeatmap.HitObjects.Count; index++)
            {
                if (index > 0 && !osuBeatmap.HitObjects[index].NewCombo) // Make sure objects are at the correct combo number in case previous sliders in the combo were converted
                {
                    osuBeatmap.HitObjects[index].IndexInCurrentCombo = osuBeatmap.HitObjects[index - 1].IndexInCurrentCombo + 1;
                }

                if (osuBeatmap.HitObjects[index] is not Slider slider)
                {
                    continue;
                }

                HitCircle newCircle = convertToCircle(slider.HeadCircle, osuBeatmap);

                newCircle.Samples = slider.NodeSamples.ElementAt(0);

                osuBeatmap.HitObjects[index] = newCircle;

                if (!ConvertEnds.Value)
                {
                    newCircle.LastInCombo = slider.LastInCombo;
                    continue;
                }

                int addedCircles = 0;

                for (int k = 1; k < slider.NestedHitObjects.Count; k++) // We start at 1 to skip over the SliderHeadCircle
                {
                    if (slider.NestedHitObjects.ElementAt(k) is not HitCircle circle)
                        continue;

                    HitCircle repeatCircle = convertToCircle(circle, osuBeatmap, newCircle);
                    addedCircles += 1;

                    repeatCircle.Samples = slider.NodeSamples.ElementAt(addedCircles);
                    repeatCircle.IndexInCurrentCombo = osuBeatmap.HitObjects[index + addedCircles - 1].IndexInCurrentCombo + 1;

                    osuBeatmap.HitObjects.Insert(index + addedCircles, repeatCircle);
                }

                osuBeatmap.HitObjects[index + addedCircles].LastInCombo = slider.LastInCombo;
            }
        }

        private HitCircle convertToCircle(HitCircle objectToConvert, OsuBeatmap osuBeatmap)
        {
            return convertToCircle(objectToConvert, osuBeatmap, objectToConvert);
        }

        private HitCircle convertToCircle(HitCircle objectToConvert, OsuBeatmap osuBeatmap, HitCircle overrideCircle)
        {
            HitCircle newCircle = new HitCircle
            {
                ComboIndex = objectToConvert.ComboIndex,
                IndexInCurrentCombo = objectToConvert.IndexInCurrentCombo,
                NewCombo = objectToConvert.NewCombo,
                Position = objectToConvert.Position,
                Scale = objectToConvert.Scale,
                StackHeight = objectToConvert.StackHeight,
                StartTime = objectToConvert.StartTime,
                TimeFadeIn = overrideCircle.TimeFadeIn,
                TimePreempt = overrideCircle.TimePreempt,
                HitWindows = new OsuHitWindows()
            };
            newCircle.HitWindows.SetDifficulty(osuBeatmap.Difficulty.OverallDifficulty);

            return newCircle;
        }
    }
}
