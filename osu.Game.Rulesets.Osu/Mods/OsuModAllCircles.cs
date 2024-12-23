// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Scoring;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModAllCircles : Mod, IApplicableToBeatmap
    {
        public override string Name => "All Circles";

        public override string Acronym => "CC";

        public override ModType Type => ModType.Conversion;

        public override LocalisableString Description => "Oops! All Circles! Sliders get changed into circles.";

        public override double ScoreMultiplier => 1;

        public void ApplyToBeatmap(IBeatmap beatmap)
        {
            if (beatmap is not OsuBeatmap osuBeatmap)
                return;

            for (int i = 0; i < osuBeatmap.HitObjects.Count; i++)
            {
                if (osuBeatmap.HitObjects[i] is not Slider slider)
                    continue;

                HitCircle newCircle = new HitCircle
                {
                    ComboIndex = slider.ComboIndex,
                    IndexInCurrentCombo = slider.IndexInCurrentCombo,
                    LastInCombo = slider.LastInCombo,
                    NewCombo = slider.NewCombo,
                    Position = slider.Position,
                    Samples = slider.Samples,
                    Scale = slider.Scale,
                    StackHeight = slider.StackHeight,
                    StartTime = slider.StartTime,
                    TimeFadeIn = slider.TimeFadeIn,
                    TimePreempt = slider.TimePreempt,
                    HitWindows = new OsuHitWindows()
                };
                newCircle.HitWindows.SetDifficulty(osuBeatmap.Difficulty.OverallDifficulty);

                osuBeatmap.HitObjects[i] = newCircle;
            }
        }
    }
}
