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
using OpenTK.Graphics;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDonFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleDonFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonFinisherPiece();

        protected override ExplodePiece CreateExplode() => new FinisherExplodePiece()
        {
            Colour = new Color4(187, 17, 119, 255)
        };

        protected override FlashPiece CreateFlash() => new FinisherFlashPiece();
    }

    public class DrawableHitCircleKatsuFinisher : DrawableHitCircleFinisher
    {
        public DrawableHitCircleKatsuFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuFinisherPiece();

        protected override ExplodePiece CreateExplode() => new FinisherExplodePiece()
        {
            Colour = new Color4(17, 136, 170, 255),
        };

        protected override FlashPiece CreateFlash() => new FinisherFlashPiece();
    }

    public abstract class DrawableHitCircleFinisher : DrawableHitCircle
    {
        private bool validKeyPressed;

        public DrawableHitCircleFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size *= 1.5f;
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo() { MaxScore = TaikoScoreResult.Great };

        protected override bool ProcessHit(bool validKey)
        {
            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;

            if (!tji.Result.HasValue)
                return base.ProcessHit(validKey);

            validKeyPressed = validKey;

            CheckJudgement(true);
            return true;
        }

        double secondHitTime = 30;

        protected override void CheckJudgement(bool userTriggered)
        {
            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;

            if (!tji.Result.HasValue)
            {
                base.CheckJudgement(userTriggered);
                return;
            }

            double timeOffset = Time.Current - HitObject.EndTime;
            double hitOffset = Math.Abs(timeOffset - tji.TimeOffset);

            if (!userTriggered)
                return;

            if (!validKeyPressed)
                return;

            if (hitOffset < 30)
                tji.SecondHit = true;
        }
    }
}
