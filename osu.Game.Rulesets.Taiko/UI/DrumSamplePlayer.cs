// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<TaikoAction>
    {
        private readonly DrumSampleTriggerSource leftHitSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightHitSampleTriggerSource;
        private readonly DrumSampleTriggerSource leftRimSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightRimSampleTriggerSource;
        private readonly DrumSampleTriggerSource strongHitSampleTriggerSource;
        private readonly DrumSampleTriggerSource strongRimSampleTriggerSource;

        private double lastHitTime;
        private TaikoAction? lastAction;

        public DrumSamplePlayer(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new Drawable[]
            {
                leftHitSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightHitSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Right),
                leftRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer, SampleBalance.Right),
                strongHitSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                strongRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer)
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
                    triggerSource = strong ? strongHitSampleTriggerSource : leftHitSampleTriggerSource;
                    break;

                case TaikoAction.RightCentre:
                    hitType = HitType.Centre;
                    triggerSource = strong ? strongHitSampleTriggerSource : rightHitSampleTriggerSource;
                    break;

                case TaikoAction.LeftRim:
                    hitType = HitType.Rim;
                    triggerSource = strong ? strongRimSampleTriggerSource : leftRimSampleTriggerSource;
                    break;

                case TaikoAction.RightRim:
                    hitType = HitType.Rim;
                    triggerSource = strong ? strongRimSampleTriggerSource : rightRimSampleTriggerSource;
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
            const double big_hit_window = 30;

            if (lastAction == null)
                return false;

            if (timeBetweenActions > big_hit_window)
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
            leftHitSampleTriggerSource.FlushPlayback();
            rightHitSampleTriggerSource.FlushPlayback();
            strongHitSampleTriggerSource.FlushPlayback();
        }

        private void flushRimTriggerSources()
        {
            leftRimSampleTriggerSource.FlushPlayback();
            rightRimSampleTriggerSource.FlushPlayback();
            strongRimSampleTriggerSource.FlushPlayback();
        }

        public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
        {
        }
    }
}
