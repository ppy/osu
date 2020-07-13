using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Purcashe
{
    public class PurcasheBasicScreen : ScreenWithBeatmapBackground
    {
        protected const float anim_duration = 500;
        private MfBgTriangles triangles;
        private BindableBool EnableTriangles = new BindableBool();
        private BindableBool EnableBeatmapBG = new BindableBool();
        protected Drawable[] Content;
        private Container content;


        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
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
            config.BindWith(MfSetting.EasterEggBGBeatmap, EnableBeatmapBG);
            config.BindWith(MfSetting.EasterEggBGTriangle, EnableTriangles);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EnableTriangles.BindValueChanged(UpdateTriangles, true);
            EnableBeatmapBG.BindValueChanged(_ => updateComponentFromBeatmap(Beatmap.Value));
            Beatmap.BindValueChanged(v => updateComponentFromBeatmap(v.NewValue));

            if (Content != null)
                content.AddRange(Content);
        }

        private void UpdateTriangles(ValueChangedEvent<bool> v)
        {
            switch(v.NewValue)
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
            this.ScaleTo(0.6f, anim_duration, Easing.OutQuint)
                .FadeOut(anim_duration, Easing.OutQuint);

            return base.OnExiting(next);
        }

        public override void OnEntering(IScreen last)
        {
            base.OnEntering(last);

            this.ScaleTo(1f, anim_duration, Easing.OutQuint)
                .FadeInFromZero(anim_duration, Easing.OutQuint);

            updateComponentFromBeatmap(Beatmap.Value, true);
        }

        //切换至前台
        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            this.ScaleTo(1f, anim_duration, Easing.OutQuint)
                .FadeInFromZero(anim_duration, Easing.OutQuint);

            updateComponentFromBeatmap(Beatmap.Value, true);
        }

        //切换至后台
        public override void OnSuspending(IScreen next)
        {
            base.OnSuspending(next);

            this.ScaleTo(0.6f, anim_duration, Easing.OutQuint)
                .FadeOut(anim_duration, Easing.OutQuint);
        }


        protected void updateComponentFromBeatmap(WorkingBeatmap beatmap, bool BlurOnly = false)
        {
            if ( !EnableBeatmapBG.Value )
            {
                ((BackgroundScreenBeatmap)Background).BlurAmount.Value = 0;
                return;
            }

            Background.BlurAmount.Value = 20;

            if ( BlurOnly ) return;

            Background.Beatmap = beatmap;
        }
    }
}