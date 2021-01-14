// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Skinning.Default;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Taiko.Objects.Drawables
{
    public class DrawableHit : DrawableTaikoStrongableHitObject<Hit, Hit.StrongNestedHit>
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

        private bool pressHandledThisFrame;

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
            type.BindValueChanged(_ =>
            {
                updateActionsFromType();

                // will overwrite samples, should only be called on subsequent changes
                // after the initial application.
                updateSamplesFromTypeChange();

                RecreatePieces();
            });

            // action update also has to happen immediately on application.
            updateActionsFromType();

            base.OnApply();
        }

        protected override void OnFree()
        {
            base.OnFree();

            type.UnbindFrom(HitObject.TypeBindable);
            type.UnbindEvents();

            UnproxyContent();

            HitActions = null;
            HitAction = null;
            validActionPressed = pressHandledThisFrame = false;
        }

        private HitSampleInfo[] getRimSamples() => HitObject.Samples.Where(s => s.Name == HitSampleInfo.HIT_CLAP || s.Name == HitSampleInfo.HIT_WHISTLE).ToArray();

        protected override void LoadSamples()
        {
            base.LoadSamples();

            type.Value = getRimSamples().Any() ? HitType.Rim : HitType.Centre;
        }

        private void updateSamplesFromTypeChange()
        {
            var rimSamples = getRimSamples();

            bool isRimType = HitObject.Type == HitType.Rim;

            if (isRimType != rimSamples.Any())
            {
                if (isRimType)
                    HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_CLAP));
                else
                {
                    foreach (var sample in rimSamples)
                        HitObject.Samples.Remove(sample);
                }
            }
        }

        private void updateActionsFromType()
        {
            HitActions =
                HitObject.Type == HitType.Centre
                    ? new[] { TaikoAction.LeftCentre, TaikoAction.RightCentre }
                    : new[] { TaikoAction.LeftRim, TaikoAction.RightRim };
        }

        protected override SkinnableDrawable CreateMainPiece() => HitObject.Type == HitType.Centre
            ? new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.CentreHit), _ => new CentreHitCirclePiece(), confineMode: ConfineMode.ScaleToFit)
            : new SkinnableDrawable(new TaikoSkinComponent(TaikoSkinComponents.RimHit), _ => new RimHitCirclePiece(), confineMode: ConfineMode.ScaleToFit);

        public override IEnumerable<HitSampleInfo> GetSamples()
        {
            // normal and claps are always handled by the drum (see DrumSampleMapping).
            // in addition, whistles are excluded as they are an alternative rim marker.

            var samples = HitObject.Samples.Where(s =>
                s.Name != HitSampleInfo.HIT_NORMAL
                && s.Name != HitSampleInfo.HIT_CLAP
                && s.Name != HitSampleInfo.HIT_WHISTLE);

            if (HitObject.Type == HitType.Rim && HitObject.IsStrong)
            {
                // strong + rim always maps to whistle.
                // TODO: this should really be in the legacy decoder, but can't be because legacy encoding parity would be broken.
                // when we add a taiko editor, this is probably not going to play nice.

                var corrected = samples.ToList();

                for (var i = 0; i < corrected.Count; i++)
                {
                    var s = corrected[i];

                    if (s.Name != HitSampleInfo.HIT_FINISH)
                        continue;

                    corrected[i] = s.With(HitSampleInfo.HIT_WHISTLE);
                }

                return corrected;
            }

            return samples;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            Debug.Assert(HitObject.HitWindows != null);

            if (!userTriggered)
            {
                if (!HitObject.HitWindows.CanBeHit(timeOffset))
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                return;
            }

            var result = HitObject.HitWindows.ResultFor(timeOffset);
            if (result == HitResult.None)
                return;

            if (!validActionPressed)
                ApplyResult(r => r.Type = r.Judgement.MinResult);
            else
                ApplyResult(r => r.Type = result);
        }

        public override bool OnPressed(TaikoAction action)
        {
            if (pressHandledThisFrame)
                return true;
            if (Judged)
                return false;

            validActionPressed = HitActions.Contains(action);

            // Only count this as handled if the new judgement is a hit
            var result = UpdateResult(true);
            if (IsHit)
                HitAction = action;

            // Regardless of whether we've hit or not, any secondary key presses in the same frame should be discarded
            // E.g. hitting a non-strong centre as a strong should not fall through and perform a hit on the next note
            pressHandledThisFrame = true;
            return result;
        }

        public override void OnReleased(TaikoAction action)
        {
            if (action == HitAction)
                HitAction = null;
            base.OnReleased(action);
        }

        protected override void Update()
        {
            base.Update();

            // The input manager processes all input prior to us updating, so this is the perfect time
            // for us to remove the extra press blocking, before input is handled in the next frame
            pressHandledThisFrame = false;
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
                    // If we're far enough away from the left stage, we should bring outselves in front of it
                    ProxyContent();

                    var flash = (MainPiece.Drawable as CirclePiece)?.FlashBox;
                    flash?.FadeTo(0.9f).FadeOut(300);

                    const float gravity_time = 300;
                    const float gravity_travel_height = 200;

                    this.ScaleTo(0.8f, gravity_time * 2, Easing.OutQuad);

                    this.MoveToY(-gravity_travel_height, gravity_time, Easing.Out)
                        .Then()
                        .MoveToY(gravity_travel_height * 2, gravity_time * 2, Easing.In);

                    this.FadeOut(800);
                    break;
            }
        }

        protected override DrawableStrongNestedHit CreateStrongNestedHit(Hit.StrongNestedHit hitObject) => new StrongNestedHit(hitObject);

        public class StrongNestedHit : DrawableStrongNestedHit
        {
            public new DrawableHit ParentHitObject => (DrawableHit)base.ParentHitObject;

            /// <summary>
            /// The lenience for the second key press.
            /// This does not adjust by map difficulty in ScoreV2 yet.
            /// </summary>
            private const double second_hit_window = 30;

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
                    ApplyResult(r => r.Type = r.Judgement.MinResult);
                    return;
                }

                if (!userTriggered)
                {
                    if (timeOffset - ParentHitObject.Result.TimeOffset > second_hit_window)
                        ApplyResult(r => r.Type = r.Judgement.MinResult);
                    return;
                }

                if (Math.Abs(timeOffset - ParentHitObject.Result.TimeOffset) <= second_hit_window)
                    ApplyResult(r => r.Type = r.Judgement.MaxResult);
            }

            public override bool OnPressed(TaikoAction action)
            {
                // Don't process actions until the main hitobject is hit
                if (!ParentHitObject.IsHit)
                    return false;

                // Don't process actions if the pressed button was released
                if (ParentHitObject.HitAction == null)
                    return false;

                // Don't handle invalid hit action presses
                if (!ParentHitObject.HitActions.Contains(action))
                    return false;

                return UpdateResult(true);
            }
        }
    }
}
