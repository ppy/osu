// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class BarLine : TaikoHitObject, IBarLine
    {
        public bool Major
        {
            get => MajorBindable.Value;
            set => MajorBindable.Value = value;
        }

        public readonly Bindable<bool> MajorBindable = new BindableBool();

        public override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
