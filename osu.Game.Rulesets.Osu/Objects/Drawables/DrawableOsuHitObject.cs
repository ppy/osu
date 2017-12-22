// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.ComponentModel;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Framework.Graphics;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuHitObject : DrawableHitObject<OsuHitObject>
    {
        /// <summary>
        /// This should be calculated in the future.
        /// </summary>
        public const float TIME_PREEMPT = 600;

        public const float TIME_FADEIN = 400;
        public const float TIME_FADEOUT = 500;

        public bool HiddenMod;
        public double FadeInSpeedMultiplier = 1;
        public double FadeOutSpeedMultiplier = 1;

        /// <summary>
        /// The number of milliseconds the expiration should be delayed.
        /// </summary>
        protected double ExpireAfter;

        public override bool IsPresent => base.IsPresent || State.Value == ArmedState.Idle && Time.Current >= HitObject.StartTime - TIME_PREEMPT;

        protected DrawableOsuHitObject(OsuHitObject hitObject)
            : base(hitObject)
        {
            AccentColour = HitObject.ComboColour;
            Alpha = 0;
        }

        protected sealed override void UpdateState(ArmedState state)
        {
            double transformTime = HitObject.StartTime - TIME_PREEMPT;

            base.ApplyTransformsAt(transformTime, true);
            base.ClearTransformsAfter(transformTime, true);

            using (BeginAbsoluteSequence(transformTime, true))
            {
                UpdatePreemptState();

                double delay = TIME_PREEMPT;
                if (HiddenMod)
                {
                    delay *= FadeInSpeedMultiplier;
                    // If we shorten the delay we fade out earlier than we actually play the HitObject
                    // We need to keep some DrawableHitObjects alive for a bit longer so they stay playable.
                    ExpireAfter = TIME_PREEMPT - TIME_PREEMPT * FadeInSpeedMultiplier;
                }

                delay += Judgements.FirstOrDefault()?.TimeOffset ?? 0;
                using (BeginDelayedSequence(delay, true))
                    UpdateCurrentState(state);
            }
        }

        protected virtual void UpdatePreemptState()
        {
            double duration;
            if (HiddenMod)
                duration = HitObject.StartTime - TIME_PREEMPT * FadeInSpeedMultiplier - (HitObject.StartTime - TIME_PREEMPT);
            else
                duration = TIME_FADEIN * FadeInSpeedMultiplier;

            this.FadeIn(duration);
        }

        protected virtual void UpdateCurrentState(ArmedState state)
        {

        }

        // Todo: At some point we need to move these to DrawableHitObject after ensuring that all other Rulesets apply
        // transforms in the same way and don't rely on them not being cleared
        public override void ClearTransformsAfter(double time, bool propagateChildren = false, string targetMember = null) { }
        public override void ApplyTransformsAt(double time, bool propagateChildren = false) { }

        private OsuInputManager osuActionInputManager;
        internal OsuInputManager OsuActionInputManager => osuActionInputManager ?? (osuActionInputManager = GetContainingInputManager() as OsuInputManager);
    }

    public enum ComboResult
    {
        [Description(@"")]
        None,
        [Description(@"Good")]
        Good,
        [Description(@"Amazing")]
        Perfect
    }
}
