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
using OpenTK.Graphics;
using System.Diagnostics;

namespace osu.Game.Modes.Taiko.Objects.Drawables
{
    public class DrawableHitCircleDon : DrawableHitCircle
    {
        public override Color4 ExplodeColour => new Color4(187, 17, 119, 255);

        public DrawableHitCircleDon(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonPiece();
    }

    public class DrawableHitCircleKatsu : DrawableHitCircle
    {
        public override Color4 ExplodeColour => new Color4(17, 136, 170, 255);

        public DrawableHitCircleKatsu(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuPiece();
    }

    public abstract class DrawableHitCircle : DrawableTaikoHitObject
    {
        private HitCirclePiece bodyPiece;
        private Container bodyContainer;

        private bool validKeyPressed = true;

        public DrawableHitCircle(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size = new Vector2(64);

            Children = new Drawable[]
            {
                bodyContainer = new Container()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    
                    Children = new[]
                    {
                        bodyPiece = CreateBody()
                    }
                }
            };

            bodyPiece.Kiai = hitCircle.Kiai;
            bodyPiece.Hit += ProcessHit;
        }

        protected abstract HitCirclePiece CreateBody();

        protected virtual bool ProcessHit(bool validKey)
        {
            if (Judgement.Result.HasValue)
                return false;

            validKeyPressed = validKey;

            return UpdateJudgement(true);
        }

        double hitMiss = 95;
        double hitGood = 80;
        double hitGreat = 35;

        protected override void CheckJudgement(bool userTriggered)
        {
            if (!userTriggered)
            {
                if (Judgement.TimeOffset > hitGood)
                    Judgement.Result = HitResult.Miss;
                return;
            }

            double hitOffset = Math.Abs(Judgement.TimeOffset);

            // Must be within great range to be hittable/missable
            if (hitOffset > hitMiss)
                return;

            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;

            if (!validKeyPressed)
                Judgement.Result = HitResult.Miss;
            else if (hitOffset < hitGood)
            {
                Judgement.Result = HitResult.Hit;

                if (hitOffset < hitGreat)
                    tji.Score = TaikoScoreResult.Great;
                else
                    tji.Score = TaikoScoreResult.Good;
            }
            else
                Judgement.Result = HitResult.Miss;
        }

        protected override void UpdateState(ArmedState state)
        {
            if (!IsLoaded)
                return;

            base.UpdateState(state);

            switch (State)
            {
                case ArmedState.Idle:
                    break;
                case ArmedState.Miss:
                    break;
                case ArmedState.Hit:
                    const double scale_out = 150;

                    bodyContainer.ScaleTo(1.5f, scale_out);
                    bodyContainer.FadeOut(scale_out);
                    break;
            }
        }

        protected override void Update()
        {
            if (State != ArmedState.Hit)
                base.Update();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            bodyPiece.Hit -= ProcessHit;
        }
    }
}
