// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public class DrawableTaikoJudgement : DrawableJudgement<TaikoJudgement>
    {
        /// <summary>
        /// Creates a new judgement text.
        /// </summary>
        /// <param name="judgement">The judgement to visualise.</param>
        public DrawableTaikoJudgement(TaikoJudgement judgement)
            : base(judgement)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (Judgement.Result)
            {
                case HitResult.Hit:
                    switch (Judgement.TaikoResult)
                    {
                        case TaikoHitResult.Good:
                            Colour = colours.GreenLight;
                            break;
                        case TaikoHitResult.Great:
                            Colour = colours.BlueLight;
                            break;
                    }
                    break;
            }
        }

        protected override void LoadComplete()
        {
            switch (Judgement.Result)
            {
                case HitResult.Hit:
                    MoveToY(-100, 500);
                    break;
            }

            base.LoadComplete();
        }
    }
}