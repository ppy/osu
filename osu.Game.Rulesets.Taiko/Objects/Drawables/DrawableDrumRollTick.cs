// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public partial class DrawableDrumRollTick : DrawableTaikoStrongableHitObject<DrumRollTick, DrumRollTick.StrongNestedHit>
    {
        public BindableBool IsFirstTick = new BindableBool();

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

        protected override SkinnableDrawable CreateMainPiece() => new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.DrumRollTick), _ => new TickPiece());

        protected override void OnApply()
        {
            base.OnApply();

            IsFirstTick.Value = HitObject.FirstTick;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (!userTriggered)
            {
                if (timeOffset > HitObject.HitWindow)
                    ApplyMinResult();
                return;
            }

            if (Math.Abs(timeOffset) > HitObject.HitWindow)
                return;

            ApplyMaxResult();
        }

        public override void OnKilled()
        {
            base.OnKilled();

            if (Time.Current > HitObject.GetEndTime() && !Judged)
                ApplyMinResult();
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(1.4f, 200, Easing.OutQuint);
                    this.FadeOut(200, Easing.OutQuint);
                    break;
            }
        }

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            JudgementType = e.Action == TaikoAction.LeftRim || e.Action == TaikoAction.RightRim ? HitType.Rim : HitType.Centre;
            return UpdateResult(true);
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(DrumRollTick.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        public partial class StrongNestedHit : DrawableStrongNestedHit
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

                ApplyResult(static (r, hitObject) =>
                {
                    var nestedHit = (StrongNestedHit)hitObject;
                    r.Type = nestedHit.ParentHitObject!.IsHit ? r.Judgement.MaxResult : r.Judgement.MinResult;
                });
            }

            public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e) => false;
        }
    }
}
