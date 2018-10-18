﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using OpenTK.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        public override bool IsPresent => base.IsPresent || State.Value == ArmedState.Idle && Clock?.CurrentTime >= HitObject.StartTime - HitObject.TimePreempt;

        private readonly ShakeContainer shakeContainer;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
            base.AddInternal(shakeContainer = new ShakeContainer { RelativeSizeAxes = Axes.Both });
            Alpha = 0;
        }

        // Forward all internal management to shakeContainer.
        // This is a bit ugly but we don't have the concept of InternalContent so it'll have to do for now. (https://github.com/ppy/osu-framework/issues/1690)
        protected override void AddInternal(Drawable drawable) => shakeContainer.Add(drawable);
        protected override void ClearInternal(bool disposeChildren = true) => shakeContainer.Clear(disposeChildren);
        protected override bool RemoveInternal(Drawable drawable) => shakeContainer.Remove(drawable);

        protected sealed override void UpdateState(ArmedState state)
        {
            double transformTime = HitObject.StartTime - HitObject.TimePreempt;

            base.ApplyTransformsAt(transformTime, true);
            base.ClearTransformsAfter(transformTime, true);

            using (BeginAbsoluteSequence(transformTime, true))
            {
                UpdatePreemptState();

                var judgementOffset = Math.Min(HitObject.HitWindows.HalfWindowFor(HitResult.Miss), Result?.TimeOffset ?? 0);

                using (BeginDelayedSequence(HitObject.TimePreempt + judgementOffset, true))
                    UpdateCurrentState(state);
            }
        }

        protected override void SkinChanged(ISkinSource skin, bool allowFallback)
        {
            base.SkinChanged(skin, allowFallback);

            if (HitObject is IHasComboInformation combo)
                AccentColour = skin.GetValue<SkinConfiguration, Color4>(s => s.ComboColours.Count > 0 ? s.ComboColours[combo.ComboIndex % s.ComboColours.Count] : (Color4?)null) ?? Color4.White;
        }

        protected virtual void UpdatePreemptState() => this.FadeIn(HitObject.TimeFadeIn);

        protected virtual void UpdateCurrentState(ArmedState state)
        {
        }

        // Todo: At some point we need to move these to DrawableHitObject after ensuring that all other Rulesets apply
        // transforms in the same way and don't rely on them not being cleared
        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null)
        {
        }

        public override void ApplyTransformsAt(double time, bool propagateChildren = false)
        {
        }

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ?? (osuActionInputManager = GetContainingInputManager() as OsuInputManager);

        protected virtual void Shake(double maximumLength) => shakeContainer.Shake(maximumLength);

        protected override JudgementResult CreateResult(Judgement judgement) => new OsuJudgementResult(judgement);
    }
}
