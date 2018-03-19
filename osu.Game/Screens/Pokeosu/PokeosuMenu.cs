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

namespace osu.Game.Screens.Pokeosu
{
    public class PokeosuMenu : OsuScreen, IOnlineComponent
    {
        public override bool ShowOverlaysOnEnter => false;

        public static bool AssetsLoaded = false;

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenDefault();

        private PokeosuBackground background;

        public Container StartScreen;
        private OsuButton startButton;
        private SpriteText stateText;
        private PokeosuProfilePic profilePic;

        public Container IntroScreen;
        private SpriteText hi;
        private SpriteText ready;

        public Container PlayerScreen;
        private Container badgesChart;

        private Container badge1;
        private SpriteText badge1Description;
        private Sprite badge1Sprite;
        /*
        private Container badge2;
        private SpriteText badge2Description;
        private Sprite badge2Sprite;

        private Container badge3;
        private SpriteText badge3Description;
        private Sprite badge3Sprite;
        */
        private OsuButton nextButton;

        private DependencyContainer dependencies;
        protected override IReadOnlyDependencyContainer CreateLocalDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateLocalDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, Storage storage, APIAccess api)
        {
            api.Register(this);

            User user = api.LocalUser.Value;

            TextureStore flagStore = new TextureStore();
            // Local flag store
            flagStore.AddStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(new StorageBackedResourceStore(storage), "Drawings")));
            // Default texture store
            flagStore.AddStore(textures);

            dependencies.Cache(flagStore);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(77, 77, 77, 255)
                },
                new Sprite
                {
                    FillMode = FillMode.Fill,
                    Texture = textures.Get(@"Backgrounds/Drawings/background.png")
                },
                background = new PokeosuBackground
                {
                    Scale = new Vector2(1.005f),
                    Alpha = 0.8f,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Triangles
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColourDark = new Color4(0.25f, 0.25f, 0.25f, 1),
                            ColourLight = new Color4(0.5f, 0.5f, 0.5f, 1),
                            TriangleScale = 4,
                        }
                    }
                },
                StartScreen = new Container
                {
                    Alpha = 1,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Position = new Vector2(0 , -200),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Pokemosu",
                            TextSize = 32,
                            Colour = Color4.Yellow,
                        },
                        new SpriteText
                        {
                            Position = new Vector2(0),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "a tournement based on not just one gamemode",
                            TextSize = 20,
                            Colour = Color4.Yellow,
                        },
                        stateText = new SpriteText
                        {
                            Alpha = 0,
                            Position = new Vector2(0 , 400),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "null",
                            TextSize = 16,
                            Colour = Color4.Red,
                        },
                        new Container
                        {
                            Position = new Vector2(0 , 60),
                            Size = new Vector2(220 , 40),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            CornerRadius = 4,
                            Children = new Drawable[]
                            {
                                startButton = new OsuButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = "Start your journey",
                                    Action = hiScreen,
                                }
                            }
                        },
                        profilePic = new PokeosuProfilePic
                        {
                            Position = new Vector2(0 , -100),
                            Size = new Vector2(140),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                },
                IntroScreen = new Container
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Children = new Drawable[]
                    {
                        hi = new SpriteText
                        {
                            Alpha = 1,
                            Position = new Vector2(0),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Hi",
                            TextSize = 80,
                            Colour = Color4.Yellow,
                        },
                        ready = new SpriteText
                        {
                            Alpha = 0,
                            Position = new Vector2(0),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Welcome to Pokeosu!",
                            TextSize = 80,
                            Colour = Color4.Yellow,
                        },
                        new Container
                        {
                            Position = new Vector2(0 , 80),
                            Size = new Vector2(100 , 40),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            CornerRadius = 4,
                            Children = new Drawable[]
                            {
                                nextButton = new OsuButton
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Text = "Next",
                                    Action = welcomeScreen,
                                }
                            }
                        }
                    }
                },
                PlayerScreen = new Container
                {
                    Alpha = 0,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    Children = new Drawable[]
                    {
                        /*
                        new AsyncLoadWrapper(new CoverBackgroundSprite(user)
                        {
                            Depth = 11,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            FillMode = FillMode.Fill,
                        }),
                        */
                        new DrawableFlag(user.Country)
                        {
                            Position = new Vector2(10 , -10),
                            Size = new Vector2(3 , 2),
                            Scale = new Vector2(20),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.12f,
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,

                            Children = new Drawable[]
                            {
                                new Triangles
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColourDark = Color4.DarkGoldenrod,
                                    ColourLight = Color4.LightYellow,
                                    TriangleScale = 2,
                                },
                                new OsuSpriteText
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    Text = "Pokeosu!",
                                    TextSize = 22f,
                                    Font = "Exo2.0-Bold"
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, 35f),

                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
                                    {
                                        new OsuButton
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Find Match",
                                        },
                                        new OsuButton
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Start Match",
                                        },
                                        new OsuButton
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Open Discord",
                                        },
                                    }
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.BottomCentre,
                                    Origin = Anchor.BottomCentre,

                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.75f,

                                    Position = new Vector2(0, -5f),

                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(0, 5f),

                                    Children = new Drawable[]
                                    {
                                        new OsuButton
                                        {
                                            RelativeSizeAxes = Axes.X,

                                            Text = "Leave Pokeosu :(",
                                        }
                                    }
                                }
                            }
                        },
                        profilePic = new PokeosuProfilePic
                        {
                            Position = new Vector2(10),
                            Size = new Vector2(140),
                            Anchor = Anchor.TopLeft,
                            Origin = Anchor.TopLeft,
                        },
                        badgesChart = new Container
                        {
                            Masking = true,
                            Depth = 0,
                            Position = new Vector2(0),
                            Size = new Vector2(600, 400),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            BorderColour = Color4.White,
                            BorderThickness = 10,
                            CornerRadius = 20,
                            Children = new Drawable[]
                            {
                                badge1 = new Container
                                {
                                    Depth = -3,
                                    Position = new Vector2(0),
                                    Size = new Vector2(40),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Colour = Color4.Brown,
                                    Children = new Drawable[]
                                    {
                                        badge1Sprite = new Sprite
                                        {
                                            Position = new Vector2(0 , -20),
                                            Size = new Vector2(40),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Alpha = 1,
                                        },
                                        badge1Description = new SpriteText
                                        {
                                            Position = new Vector2(0),
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Text = "Standard Tier 1",
                                            TextSize = 20,
                                        }
                                    }
                                },/*
                                badge2 = new Container
                                {
                                    Position = new Vector2(0),
                                    Size = new Vector2(40),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        badge2Sprite = new Sprite
                                        {

                                        },
                                        badge2Description = new SpriteText
                                        {

                                        }
                                    }
                                },
                                badge3 = new Container
                                {
                                    Position = new Vector2(0),
                                    Size = new Vector2(40),
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Children = new Drawable[]
                                    {
                                        badge3Sprite = new Sprite
                                        {

                                        },
                                        badge3Description = new SpriteText
                                        {

                                        }
                                    }
                                },*/
                                new SpriteText
                                {
                                    Depth = -1,
                                    Position = new Vector2 (0 , 6),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Text = "Badges",
                                    Colour = Color4.White,
                                    TextSize = 32,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = ColourInfo.GradientVertical(Color4.DarkBlue , Color4.Blue),
                                },
                                new Container
                                {
                                    Depth = 0,
                                    Alpha = 0.5f,
                                    Masking = true,
                                    RelativeSizeAxes = Axes.Both,
                                    CornerRadius = 20,
                                    Children = new Drawable[]
                                    {
                                        new Triangles
                                        {
                                            TriangleScale = 2,
                                            RelativeSizeAxes = Axes.Both,
                                            ColourDark = Color4.DarkBlue,
                                            ColourLight = Color4.LightBlue,
                                        }
                                    }
                                },
                            },
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Radius = 10,
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Cyan.Opacity(0.5f),
                            },
                        },
                    }
                }
            };

            //badge1Sprite.Texture = PokeosuTextures.Get("pokeosuIcon");
            //badge2Sprite.Texture = PokeosuTextures.Get("pokeosuIcon");
            //badge3Sprite.Texture = PokeosuTextures.Get("pokeosuIcon");
        }
        
        private class CoverBackgroundSprite : Sprite
        {
            private readonly User user;

            public CoverBackgroundSprite(User user)
            {
                this.user = user;
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                if (!string.IsNullOrEmpty(user.CoverUrl))
                    Texture = textures.Get(user.CoverUrl);
            }
        }
        
        public void APIStateChanged(APIAccess api, APIState state)
        {
            switch (state)
            {
                default:
                    startButton.Text = "Start your journey";
                    break;
                case APIState.Online:
                    startButton.Text = "Start your journey, " + api.LocalUser.Value.Username;
                    break;
            }
        }
        /*
        public void PokeosuAPIStateChanged()
        {
            switch (state)
            {
                case PokeosuAPIState.Connecting:
                    stateText.Text = "Connecting. . .";
                    stateText.Alpha = 0;
                    break;
                case PokeosuAPIState.Online:
                    stateText.Text = "We have connected!";
                    stateText.FadeOut(1000 , EasingTypes.InSine);
                    break;
            }
        }
        */
        private void testMatch()
        {
            //Lol multiplayer has to be made first

            //string matchID = "Pokeosu Test Match 34Gj7";
            //string matchPASSWORD = "34Gj7"
        }

        private void testLink()
        {
            //System.Diagnostics.Process.Start("https://discord.gg/5U6QDEA");
        }

        private void hiScreen()
        {
            StartScreen.FadeOutFromOne(1000, Easing.InOutQuad);
            IntroScreen.FadeInFromZero(1000, Easing.InOutQuad);
        }
        private void welcomeScreen()
        {
            hi.FadeOutFromOne(500, Easing.InOutQuad);
            ready.FadeInFromZero(500, Easing.InOutQuad);
            nextButton.Action = playerPage;
        }
        private void playerPage()
        {
            IntroScreen.FadeOutFromOne(1000, Easing.InOutQuad);
            PlayerScreen.FadeInFromZero(1000, Easing.InOutQuad);
        }
    }
}
