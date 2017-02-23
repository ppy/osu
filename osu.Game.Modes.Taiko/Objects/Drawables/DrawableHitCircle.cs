using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Taiko.Objects.Drawables.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Modes.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDon : DrawableHitCircle
    {

        public DrawableHitCircleDon(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonPiece();
    }

    public class DrawableHitCircleKatsu : DrawableHitCircle
    {
        public DrawableHitCircleKatsu(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuPiece();
    }

    public abstract class DrawableHitCircle : DrawableTaikoHitObject
    {
        private HitCirclePiece bodyPiece;

        public DrawableHitCircle(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size = new Vector2(128);


            Children = new[]
            {
                bodyPiece = CreateBody()
            };

            bodyPiece.Hit += ProcessHit;
        }

        protected abstract HitCirclePiece CreateBody();

        protected virtual bool ProcessHit()
        {
            if (Judgement.Result.HasValue)
                return false;

            UpdateJudgement(true);
            return true;
        }

        double hitGood = 80;
        double hitGreat = 30;

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > hitGood)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            TaikoJudgementInfo taikoJudgement = (Judgement as TaikoFinisherJudgementInfo)?.FirstHitJudgement;
            if (taikoJudgement == null)
                taikoJudgement = Judgement as TaikoJudgementInfo;

            if (hitOffset < hitGood)
            {
                Judgement.Result = HitResult.Hit;

                if (hitOffset < hitGreat)
                    taikoJudgement.Score = TaikoScoreResult.Great;
                else
                    taikoJudgement.Score = TaikoScoreResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            bodyPiece.Hit -= ProcessHit;
        }
    }
}
