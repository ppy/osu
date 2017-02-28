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
        public DrawableTaikoHitObject(TaikoHitObject hitObject)
            : base(hitObject)
        {
            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

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

            TaikoHitObject tho = HitObject as TaikoHitObject;

            Flush();

            //UpdateInitialState();

            //Delay(HitObject.StartTime - Time.Current - tho.PreEmpt, true);

            //UpdatePreemptState();

            //Delay(tho.PreEmpt, true);
        }

        protected virtual void UpdateInitialState()
        {
        }

        protected virtual void UpdatePreemptState()
        {
        }

        protected override void Update()
        {
            if (State != ArmedState.Hit)
            {
                TaikoHitObject tho = HitObject as TaikoHitObject;
                MoveToX((float)((tho.StartTime - Time.Current) / tho.PreEmpt));
            }
        }
    }
}
