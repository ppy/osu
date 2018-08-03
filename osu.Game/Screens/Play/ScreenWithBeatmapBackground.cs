// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Screens.Backgrounds;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public abstract class ScreenWithBeatmapBackground : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap.Value);

        protected new BackgroundScreenBeatmap Background => (BackgroundScreenBeatmap)base.Background;

        public override bool AllowBeatmapRulesetChange => false;

        protected const float BACKGROUND_FADE_DURATION = 800;

        protected float BackgroundOpacity => 1 - (float)DimLevel;

        #region User Settings

        protected Bindable<double> DimLevel;
        protected Bindable<double> BlurLevel;
        protected Bindable<bool> ShowStoryboard;

        #endregion

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            BlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            DimLevel.ValueChanged += _ => UpdateBackgroundElements();
            BlurLevel.ValueChanged += _ => UpdateBackgroundElements();
            ShowStoryboard.ValueChanged += _ => UpdateBackgroundElements();
            InitializeBackgroundElements();
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            InitializeBackgroundElements();
        }

        /// <summary>
        /// Called once on entering screen. By Default, performs a full <see cref="UpdateBackgroundElements"/> call.
        /// </summary>
        protected virtual void InitializeBackgroundElements() => UpdateBackgroundElements();

        /// <summary>
        /// Called wen background elements require updates, usually due to a user changing a setting.
        /// </summary>
        /// <param name="userChange"></param>
        protected virtual void UpdateBackgroundElements()
        {
            if (!IsCurrentScreen) return;

            Background?.FadeTo(BackgroundOpacity, BACKGROUND_FADE_DURATION, Easing.OutQuint);
            Background?.BlurTo(new Vector2((float)BlurLevel.Value * 25), BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }
    }
}
