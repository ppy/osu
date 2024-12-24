// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public abstract partial class DrawableHit : DrawableTaikoStrongableHitObject<Hit, Hit.StrongNestedHit>
    {
        /// <summary>
        /// A list of keys which can result in hits for this HitObject.
        /// </summary>
        public abstract TaikoAction[] HitActions { get; protected set; }

        /// <summary>
        /// When this is true any hit is valid.
        /// </summary>
        public bool RelaxMod = false;

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
            base.OnApply();
        }

        protected override void RestorePieceState()
        {
            Size = new Vector2(HitObject.IsStrong ? TaikoStrongableHitObject.DEFAULT_STRONG_SIZE : TaikoHitObject.DEFAULT_SIZE);
        }

        protected override void OnFree()
        {
            base.OnFree();

            UnproxyContent();

            RelaxMod = false;
            HitAction = null;
            validActionPressed = false;
            lastPressHandleTime = null;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyMinResult();
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            if (!validActionPressed)
                ApplyMinResult();
            else
                ApplyResult(result);
        }

        public bool IsValidAction(TaikoAction action) => RelaxMod || HitActions.Contains(action);

        public override bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            if (lastPressHandleTime == Time.Current)
                return true;
            if (Judged)
                return false;

            validActionPressed = IsValidAction(e.Action);

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
                    ApplyMinResult();
                    return;
                }

                if (!userTriggered)
                {
                    if (timeOffset - ParentHitObject.Result.TimeOffset > SECOND_HIT_WINDOW)
                        ApplyMinResult();
                    return;
                }

                if (Math.Abs(timeOffset - ParentHitObject.Result.TimeOffset) <= SECOND_HIT_WINDOW)
                    ApplyMaxResult();
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
                if (!ParentHitObject.IsValidAction(e.Action))
                    return false;

                return UpdateResult(true);
            }
        }
    }

    public partial class DrawableHitCenter : DrawableHit
    {
        public override TaikoAction[] HitActions { get; protected set; } = { TaikoAction.LeftCentre, TaikoAction.RightCentre };

        /// <summary> This constructor exsits only to satisfy Pool where constraints </summary>
        public DrawableHitCenter() : base(null) { }

        public DrawableHitCenter([CanBeNull] Hit hit)
            : base(hit)
        {
            if (hit != null && hit.Type != HitType.Centre)
                throw new ArgumentException("hit must be `Centre`");
        }

        protected override SkinnableDrawable OnLoadCreateMainPiece()
            => new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.CentreHit), _ => new CentreHitCirclePiece(), confineMode: ConfineMode.ScaleToFit);
    }

    public partial class DrawableHitRim : DrawableHit
    {
        public override TaikoAction[] HitActions { get; protected set; } = { TaikoAction.LeftRim, TaikoAction.RightRim };

        /// <summary> This constructor exsits only to satisfy Pool where constraints </summary>
        public DrawableHitRim() : base(null) { }

        public DrawableHitRim([CanBeNull] Hit hit)
            : base(hit)
        {
            if (hit != null && hit.Type != HitType.Rim)
                throw new ArgumentException("hit must be `Rim`");
        }
        protected override SkinnableDrawable OnLoadCreateMainPiece()
            => new SkinnableDrawable(new TaikoSkinComponentLookup(TaikoSkinComponents.RimHit), _ => new RimHitCirclePiece(), confineMode: ConfineMode.ScaleToFit);

    }
}
