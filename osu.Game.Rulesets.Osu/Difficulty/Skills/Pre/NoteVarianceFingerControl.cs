using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Osu.Difficulty.Skills.Pre
{
    public class NoteVarianceFingerControl : PrePerNoteStrainSkill
    {
        public NoteVarianceFingerControl(IBeatmap beatmap, Mod[] mods, double clockRate) : base(beatmap, mods, clockRate)
        {
        }

        protected override double SkillMultiplier => 1;

        protected override double StrainDecayBase => 0.5;

        protected override double StrainValueOf(PrePerNoteStrainSkill[] preSkills, int index, DifficultyHitObject current)
        {
            // 양타 가능성을 계산한다.
            // bpm이 150일때 0
            // bpm이 200일때 1로 처리
            // 즉 bpm이 200이라면 무조건 이사람은 양타할걸로 보는것이다.
            // 200브픔을 단타로 치는 사람은 극히 드물거고 그정도면 개잘하는것

            // calculates the percentage of alternative.
            // when bpm is 150, it gives 0.
            // when bpm is 200, 1.
            // as a result, when bpm is 200, we consider the note has 100% of percentage of alternative
            double deltaTimeToBpm = 15000 / current.DeltaTime;
            double probablityAlternative = Math.Max((deltaTimeToBpm - 150.0), 0) / (200.0 - 150.0);

            return probablityAlternative;
        }
    }
}
