// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.EmptyScrolling.Objects
{
    public class EmptyScrollingHitObject : HitObject
    {
        protected override Judgement CreateJudgement() => new Judgement();
    }
}
