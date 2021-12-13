// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    public abstract class PreStrainSkill : Skill
    {
        protected readonly ReverseQueue<double> PreviousValue;

        protected abstract double StrainDecayBase { get; }

        protected abstract double SkillMultiplier { get; }

        protected double CurrentStrain { get; private set; }

        protected PreStrainSkill(Mod[] mods)
            : base(mods)
        {
            PreviousValue = new ReverseQueue<double>(HistoryLength + 1);
        }

        protected override void Process(int index, DifficultyHitObject current)
        {
            while (PreviousValue.Count > HistoryLength)
                PreviousValue.Dequeue();

            CurrentStrain *= StrainDecayBase;
            CurrentStrain += StrainValueOf(index, current);

            PreviousValue.Enqueue(CurrentStrain);
        }

        protected abstract double StrainValueOf(int index, DifficultyHitObject current);

        public void ProcessPre(int index, DifficultyHitObject current)
        {
            ProcessInternal(index, current);
        }

        public double this[int i] => PreviousValue[i];

        public override double DifficultyValue()
        {
            return 0;
        }
    }
}
