// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.UI
{
    public partial class DrumSamplePlayer : CompositeDrawable, IKeyBindingHandler<TaikoAction>
    {
        private DrumSampleTriggerSource leftRimSampleTriggerSource = null!;
        private DrumSampleTriggerSource leftCentreSampleTriggerSource = null!;
        private DrumSampleTriggerSource rightCentreSampleTriggerSource = null!;
        private DrumSampleTriggerSource rightRimSampleTriggerSource = null!;

        [BackgroundDependencyLoader]
        private void load(DrawableRuleset drawableRuleset)
        {
            var hitObjectContainer = drawableRuleset.Playfield.HitObjectContainer;

            InternalChildren = new Drawable[]
            {
                leftRimSampleTriggerSource = CreateTriggerSource(hitObjectContainer),
                leftCentreSampleTriggerSource = CreateTriggerSource(hitObjectContainer),
                rightCentreSampleTriggerSource = CreateTriggerSource(hitObjectContainer),
                rightRimSampleTriggerSource = CreateTriggerSource(hitObjectContainer),
            };
        }

        protected virtual DrumSampleTriggerSource CreateTriggerSource(HitObjectContainer hitObjectContainer)
            => new DrumSampleTriggerSource(hitObjectContainer);

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
