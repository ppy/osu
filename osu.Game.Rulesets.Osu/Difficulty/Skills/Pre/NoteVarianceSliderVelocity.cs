using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

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
            var result = 0.0;

            // 슬라이더 속도가 급변할 시 보너스를 준다.
            // The Bonus is given when the slider speed changes rapidly.
            if (osuCurrent.LastObject is Slider OsuSlider)
            {
                if (lastVelocity >= 0)
                {
                    result = Math.Max(OsuSlider.Velocity - lastVelocity, 0) * 1.2;
                }
                lastVelocity = OsuSlider.Velocity;

                // default bonus for velocity
                result += OsuSlider.Velocity / 5;

                //if(result >= 0.5)
                //{
                //    result = 0.5;
                //}
            }

            return result;
        }
    }
}
