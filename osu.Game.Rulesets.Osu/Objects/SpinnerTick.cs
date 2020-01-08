// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerTick : OsuHitObject
    {
        public SpinnerTick()
        {
            Samples.Add(new HitSampleInfo { Name = "spinnerbonus" });
        }

        public override Judgement CreateJudgement() => new OsuSpinnerTickJudgement();

        protected override HitWindows CreateHitWindows() => HitWindows.Empty;
    }
}