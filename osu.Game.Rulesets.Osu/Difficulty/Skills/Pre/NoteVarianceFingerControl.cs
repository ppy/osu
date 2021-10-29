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
    public class NoteVarianceFingerControl : PrePerNoteStrainSkill
    {
        public NoteVarianceFingerControl(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.5;

        private const double min_doubletap_nerf = 0.0; // minimum value (eventually on stacked)
        private const double max_doubletap_nerf = 1.0; // maximum value 
        private const double threshold_doubletap_contributing = 2.0; // minimum distance not influenced (2.0 means it is not stacked at least)


        protected override double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            OsuDifficultyHitObject osuCurrent = (OsuDifficultyHitObject)current;

            if (osuCurrent.BaseObject is Spinner || osuCurrent.LastObject is Spinner)
                return 0;

            // calculates the percentage of alternative.
            // when bpm is 160, it gives 0.
            // when bpm is 210, 1.
            // as a result, when bpm is 210, we consider the note has 100% of percentage of alternative
            double deltaTimeToBpm = 15000 / current.DeltaTime;
            double probablityAlternative = Math.Max((deltaTimeToBpm - 160.0), 0) / (210.0 - 160.0);


            // short stream nerf
            double distance = osuCurrent.JumpDistance + osuCurrent.TravelDistance;
            double radius = ((OsuHitObject)osuCurrent.LastObject).Radius * osuCurrent.ScalingFactor;

            double multiplier = min_doubletap_nerf +
                Math.Clamp(distance / (radius * threshold_doubletap_contributing), 0.0, 1.0)
                * (max_doubletap_nerf - min_doubletap_nerf);
            probablityAlternative *= multiplier;

            return probablityAlternative;
        }
    }
}
