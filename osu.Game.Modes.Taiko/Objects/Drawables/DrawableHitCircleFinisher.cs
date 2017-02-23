using OpenTK;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Modes.Objects.Drawables;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDonFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleDonFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonFinisherPiece();
    }

    public class DrawableHitCircleKatsuFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleKatsuFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuFinisherPiece();
    }

    public abstract class DrawableHitCircleFinisher : DrawableHitCircle
    {
        public DrawableHitCircleFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size *= 1.5f;
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoFinisherJudgementInfo() { MaxScore = TaikoScoreResult.Great };

        protected override bool ProcessHit()
        {
            TaikoFinisherJudgementInfo taikoJudgement = Judgement as TaikoFinisherJudgementInfo;

            if (taikoJudgement.FirstHitJudgement.Result.HasValue)
                return base.ProcessHit();

            UpdateJudgement(true);
            return true;
        }

        double secondHitTime = 30;

        protected override void CheckJudgement(bool userTriggered)
        {
            TaikoFinisherJudgementInfo taikoJudgement = Judgement as TaikoFinisherJudgementInfo;

            if (!taikoJudgement.FirstHitJudgement.Result.HasValue)
            {
                base.CheckJudgement(userTriggered);
                return;
            }

            double hitOffset = Math.Abs(taikoJudgement.TimeOffset - taikoJudgement.FirstHitJudgement.TimeOffset);

            if (!userTriggered)
            {
                if (hitOffset > secondHitTime)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            if (hitOffset < 30)
            {
                Judgement.Result = HitResult.Hit;
                taikoJudgement.Score = taikoJudgement.FirstHitJudgement.Score;
            }
            else
                Judgement.Result = HitResult.Miss;
        }
    }
}
