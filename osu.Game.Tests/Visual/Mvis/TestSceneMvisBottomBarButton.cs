using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.BottomBar.Buttons;
using osuTK;

namespace osu.Game.Tests.Visual.Mvis
{
    public class TestSceneBottomBarButton : ScreenTestScene
    {
        private DependencyContainer dependencies;
        private BottomBarButton button;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(new CustomColourProvider(0, 0, 1));

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(Color4Extensions.FromHex("#333"), Color4Extensions.FromHex("#777"))
                },
                button = new BottomBarButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(4),
                    Size = new Vector2(60, 30)
                }
            };

            AddStep("Set Icon", () => button.ButtonIcon = FontAwesome.Solid.Ad);
            AddStep("Clear Icon", () => button.ButtonIcon = new IconUsage());
            AddSliderStep("Set Scale", 1f, 5f, 4f, v => button.Scale = new Vector2(v));
        }
    }
}
