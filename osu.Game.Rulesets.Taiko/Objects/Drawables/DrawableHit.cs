// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public partial class DrawableHit : DrawableTaikoStrongableHitObject<Hit, Hit.StrongNestedHit>
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        public TaikoAction[] HitActions { get; private set; }

        /// <summary>
        /// The action that caused this <see cref="DrawableHit"/> to be hit.
        /// </summary>
        public TaikoAction? HitAction
        {
            get;
            private set;
        }

        private bool validActionPressed;

        private double? lastPressHandleTime;

        private readonly Bindable<HitType> type = new Bindable<HitType>();

        public DrawableHit()
            : this(null)
        {
        }

        public DrawableHit([CanBeNull] Hit hit)
            : base(hit)
        {
            FillMode = FillMode.Fit;
        }

        protected override void OnApply()
        {
            type.BindTo(HitObject.TypeBindable);
            // this doesn't need to be run inline as RecreatePieces is called by the base call below.
            type.BindValueChanged(_ => Scheduler.AddOnce(RecreatePieces));

            base.OnApply();
        }

        protected override void RecreatePieces()
        {
            updateActionsFromType();
            base.RecreatePieces();
        }

        protected override void OnFree()
        {
            base.OnFree();

            type.UnbindFrom(HitObject.TypeBindable);
            type.UnbindEvents();

            UnproxyContent();

            HitActions = null;
            HitAction = null;
            validActionPressed = false;
            lastPressHandleTime = null;
        }

        private void updateActionsFromType()
        {
            HitActions =
                HitObject.Type == HitType.Centre
                    ? new[] { TaikoAction.LeftCentre, TaikoAction.RightCentre }
                    : new[] { TaikoAction.LeftRim, TaikoAction.RightRim };
        }

        protected override SkinnableDrawable CreateMainPiece() => HitObject.Type == HitType.Centre
            ? new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.CentreHit), _ => new CentreHitCirclePiece(), confineMode: ConfineMode.ScaleToFit)
            : new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.RimHit), _ => new RimHitCirclePiece(), confineMode: ConfineMode.ScaleToFit);

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.JudgementInfo.MinResult);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            if (!validActionPressed)
                ApplyResult(r => r.Type = r.JudgementInfo.MinResult);
            else
                ApplyResult(r => r.Type = result);
        }

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            if (lastPressHandleTime == Time.Current)
                return true;
            if (Judged)
                return false;

            validActionPressed = HitActions.Contains(e.Action);

            // Only count this as handled if the new judgement is a hit
            bool result = UpdateResult(true);
            if (IsHit)
                HitAction = e.Action;

            // Regardless of whether we've hit or not, any secondary key presses in the same frame should be discarded
            // E.g. hitting a non-strong centre as a strong should not fall through and perform a hit on the next note
            lastPressHandleTime = Time.Current;
            return result;
        }

        public override void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
        {
            if (e.Action == HitAction)
                HitAction = null;
            base.OnReleased(e);
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            Debug.Assert(HitObject.HitWindows != null);

            switch (state)
            {
                case ArmedState.Idle:
                    validActionPressed = false;

                    UnproxyContent();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(100);
                    break;

                case ArmedState.Hit:
                    // If we're far enough away from the left stage, we should bring ourselves in front of it
                    ProxyContent();

                    const float gravity_time = 300;
                    const float gravity_travel_height = 200;

                    if (SnapJudgementLocation)
                        MainPiece.MoveToX(-X);

                    this.ScaleTo(0.8f, gravity_time * 2, Easing.OutQuad);

                    this.MoveToY(-gravity_travel_height, gravity_time, Easing.Out)
                        .Then()
                        .MoveToY(gravity_travel_height * 2, gravity_time * 2, Easing.In);

                    this.FadeOut(800);
                    break;
            }
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(Hit.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        public partial class StrongNestedHit : DrawableStrongNestedHit
        {
            public new DrawableHit ParentHitObject => (DrawableHit)base.ParentHitObject;

            /// <summary>
            /// The lenience for the second key press.
            /// This does not adjust by map difficulty in ScoreV2 yet.
            /// </summary>
            public const double SECOND_HIT_WINDOW = 30;

            public StrongNestedHit()
                : this(null)
            {
            }

            public StrongNestedHit([CanBeNull] Hit.StrongNestedHit nestedHit)
                : base(nestedHit)
            {
            }

            protected override void CheckForResult(bool userTriggered, double timeOffset)
            {
                if (!ParentHitObject.Result.HasResult)
                {
                    base.CheckForResult(userTriggered, timeOffset);
                    return;
                }

                if (!ParentHitObject.Result.IsHit)
                {
                    ApplyResult(r => r.Type = r.JudgementInfo.MinResult);
                    return;
                }

                if (!userTriggered)
                {
                    if (timeOffset - ParentHitObject.Result.TimeOffset > SECOND_HIT_WINDOW)
                        ApplyResult(r => r.Type = r.JudgementInfo.MinResult);
                    return;
                }

                if (Math.Abs(timeOffset - ParentHitObject.Result.TimeOffset) <= SECOND_HIT_WINDOW)
                    ApplyResult(r => r.Type = r.JudgementInfo.MaxResult);
            }

            public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
            {
                // Don't process actions until the main hitobject is hit
                if (!ParentHitObject.IsHit)
                    return false;

                // Don't process actions if the pressed button was released
                if (ParentHitObject.HitAction == null)
                    return false;

                // Don't handle invalid hit action presses
                if (!ParentHitObject.HitActions.Contains(e.Action))
                    return false;

                return UpdateResult(true);
            }
        }
    }
}
