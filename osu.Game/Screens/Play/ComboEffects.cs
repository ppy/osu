// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Containers;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;

namespace osu.Game.Screens.Play
{
    public class ComboEffects : CompositeDrawable
    {
        private readonly ScoreProcessor processor;

        private SkinnableSound comboBreakSample;

        private Bindable<bool> alwaysPlay;
        private bool firstTime = true;

        public ComboEffects(ScoreProcessor processor)
        {
            this.processor = processor;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            InternalChild = comboBreakSample = new SkinnableSound(new SampleInfo("Gameplay/combobreak"));
            alwaysPlay = config.GetBindable<bool>(OsuSetting.AlwaysPlayFirstComboBreak);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            processor.Combo.BindValueChanged(onComboChange);
        }

        private void onComboChange(ValueChangedEvent<int> combo)
        {
            if (combo.NewValue == 0 && (combo.OldValue > 20 || (alwaysPlay.Value && firstTime)))
            {
                comboBreakSample?.Play();
                firstTime = false;
            }
        }
    }
}
