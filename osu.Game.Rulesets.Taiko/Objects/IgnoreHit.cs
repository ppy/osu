// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class IgnoreHit : Hit
    {
        protected override Judgement CreateJudgement() => new IgnoreJudgement();
    }
}
