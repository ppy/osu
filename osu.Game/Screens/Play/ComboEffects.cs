// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public class ComboEffects : CompositeDrawable
    {
        private readonly ScoreProcessor processor;

        private SkinnableSound comboBreakSample;

        public ComboEffects(ScoreProcessor processor)
        {
            this.processor = processor;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = comboBreakSample = new SkinnableSound(new SampleInfo("combobreak"));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            processor.Combo.BindValueChanged(onComboChange, true);
        }

        private void onComboChange(ValueChangedEvent<int> combo)
        {
            if (combo.NewValue == 0 && combo.OldValue > 20)
                comboBreakSample?.Play();
        }
    }
}
