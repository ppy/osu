using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Play;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens
{
    public abstract partial class SandboxScreen : ScreenWithBeatmapBackground
    {
        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.BindValueChanged(b => updateComponentFromBeatmap(b.NewValue));
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);
            this.FadeInFromZero(250, Easing.OutQuint);
            updateComponentFromBeatmap(Beatmap.Value);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(250, Easing.OutQuint);
            return base.OnExiting(e);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);
            this.FadeIn(250, Easing.OutQuint);
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            base.OnSuspending(e);
            this.FadeOut(250, Easing.OutQuint);
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            ApplyToBackground(b =>
            {
                b.IgnoreUserSettings.Value = false;
                b.Beatmap = beatmap;
            });
        }
    }
}
