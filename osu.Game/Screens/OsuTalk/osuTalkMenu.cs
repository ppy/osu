using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Platform;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.IO.Stores;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Online.API;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Screens.Pokeosu.PokeosuScreens;
using osu.Framework.Graphics.Colour;
using osu.Game.Users;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Screens.OsuTalk
{
    public class OsuTalkMenu : OsuScreen
    {
        public override bool ShowOverlaysOnEnter => false;

        public static bool AssetsLoaded = false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        private OsuTalkBackground background;

        private Sprite pippi;

        private DependencyContainer dependencies;
        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Children = new Drawable[]
            {
                pippi = new Sprite
                {
                    Depth = -1,
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(@"Backgrounds/Talk/BG_talk.png")
                },
                new Box
                {
                    Depth = 2,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = new Color4(21 , 37 , 51 , 255),
                },
                background = new OsuTalkBackground
                {
                    Depth = 1,
                    Scale = new Vector2(1.005f),
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Triangles
                        {
                            Depth = 0,
                            RelativeSizeAxes = Axes.Both,
                            ColourDark = new Color4(33 , 58 , 79 , 255),
                            ColourLight = new Color4(17 , 31 , 42 , 255),
                            TriangleScale = 4,
                        }
                    }
                },
            };
        }
    }
}
