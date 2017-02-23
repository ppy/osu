using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using osu.Framework.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableTaikoHitObject : DrawableHitObject
    {
        public const float TIME_PREEMPT = 600;

        public DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            RelativePositionAxes = Axes.X;
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo { MaxScore = TaikoScoreResult.Great };

        /// <summary>
        /// Todo: Remove
        /// </summary>
        protected override void LoadComplete()
        {
            if (Judgement == null)
                Judgement = CreateJudgementInfo();

            UpdateState(State);
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            Flush();

            UpdateInitialState();

            Delay(HitObject.StartTime - Time.Current - TIME_PREEMPT + Judgement.TimeOffset, true);

            UpdatePreemptState();

            Delay(TIME_PREEMPT, true);
        }

        protected virtual void UpdateInitialState()
        {
            MoveToX(1.1f);
        }

        protected virtual void UpdatePreemptState()
        {
            MoveToX(0, TIME_PREEMPT);
        }
    }
}
