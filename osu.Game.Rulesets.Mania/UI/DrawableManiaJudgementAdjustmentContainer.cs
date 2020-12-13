using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DrawableManiaJudgementAdjustmentContainer : JudgementContainer<DrawableManiaJudgement>
    {
        private float scorePosition => 0;
        public DrawableManiaJudgementAdjustmentContainer(float hitTargetPosition)
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Y = hitTargetPosition + 150;
        }

        public DrawableManiaJudgementAdjustmentContainer()
            : this(110) { }
    }
}
