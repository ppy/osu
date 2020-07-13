using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Play;

namespace osu.Game.Screens.Purcashe
{
    public class PurcasheBasicScreen : ScreenWithBeatmapBackground
    {
        private MfBgTriangles triangles;
        private BindableBool EnableTriangles = new BindableBool();
        private BindableBool EnableBeatmapBG = new BindableBool();
        protected Drawable[] Content;
        private Container content;

        [Resolved]
        private IBindable<WorkingBeatmap> b { get; set; }

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
            b.BindValueChanged(v => updateComponentFromBeatmap(v.NewValue));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            EnableTriangles.BindValueChanged(UpdateTriangles, true);
            EnableBeatmapBG.BindValueChanged(UpdateBG);

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

        private void UpdateBG(ValueChangedEvent<bool> v)
        {
            switch(v.NewValue)
            {
                case true:
                    updateComponentFromBeatmap(b.Value);
                    break;

                case false:
                    break;
            }
        }

        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            if ( !EnableBeatmapBG.Value ) return;

            ((BackgroundScreenBeatmap)Background).Beatmap = beatmap;
        }
    }
}