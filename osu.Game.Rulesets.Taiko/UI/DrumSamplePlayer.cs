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
    internal class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<TaikoAction>
    {
        private readonly DrumSampleTriggerSource leftRimSampleTriggerSource;
        private readonly DrumSampleTriggerSource leftCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightCentreSampleTriggerSource;
        private readonly DrumSampleTriggerSource rightRimSampleTriggerSource;

        public DrumSamplePlayer(HitObjectContainer hitObjectContainer)
        {
            InternalChildren = new Drawable[]
            {
                leftRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                leftCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                rightCentreSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
                rightRimSampleTriggerSource = new DrumSampleTriggerSource(hitObjectContainer),
            };
        }

        public bool OnPressed(KeyBindingPressEvent<TaikoAction> e)
        {
            switch (e.Action)
            {
                case TaikoAction.LeftRim:
                    leftRimSampleTriggerSource.Play(HitType.Rim);
                    break;

                case TaikoAction.LeftCentre:
                    leftCentreSampleTriggerSource.Play(HitType.Centre);
                    break;

                case TaikoAction.RightCentre:
                    rightCentreSampleTriggerSource.Play(HitType.Centre);
                    break;

                case TaikoAction.RightRim:
                    rightRimSampleTriggerSource.Play(HitType.Rim);
                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<TaikoAction> e)
        {
        }
    }
}
