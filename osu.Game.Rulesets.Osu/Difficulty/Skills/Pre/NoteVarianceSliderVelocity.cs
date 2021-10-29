using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills
{
    public class NoteVarianceSliderVelocity : PrePerNoteStrainSkill
    {
        public NoteVarianceSliderVelocity(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.5;

        private double lastVelocity = -1;

        protected override double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            var osuCurrent = (OsuDifficultyHitObject)current;
            double result = 0.0;
            //beatmap.BeatmapInfo.

            //Console.WriteLine("sliderCount: " + beatmap.GetStatistics().ToString());

            //((OsuHitObject)current.BaseObject)

            // 슬라이더 속도가 급변할 시 보너스를 준다.
            // The Bonus is given when the slider speed changes rapidly.
            if (osuCurrent.LastObject is Slider OsuSlider)
            {
                double adaptedVelocity = Math.Max(OsuSlider.Velocity - 0.5, 0);

                if (lastVelocity >= 0)
                {
                    result = Math.Abs(adaptedVelocity - lastVelocity) * 2;
                }
                lastVelocity = adaptedVelocity;

                // default bonus for velocity
                result += adaptedVelocity / 2;

                //if(result >= 0.5)
                //{
                //    result = 0.5;
                //}
            }

            return result;
        }


    }
}
