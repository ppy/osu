using System;
using System.Collections.Generic;
using System.Text;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Mania.UI
{
    public class DrawableManiaJudgementAdjustmentContainer : JudgementContainer<DrawableManiaJudgement>
    {
        private float hitTargetPosition = 110;
        private float scorePosition;

        public float HitTargetPosition
        {
            get => hitTargetPosition;
            set
            {
                hitTargetPosition = value;
                Y = value + scorePosition + 150;
            }
        }

        public float ScorePosition
        {
            set
            {
                scorePosition = value;
                Y = hitTargetPosition + value + 150;
            }
        }

        public DrawableManiaJudgementAdjustmentContainer()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
        }
    }
}
