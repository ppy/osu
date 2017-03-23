// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Game.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Modes.Taiko.Judgements;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Modes.Judgements;

namespace osu.Game.Modes.Taiko.UI
{
    /// <summary>
    /// Text that is shown as judgement when a hit object is hit or missed.
    /// </summary>
    public class JudgementText : DrawableJudgementInfo<TaikoJudgementInfo>
    {
        /// <summary>
        /// Creates a new judgement text.
        /// </summary>
        /// <param name="judgement">The judgement to visualise.</param>
        public JudgementText(TaikoJudgementInfo judgement)
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
                            Colour = colours.Green;
                            break;
                        case TaikoHitResult.Great:
                            Colour = colours.Blue;
                            break;
                    }
                    break;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Judgement.Result == HitResult.Hit)
            {
                MoveToY(-100, 500);
                Delay(250);
                FadeOut(250);
            }

            Expire();
        }
    }
}