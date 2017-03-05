// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        public override Color4 ExplodeColour => new Color4(187, 17, 119, 255);

        public DrawableHitCircleDonFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new DonFinisherPiece();
    }

    public class DrawableHitCircleKatsuFinisher : DrawableHitCircleFinisher
    {
        public override Color4 ExplodeColour => new Color4(17, 136, 170, 255);

        public DrawableHitCircleKatsuFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
        }

        protected override HitCirclePiece CreateBody() => new KatsuFinisherPiece();
    }

    public abstract class DrawableHitCircleFinisher : DrawableHitCircle
    {
        private const double second_hit_window = 30;

        private bool validKeyPressed;

        public DrawableHitCircleFinisher(HitCircle hitCircle)
            : base(hitCircle)
        {
            Size *= 1.5f;
        }

        public override JudgementInfo CreateJudgementInfo() => new TaikoJudgementInfo() { MaxScore = TaikoScoreResult.Great, SecondHit = true };

        protected override bool ProcessHit(bool validKey)
        {
            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;

            if (!tji.Result.HasValue)
                return base.ProcessHit(validKey);

            validKeyPressed = validKey;

            CheckJudgement(true);
            return true;
        }

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

        protected override void UpdateAuto()
        {
            base.UpdateAuto();

            TaikoJudgementInfo tji = Judgement as TaikoJudgementInfo;
            if (!tji.SecondHit && Time.Current >= HitObject.EndTime)
                base.UpdateAuto();
        }
    }
}
