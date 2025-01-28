// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAllCircles : ModAllCircles, IApplicableToBeatmap
    {
        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap is not OsuBeatmap osuBeatmap)
                return;

            for (int i = 0; i < osuBeatmap.HitObjects.Count; i++)
            {
                if (osuBeatmap.HitObjects[i] is not Slider slider)
                    continue;

                HitCircle newCircle = convertToCircle(slider.HeadCircle, osuBeatmap);

                osuBeatmap.HitObjects[i] = newCircle;

                if (!ConvertEnds.Value)
                    continue;

                Vector2 otherEnd = slider.RepeatCount > 0 ? slider.NestedHitObjects.OfType<SliderRepeat>().First().Position : slider.EndPosition;
                for (int k = 0; k <= slider.RepeatCount; k++)
                {
                    HitCircle repeatCircle;
                    if (k % 2 == 0)
                    { // If this should be at the other end of the slider
                        repeatCircle = convertToCircle(slider.HeadCircle, osuBeatmap);

                        repeatCircle.Position = otherEnd;
                        repeatCircle.StartTime = slider.StartTime;
                    }
                    else
                    { // If this should overlap with the beginning of the slider
                        repeatCircle = convertToCircle(slider.HeadCircle, osuBeatmap);
                    }

                    repeatCircle.IndexInCurrentCombo += k + 1;
                    osuBeatmap.HitObjects.Insert(i + k + 1, repeatCircle);
                }
            }
        }
        private HitCircle convertToCircle(HitCircle objectToConvert, OsuBeatmap osuBeatmap)
        {
            HitCircle newCircle = new HitCircle
            {
                ComboIndex = objectToConvert.ComboIndex,
                IndexInCurrentCombo = objectToConvert.IndexInCurrentCombo,
                LastInCombo = objectToConvert.LastInCombo,
                NewCombo = objectToConvert.NewCombo,
                Position = objectToConvert.Position,
                Samples = objectToConvert.Samples,
                Scale = objectToConvert.Scale,
                StackHeight = objectToConvert.StackHeight,
                StartTime = objectToConvert.StartTime,
                TimeFadeIn = objectToConvert.TimeFadeIn,
                TimePreempt = objectToConvert.TimePreempt,
                HitWindows = new OsuHitWindows()
            };
            newCircle.HitWindows.SetDifficulty(osuBeatmap.Difficulty.OverallDifficulty);

            return newCircle;
        }
    }
}
