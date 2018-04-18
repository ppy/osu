// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        /// <param name="judgement">The judgement to visualise.</param>
        public DrawableTaikoJudgement(Judgement judgement, DrawableHitObject judgedObject)
            : base(judgement, judgedObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (Judgement.Result)
            {
                case HitResult.Good:
                    Colour = colours.GreenLight;
                    break;
                case HitResult.Great:
                    Colour = colours.BlueLight;
                    break;
            }
        }

        protected override void LoadComplete()
        {
            if (Judgement.IsHit)
                this.MoveToY(-100, 500);

            base.LoadComplete();
        }
    }
}
