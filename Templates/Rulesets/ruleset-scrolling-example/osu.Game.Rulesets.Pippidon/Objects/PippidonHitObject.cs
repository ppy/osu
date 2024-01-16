// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Pippidon.Objects
{
    public class PippidonHitObject : HitObject
    {
        /// <summary>
        /// Range = [-1,1]
        /// </summary>
        public int Lane;

        public override JudgementInfo CreateJudgement() => new JudgementInfo();
    }
}
