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
        /// The number of milliseconds before <see cref="Rulesets.Objects.HitObject.StartTime"/> we should start fading in.
        /// <para>This should be calculated in the future.</para>
        /// </summary>
        public const float TIME_PREEMPT = 600;
        public const float TIME_FADEIN = 400;

        public double FadeOutSpeedMultiplier = 1;

        /// <summary>
        /// The number of milliseconds to fade in.
        /// </summary>
        public double FadeIn = TIME_FADEIN;

        /// <summary>
        /// The number of milliseconds, starting at fade in, that the <see cref="UpdateCurrentState(ArmedState)"/> should be delayed by.
        /// </summary>
        public double DelayUpdates = TIME_PREEMPT;

        /// <summary>
        /// The number of milliseconds the duration should be extended to guarantee a correct lifetime.
        /// </summary>
        public double ExtendDuration;

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

                var delay = DelayUpdates + (Judgements.FirstOrDefault()?.TimeOffset ?? 0);
                using (BeginDelayedSequence(delay, true))
                    UpdateCurrentState(state);
            }
        }

        protected virtual void UpdatePreemptState() => this.FadeIn(FadeIn);

        protected virtual void UpdateCurrentState(ArmedState state)
        {

        }

        // TODO: At some point we need to move these to DrawableHitObject after ensuring that all other Rulesets apply
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
