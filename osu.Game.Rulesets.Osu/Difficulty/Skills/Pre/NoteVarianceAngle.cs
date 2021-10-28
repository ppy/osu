using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu.Difficulty.Preprocessing;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class NoteVarianceAngle : PrePerNoteStrainSkill
    {
        private const double angle_bonus_begin = Math.PI / 3;
        //private const double timing_threshold = 107; // 140bpm limit
        private const double timing_threshold = 75;
        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.75;
        public NoteVarianceAngle(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {

        }

        protected override double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            OsuDifficultyHitObject osuCurrent = (OsuDifficultyHitObject)current;
            double deltaTimeToBpm = 15000 / current.DeltaTime;
            double angle = osuCurrent.Angle ?? 0;

            double angleBonus = 0.0;

            

            // 둔각보너스
            if(Previous.Count > 0)
            {
                OsuDifficultyHitObject osuPast = (OsuDifficultyHitObject)Previous[0];
                double lastAngle = osuPast.Angle ?? 0;
                

                angleBonus = 0.01 * Math.Max(Math.Sin(angle - angle_bonus_begin), 0);

                // 각도 변화 값
                // 각도가 자주 변화하면 보너스를 받도록 한다
                // bonus for changing angles frequently
                // 
                double angleVariance = Math.Sin(Math.Max(Math.Abs(angle - lastAngle) - Math.PI / 2, 0)) * 0.1;

                angleBonus += angleVariance;


                // 150bpm 이상은 예각일때 보너스 제공
                // bonus for acute bonus at least 150bpm
                if (deltaTimeToBpm >= 150)
                {
                    // 스택된 예각 연타 너프
                    // nerf stacked acute stream

                    // 200bpm까지 유효
                    // limit of 200bpm
                    angleBonus += Math.Sin(Math.Max((Math.PI / 2 - angle), 0))
                        * Math.Min((deltaTimeToBpm - 150), 50) / 50
                        * 0.1
                        ;
                }
            }

            double radius = ((OsuHitObject)osuCurrent.BaseObject).Radius;

            double distance = osuCurrent.JumpDistance / (radius * 2);
            double multiplier = distance >= 1.0 ? 1.0 : Math.Max(distance, 0);
            angleBonus *= multiplier;

            return angleBonus;
        }
        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
