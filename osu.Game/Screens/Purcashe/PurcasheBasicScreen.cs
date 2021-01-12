using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Purcashe
{
    public class PurcasheBasicScreen : ScreenWithBeatmapBackground
    {
        public override bool HideOverlaysOnEnter => true;
        protected const float ANIM_DURATION = 500;
        private MfBgTriangles triangles;
        private readonly BindableBool enableTriangles = new BindableBool();
        private readonly BindableBool enableBeatmapBg = new BindableBool();
        protected Drawable[] Content;
        private Container content;

        [BackgroundDependencyLoader]
        private void load(MConfigManager config)
        {
            InternalChild = content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    triangles = new MfBgTriangles(0, false, 5)
                    {
                        Depth = float.MaxValue
                    }
                }
            };
            config.BindWith(MSetting.PurcasheBgBeatmap, enableBeatmapBg);
            config.BindWith(MSetting.PurcasheBgTriangles, enableTriangles);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            enableTriangles.BindValueChanged(updateTriangles, true);
            enableBeatmapBg.BindValueChanged(_ => UpdateComponentFromBeatmap(Beatmap.Value));
            Beatmap.BindValueChanged(v => UpdateComponentFromBeatmap(v.NewValue));

            if (Content != null)
                content.AddRange(Content);
        }

        private void updateTriangles(ValueChangedEvent<bool> v)
        {
            switch (v.NewValue)
            {
                case false:
                    triangles.FadeOut(300);
                    break;

                case true:
                    triangles.FadeTo(0.65f, 300);
                    break;
            }
        }

        public override bool OnExiting(IScreen next)
        {
            this.ScaleTo(0.6f, ANIM_DURATION, Easing.OutQuint)
                .FadeOut(ANIM_DURATION, Easing.OutQuint);

            return base.OnExiting(next);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            this.ScaleTo(1f, ANIM_DURATION, Easing.OutQuint)
                .FadeInFromZero(ANIM_DURATION, Easing.OutQuint);

            UpdateComponentFromBeatmap(Beatmap.Value, true);
        }

        //切换至前台
        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            this.ScaleTo(1f, ANIM_DURATION, Easing.OutQuint)
                .FadeInFromZero(ANIM_DURATION, Easing.OutQuint);

            UpdateComponentFromBeatmap(Beatmap.Value, true);
        }

        //切换至后台
        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            this.ScaleTo(0.6f, ANIM_DURATION, Easing.OutQuint)
                .FadeOut(ANIM_DURATION, Easing.OutQuint);
        }

        protected void UpdateComponentFromBeatmap(WorkingBeatmap beatmap, bool blurOnly = false)
        {
            ApplyToBackground(b =>
            {
                if (!enableBeatmapBg.Value)
                {
                    b.BlurAmount.Value = 0;
                    return;
                }

                b.BlurAmount.Value = 20;

                if (blurOnly) return;

                b.Beatmap = beatmap;
            });
        }
    }
}
