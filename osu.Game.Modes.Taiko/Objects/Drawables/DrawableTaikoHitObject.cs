using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;
using osu.Framework.Graphics.Transformations;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableTaikoHitObject : DrawableHitObject
    {
        public const float TIME_PREEMPT = 600;

        private float? scrollTime;

        public DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
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
        }
    }
}
