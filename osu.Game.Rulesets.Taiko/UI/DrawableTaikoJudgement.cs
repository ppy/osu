// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public class DrawableTaikoJudgement : DrawableJudgement
    {
        /// <summary>
        /// Creates a new judgement text.
        /// </summary>
        /// <param name="judgedObject">The object which is being judged.</param>
        /// <param name="result">The judgement to visualise.</param>
        public DrawableTaikoJudgement(JudgementResult result, DrawableHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (Result.Type)
            {
                case HitResult.Good:
                    JudgementBody.Colour = colours.GreenLight;
                    break;

                case HitResult.Great:
                    JudgementBody.Colour = colours.BlueLight;
                    break;
            }
        }

        protected override void ApplyHitAnimations()
        {
            this.MoveToY(-100, 500);
            base.ApplyHitAnimations();
        }
    }
}
