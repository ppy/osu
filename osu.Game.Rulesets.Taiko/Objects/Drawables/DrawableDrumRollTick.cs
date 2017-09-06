// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject<DrumRollTick>
    {
        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            // Because ticks aren't added by the ScrollingPlayfield, we need to set the following properties ourselves
            RelativePositionAxes = Axes.X;
            X = (float)tick.StartTime;

            FillMode = FillMode.Fit;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // We need to set this here because RelativeSizeAxes won't/can't set our size by default with a different RelativeChildSize
            Width *= Parent.RelativeChildSize.X;
        }

        protected override TaikoPiece CreateMainPiece() => new TickPiece
        {
            Filled = HitObject.FirstTick
        };

        protected override void CheckForJudgements(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
                return;

            if (!(Math.Abs(timeOffset) < HitObject.HitWindow))
                return;

            AddJudgement(new TaikoDrumRollTickJudgement { Result = HitResult.Great });
            if (HitObject.IsStrong)
                AddJudgement(new TaikoStrongHitJudgement());
        }

        protected override void UpdateState(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    Content.ScaleTo(0, 100, Easing.OutQuint);
                    break;
            }
        }

        public override bool OnPressed(TaikoAction action) => UpdateJudgement(true);
    }
}
