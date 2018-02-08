using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;
using osu.Game.Storyboards.Drawables;
using OpenTK;

namespace osu.Game.Screens.Play
{
    public abstract class PlayerBase : OsuScreen
    {
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);
        public override bool AllowBeatmapRulesetChange => false;

        #region User Settings

        protected Bindable<double> DimLevel;
        protected Bindable<double> BlurLevel;
        protected Bindable<bool> ShowStoryboard;
        protected Bindable<bool> MouseWheelDisabled;
        protected Bindable<double> UserAudioOffset;

        protected SampleChannel SampleRestart;

        #endregion

        protected DrawableStoryboard Storyboard;
        protected Container StoryboardContainer;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            DimLevel = config.GetBindable<double>(OsuSetting.DimLevel);
            BlurLevel = config.GetBindable<double>(OsuSetting.BlurLevel);
            ShowStoryboard = config.GetBindable<bool>(OsuSetting.ShowStoryboard);

            MouseWheelDisabled = config.GetBindable<bool>(OsuSetting.MouseDisableWheel);

            SampleRestart = audio.Sample.Get(@"Gameplay/restart");

            UserAudioOffset = config.GetBindable<double>(OsuSetting.AudioOffset);
        }

        protected void ConfigureBackgroundUpdate()
        {
            DimLevel.ValueChanged += _ => UpdateBackgroundElements();
            BlurLevel.ValueChanged += _ => UpdateBackgroundElements();
            ShowStoryboard.ValueChanged += _ => UpdateBackgroundElements();
            UpdateBackgroundElements();
        }

        protected void UpdateBackgroundElements()
        {
            if (!IsCurrentScreen) return;

            const float duration = 800;

            var opacity = 1 - (float)DimLevel;

            if (ShowStoryboard && Storyboard == null)
                InitializeStoryboard(true);

            var beatmap = Beatmap.Value;
            var storyboardVisible = ShowStoryboard && beatmap.Storyboard.HasDrawable;

            StoryboardContainer?
                .FadeColour(OsuColour.Gray(opacity), duration, Easing.OutQuint)
                .FadeTo(storyboardVisible && opacity > 0 ? 1 : 0, duration, Easing.OutQuint);

            (Background as BackgroundScreenBeatmap)?.BlurTo(new Vector2((float)BlurLevel.Value * 25), duration, Easing.OutQuint);
            Background?.FadeTo(!storyboardVisible || beatmap.Background == null ? opacity : 0, duration, Easing.OutQuint);
        }

        protected void InitializeStoryboard(bool asyncLoad)
        {
            if (StoryboardContainer == null)
                return;

            var beatmap = Beatmap.Value;

            Storyboard = beatmap.Storyboard.CreateDrawable(Beatmap.Value);
            Storyboard.Masking = true;

            if (asyncLoad)
                LoadComponentAsync(Storyboard, StoryboardContainer.Add);
            else
                StoryboardContainer.Add(Storyboard);
        }
    }
}
