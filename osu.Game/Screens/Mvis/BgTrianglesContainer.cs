using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;

namespace osu.Game.Screens.Mvis
{
    public class BgTrianglesContainer : VisibilityContainer
    {
        private const float TRIANGLES_ALPHA = 0.65f;
        private Bindable<bool> EnableBgTriangles = new Bindable<bool>();
        private Container trianglesContainer;

        [BackgroundDependencyLoader]
        private void load(MfConfigManager config)
        {
            RelativeSizeAxes = Axes.Both;
            Child = trianglesContainer = new Container()
            {
                Alpha = TRIANGLES_ALPHA,
                RelativeSizeAxes = Axes.Both,
                Child = new Triangles()
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    TriangleScale = 5f,
                    EnableBeatSync = true,
                    Colour = OsuColour.Gray(0.2f),
                }
            };
            config.BindWith(MfSetting.MvisEnableBgTriangles, EnableBgTriangles);
        }

        protected override void LoadComplete()
        {
            EnableBgTriangles.BindValueChanged(UpdateVisuals, true);

            base.LoadComplete();
        }

        private void UpdateVisuals(ValueChangedEvent<bool> value)
        {
            switch ( value.NewValue )
            {
                case true:
                    trianglesContainer.FadeTo(TRIANGLES_ALPHA, 250);
                    break;

                case false:
                    trianglesContainer.FadeOut(250);
                    break;
            }
        }

        protected override void PopIn()
        {
            this.FadeIn(250);
        }

        protected override void PopOut()
        {
            this.FadeOut(250);
        }
    }
}