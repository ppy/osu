// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Screens.Backgrounds;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public abstract class ScreenWithBeatmapBackground : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);
        public override bool AllowBeatmapRulesetChange => false;

        protected float Duration => 800;
        protected float Opacity => 1 - (float)DimLevel;

        #region User Settings

        protected Bindable<double> DimLevel;
        protected Bindable<double> BlurLevel;
        protected Bindable<bool> ShowStoryboard;
        protected Bindable<bool> MouseWheelDisabled;
        protected Bindable<double> UserAudioOffset;

        #endregion

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            BlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            MouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            UserAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
        }

        protected void ConfigureBackgroundUpdate()
        {
            DimLevel.ValueChanged += _ => UpdateBackgroundElements();
            BlurLevel.ValueChanged += _ => UpdateBackgroundElements();
            ShowStoryboard.ValueChanged += _ => UpdateBackgroundElements();
            UpdateBackgroundElements();
        }

        protected virtual void UpdateBackgroundElements()
        {
            if (!IsCurrentScreen) return;

            (Background as BackgroundScreenBeatmap)?.BlurTo(new Vector2((float)BlurLevel.Value * 25), Duration, Easing.OutQuint);
        }
    }
}
