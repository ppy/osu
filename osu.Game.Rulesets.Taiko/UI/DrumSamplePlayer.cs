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
        private readonly DrumSampleTriggerSource leftCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource leftRimSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightRimSampleTriggerSource;
        private readonly DrumSampleTriggerSource strongCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource strongRimSampleTriggerSource;

        private double lastHitTime;
        private const double big_hit_window = 30;
        private TaikoAction? lastAction;

        public DrumSamplePlayer(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new Drawable[]
            {
                leftCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer) { Balance = { Value = DrumSampleTriggerSource.SampleBalance.L } },
                rightCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer) { Balance = { Value = DrumSampleTriggerSource.SampleBalance.R } },
                leftRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer) { Balance = { Value = DrumSampleTriggerSource.SampleBalance.L } },
                rightRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer) { Balance = { Value = DrumSampleTriggerSource.SampleBalance.R } },
                strongCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                strongRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer)
            };
        }

        public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            bool makeStrong;
            bool isDoubleHit = Time.Current < lastHitTime + big_hit_window;
            HitType hitType;

            DrumSampleTriggerSource triggerSource;

            switch (e.Action)
            {
                case TaikoAction.LeftCentre:
                    makeStrong = isDoubleHit && lastAction == TaikoAction.RightCentre;
                    hitType = makeStrong ? HitType.StrongCentre : HitType.Centre;
                    triggerSource = makeStrong ? strongCentreSampleTriggerSource : leftCentreSampleTriggerSource;
                    break;

                case TaikoAction.RightCentre:
                    makeStrong = isDoubleHit && lastAction == TaikoAction.LeftCentre;
                    hitType = makeStrong ? HitType.StrongCentre : HitType.Centre;
                    triggerSource = makeStrong ? strongCentreSampleTriggerSource : rightCentreSampleTriggerSource;
                    break;

                case TaikoAction.LeftRim:
                    makeStrong = isDoubleHit && lastAction == TaikoAction.RightRim;
                    hitType = makeStrong ? HitType.StrongRim : HitType.Rim;
                    triggerSource = makeStrong ? strongRimSampleTriggerSource : leftRimSampleTriggerSource;
                    break;

                case TaikoAction.RightRim:
                    makeStrong = isDoubleHit && lastAction == TaikoAction.LeftRim;
                    hitType = makeStrong ? HitType.StrongRim : HitType.Rim;
                    triggerSource = makeStrong ? strongRimSampleTriggerSource : rightRimSampleTriggerSource;
                    break;

                default:
                    return false;
            }

            if (hitType == HitType.StrongCentre)
                flushCenterTriggerSources();

            if (hitType == HitType.StrongRim)
                flushRimTriggerSources();

            triggerSource.Play(hitType);

            lastHitTime = Time.Current;
            lastAction = e.Action;

            return false;
        }

        private void flushCenterTriggerSources()
        {
            leftCentreSampleTriggerSource.FlushPlayback();
            rightCentreSampleTriggerSource.FlushPlayback();
            strongCentreSampleTriggerSource.FlushPlayback();
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
