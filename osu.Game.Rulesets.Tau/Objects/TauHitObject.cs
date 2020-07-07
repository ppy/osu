// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Tau.Judgements;
using osu.Game.Rulesets.Tau.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Tau.Objects
{
    public class TauHitObject : HitObject, IHasComboInformation
    {
        public override Judgement CreateJudgement() => new TauJudgement();

        public double TimePreempt = 600;
        public double TimeFadeIn = 400;

        public float Angle { get; set; }
        public Vector2 PositionToEnd { get; set; }

        public virtual bool NewCombo { get; set; }

        public readonly Bindable<int> ComboOffsetBindable = new Bindable<int>();

        public int ComboOffset
        {
            get => ComboOffsetBindable.Value;
            set => ComboOffsetBindable.Value = value;
        }

        public Bindable<int> IndexInCurrentComboBindable { get; } = new Bindable<int>();

        public virtual int IndexInCurrentCombo
        {
            get => IndexInCurrentComboBindable.Value;
            set => IndexInCurrentComboBindable.Value = value;
        }

        public Bindable<int> ComboIndexBindable { get; } = new Bindable<int>();

        public virtual int ComboIndex
        {
            get => ComboIndexBindable.Value;
            set => ComboIndexBindable.Value = value;
        }

        public Bindable<bool> LastInComboBindable { get; } = new Bindable<bool>();

        public bool LastInCombo
        {
            get => LastInComboBindable.Value;
            set => LastInComboBindable.Value = value;
        }

        protected override HitWindows CreateHitWindows() => new TauHitWindows();

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float) BeatmapDifficulty.DifficultyRange(difficulty.ApproachRate, 1800, 1200, 450);
            TimeFadeIn = 100;
        }
    }
}
