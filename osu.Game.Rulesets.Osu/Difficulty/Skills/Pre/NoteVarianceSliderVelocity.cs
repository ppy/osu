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

            if (osuCurrent.BaseObject is Spinner || osuCurrent.LastObject is Spinner)
                return 0;

            // The Bonus is given when the slider speed changes rapidly.
            if (osuCurrent.LastObject is Slider OsuSlider)
            {
                double adaptedVelocity = Math.Max(OsuSlider.Velocity - 0.4, 0);

                if (lastVelocity >= 0)
                    result = Math.Max(adaptedVelocity - lastVelocity, (lastVelocity - adaptedVelocity) / 2) * 2.5;

                // default bonus for velocity
                result += adaptedVelocity / 3;

                lastVelocity = adaptedVelocity;
            }
            return result;
        }
    }
}
