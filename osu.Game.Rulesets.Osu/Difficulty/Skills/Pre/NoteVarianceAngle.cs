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

        private const double min_doubletap_nerf = 0.0; // minimum value (eventually on stacked)
        private const double max_doubletap_nerf = 1.0; // maximum value 
        private const double threshold_doubletap_contributing = 2.0; // minimum distance not influenced (2.0 means it is not stacked at least)

        public NoteVarianceAngle(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate) { }

        protected override double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            OsuDifficultyHitObject osuCurrent = (OsuDifficultyHitObject)current;

            if (osuCurrent.BaseObject is Spinner || osuCurrent.LastObject is Spinner)
                return 0;

            double deltaTimeToBpm = 15000 / current.DeltaTime;
            double angle = osuCurrent.Angle ?? 0;

            double angleBonus = 0.0;

            // 둔각보너스
            if(Previous.Count > 0)
            {
                OsuDifficultyHitObject osuPast = (OsuDifficultyHitObject)Previous[0];
                double lastAngle = osuPast.Angle ?? 0;
                
                angleBonus = 0.01 * Math.Max(Math.Sin(angle - angle_bonus_begin), 0);

                // bonus for changing angles frequently
                double angleVariance = Math.Sin(Math.Max(Math.Abs(angle - lastAngle) - Math.PI / 2, 0)) * 0.10;

                angleBonus += angleVariance;

                // bonus for acute bonus at least 150bpm
                if (deltaTimeToBpm >= 160)
                {
                    // limited to 210bpm
                    angleBonus += Math.Sin(Math.Max((Math.PI / 2 - angle), 0))
                        * Math.Min((deltaTimeToBpm - 160), 50) / 50
                        * 0.05;
                }
            }

            // short stream nerf
            double distance = osuCurrent.JumpDistance + osuCurrent.TravelDistance;
            double radius = ((OsuHitObject)osuCurrent.BaseObject).Radius * osuCurrent.ScalingFactor;

            double multiplier = min_doubletap_nerf +
                Math.Max(Math.Min(distance / (radius * threshold_doubletap_contributing), 1.0), 0.0)
                * (max_doubletap_nerf - min_doubletap_nerf);
            angleBonus *= multiplier;

            return angleBonus;
        }
        private double applyDiminishingExp(double val) => Math.Pow(val, 0.99);
    }
}
