// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Objects.Drawables.Pieces;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoHitObject<DrumRollTick>
    {
        /// <summary>
        /// The hit type corresponding to the <see cref="TaikoAction"/> that the user pressed to hit this <see cref="DrawableDrumRollTick"/>.
        /// </summary>
        public HitType JudgementType;

        public DrawableDrumRollTick(DrumRollTick tick)
            : base(tick)
        {
            FillMode = FillMode.Fit;
        }

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.DrumRollTick),
            _ => new TickPiece
            {
                Filled = HitObject.FirstTick
            });

        protected override double MaximumJudgementOffset => HitObject.HitWindow;

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > HitObject.HitWindow)
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                return;
            }

            if (Math.Abs(timeOffset) > HitObject.HitWindow)
                return;

            ApplyResult(r => r.Type = r.Judgement.MaxResult);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(0, 100, Easing.OutQuint);
                    break;
            }
        }

        public override bool OnPressed(TaikoAction action)
        {
            JudgementType = action == TaikoAction.LeftRim || action == TaikoAction.RightRim ? HitType.Rim : HitType.Centre;
            return UpdateResult(true);
        }

        protected override DrawableStrongNestedHit CreateStrongHit(StrongHitObject hitObject) => new StrongNestedHit(hitObject, this);

        private class StrongNestedHit : DrawableStrongNestedHit
        {
            public StrongNestedHit(StrongHitObject strong, DrawableDrumRollTick tick)
                : base(strong, tick)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (!MainObject.Judged)
                    return;

                ApplyResult(r => r.Type = MainObject.IsHit ? r.Judgement.MaxResult : r.Judgement.MinResult);
            }

            public override bool OnPressed(TaikoAction action) => false;
        }
    }
}
