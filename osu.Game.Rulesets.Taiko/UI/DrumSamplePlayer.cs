// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;

namespace osu.Game.Rulesets.Taiko.UI
{
    internal partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<TaikoAction>
    {
        private DrumSampleTriggerSource leftCentreTrigger = null!;
        private DrumSampleTriggerSource rightCentreTrigger = null!;
        private DrumSampleTriggerSource leftRimTrigger = null!;
        private DrumSampleTriggerSource rightRimTrigger = null!;
        private DrumSampleTriggerSource strongCentreTrigger = null!;
        private DrumSampleTriggerSource strongRimTrigger = null!;

        private double lastHitTime;
        private TaikoAction? lastAction;

        [BackgroundDependencyLoader]
        private void load(Playfield playfield)
        {
            var hitObjectContainer = playfield.HitObjectContainer;
            InternalChildren = new Drawable[]
            {
                leftCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                leftRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Left),
                rightRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Right),
                strongCentreTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre),
                strongRimTrigger = CreateTriggerSource(hitObjectContainer, SampleBalance.Centre)
            };
        }

        protected virtual DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer, SampleBalance balance)
            => new DrumSampleTriggerSource(hitObjectContainer);

        public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            if (Clock is IGameplayClock { IsRewinding: true })
                return false;

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

            if (strong)
            {
                switch (hitType)
                {
                    case HitType.Centre:
                        flushCenterTriggerSources();
                        break;

                    case HitType.Rim:
                        flushRimTriggerSources();
                        break;
                }
            }

            Play(triggerSource, hitType, strong);

            lastHitTime = Time.Current;
            lastAction = e.Action;

            return false;
        }

        protected virtual void Play(DrumSampleTriggerSource triggerSource, HitType hitType, bool strong) =>
            triggerSource.Play(hitType, strong);

        private bool checkStrongValidity(TaikoAction newAction, TaikoAction? lastAction, double timeBetweenActions)
        {
            if (lastAction == null)
                return false;

            if (timeBetweenActions < 0 || timeBetweenActions > DrawableHit.StrongNestedHit.SECOND_HIT_WINDOW)
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
