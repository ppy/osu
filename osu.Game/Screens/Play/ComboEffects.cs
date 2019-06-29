// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public class ComboEffects : CompositeDrawable
    {
        private SampleChannel sampleComboBreak;

        public ComboEffects(ScoreProcessor processor)
        {
            processor.Combo.BindValueChanged(onComboChange);
        }

        private void onComboChange(ValueChangedEvent<int> combo)
        {
            if (combo.NewValue == 0 && combo.OldValue > 20)
                sampleComboBreak?.Play();
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin, AudioManager audio)
        {
            sampleComboBreak = skin.GetSample(@"Gameplay/combobreak") ?? audio.Samples.Get(@"Gameplay/combobreak");
        }
    }
}