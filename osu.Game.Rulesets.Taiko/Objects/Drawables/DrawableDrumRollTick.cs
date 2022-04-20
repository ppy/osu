// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableDrumRollTick : DrawableTaikoStrongableHitObject<DrumRollTick, DrumRollTick.StrongNestedHit>
    {
        /// <summary>
        /// The hit type corresponding to the <see cref="TaikoAction"/> that the user pressed to hit this <see cref="DrawableDrumRollTick"/>.
        /// </summary>
        public HitType JudgementType;

        public DrawableDrumRollTick()
            : this(null)
        {
        }

        public DrawableDrumRollTick([CanBeNull] DrumRollTick tick)
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

        public override void OnKilled()
        {
            base.OnKilled();

            if (Time.Current > HitObject.GetEndTime() && !Judged)
                ApplyResult(r => r.Type = r.Judgement.MinResult);
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

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            JudgementType = e.Action == TaikoAction.LeftRim || e.Action == TaikoAction.RightRim ? HitType.Rim : HitType.Centre;
            return UpdateResult(true);
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(DrumRollTick.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        public class StrongNestedHit : DrawableStrongNestedHit
        {
            public new DrawableDrumRollTick ParentHitObject => (DrawableDrumRollTick)base.ParentHitObject;

            public StrongNestedHit()
                : this(null)
            {
            }

            public StrongNestedHit([CanBeNull] DrumRollTick.StrongNestedHit nestedHit)
                : base(nestedHit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (!ParentHitObject.Judged)
                    return;

                ApplyResult(r => r.Type = ParentHitObject.IsHit ? r.Judgement.MaxResult : r.Judgement.MinResult);
            }

            public override void OnKilled()
            {
                base.OnKilled();

                if (Time.Current > ParentHitObject.HitObject.GetEndTime() && !Judged)
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
            }

            public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e) => false;
        }
    }
}
