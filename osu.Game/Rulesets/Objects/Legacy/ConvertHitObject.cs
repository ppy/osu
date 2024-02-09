// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Objects.Legacy
{
    /// <summary>
    /// A hit object only used for conversion, not actual gameplay.
    /// </summary>
    internal abstract class ConvertHitObject : HitObject, IHasCombo
    {
        public bool NewCombo { get; set; }

        public int ComboOffset { get; set; }

        protected override Judgement CreateJudgement() => new IgnoreJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}
