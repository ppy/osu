using osu.Game.Modes.Objects.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects;

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

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            Flush();


            throw new NotImplementedException();
        }
    }
}
