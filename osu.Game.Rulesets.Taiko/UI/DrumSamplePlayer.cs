// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<TaikoAction>
    {
        private readonly DrumSampleTriggerSource leftCentreTrigger;
        private readonly DrumSampleTriggerSource rightCentreTrigger;
        private readonly DrumSampleTriggerSource leftRimTrigger;
        private readonly DrumSampleTriggerSource rightRimTrigger;
        private readonly DrumSampleTriggerSource strongCentreTrigger;
        private readonly DrumSampleTriggerSource strongRimTrigger;

        private double lastHitTime;
        private TaikoAction? lastAction;

        public DrumSamplePlayer(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new Drawable[]
            {
                leftCentreTrigger = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightCentreTrigger = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Right),
                leftRimTrigger = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightRimTrigger = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Right),
                strongCentreTrigger = new DrumSampleTriggerSource(hitObjectContainer),
                strongRimTrigger = new DrumSampleTriggerSource(hitObjectContainer)
            };
        }

        public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            HitType hitType;

            DrumSampleTriggerSource triggerSource;

            bool strong = checkStrongValidity(e.Action, lastAction, Time.Current - lastHitTime);

            switch (e.Action)
            {
                case TaikoAction.LeftCentre:
                    hitType = HitType.Centre;
                    triggerSource = strong ? strongCentreTrigger : leftCentreTrigger;
                    break;

                case TaikoAction.RightCentre:
                    hitType = HitType.Centre;
                    triggerSource = strong ? strongCentreTrigger : rightCentreTrigger;
                    break;

                case TaikoAction.LeftRim:
                    hitType = HitType.Rim;
                    triggerSource = strong ? strongRimTrigger : leftRimTrigger;
                    break;

                case TaikoAction.RightRim:
                    hitType = HitType.Rim;
                    triggerSource = strong ? strongRimTrigger : rightRimTrigger;
                    break;

                default:
                    return false;
            }

            if (strong && hitType == HitType.Centre)
                flushCenterTriggerSources();

            if (strong && hitType == HitType.Rim)
                flushRimTriggerSources();

            triggerSource.Play(hitType, strong);

            lastHitTime = Time.Current;
            lastAction = e.Action;

            return false;
        }

        private bool checkStrongValidity(TaikoAction newAction, TaikoAction? lastAction, double timeBetweenActions)
        {
            if (lastAction == null)
                return false;

            if (timeBetweenActions > DrawableHit.StrongNestedHit.SECOND_HIT_WINDOW)
                return false;

            switch (newAction)
            {
                case TaikoAction.LeftCentre:
                    return lastAction == TaikoAction.RightCentre;

                case TaikoAction.RightCentre:
                    return lastAction == TaikoAction.LeftCentre;

                case TaikoAction.LeftRim:
                    return lastAction == TaikoAction.RightRim;

                case TaikoAction.RightRim:
                    return lastAction == TaikoAction.LeftRim;

                default:
                    return false;
            }
        }

        private void flushCenterTriggerSources()
        {
            leftCentreTrigger.StopAllPlayback();
            rightCentreTrigger.StopAllPlayback();
            strongCentreTrigger.StopAllPlayback();
        }

        private void flushRimTriggerSources()
        {
            leftRimTrigger.StopAllPlayback();
            rightRimTrigger.StopAllPlayback();
            strongRimTrigger.StopAllPlayback();
        }

        public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
        {
        }
    }
}
