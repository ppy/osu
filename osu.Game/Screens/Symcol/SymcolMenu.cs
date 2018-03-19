using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Symcol.Pieces;
using osu.Game.Screens.Symcol.Screens;
using osu.Framework.Configuration;
using osu.Game.Screens.Symcol.Screens.Shawdooow;
using osu.Framework.Allocation;
using osu.Framework.IO.Stores;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Screens.Symcol
{
    public class SymcolMenu : OsuScreen
    {
        private const int animation_duration = 600;
        private readonly Vector2 background_blur = new Vector2(10);

        public static OsuScreen RulesetMultiplayerScreen;

        public static ResourceStore<byte[]> SymcolResources;
        public static TextureStore SymcolTextures;

        public static Bindable<bool> AllowConverts = new Bindable<bool> { Value = true };

        private readonly Bindable<WorkingBeatmap> workingBeatmap = new Bindable<WorkingBeatmap>();

        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        private readonly OsuLogo logo;
        private readonly Container<SymcolButton> buttonsContainer;

        public static void LoadSymcolAssets()
        {
            SymcolResources = new ResourceStore<byte[]>();
            SymcolResources.AddStore(new NamespacedResourceStore<byte[]>(new DllResourceStore("osu.Game.Symcol.Resources.dll"), ("Assets")));
            SymcolResources.AddStore(new DllResourceStore("osu.Game.Symcol.Resources.dll"));
            SymcolTextures = new TextureStore(new RawTextureLoaderStore(new NamespacedResourceStore<byte[]>(SymcolResources, @"Textures")));
            SymcolTextures.AddStore(new RawTextureLoaderStore(new OnlineStore()));
        }

        public SymcolMenu()
        {
            Children = new Drawable[]
            {
                new MenuSideFlashes(),
                buttonsContainer = new Container<SymcolButton>
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        /*
                        new SymcolButton
                        {
                            ButtonName = "osu!Talk",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = new Color4(33 , 58 , 79 , 255),
                            ButtonColorBottom = new Color4(17 , 31 , 42 , 255),
                            ButtonSize = 60,
                            Action = delegate { Push(new OsuTalkMenu()); },
                            ButtonPosition = new Vector2(250 , 175),
                        },
                        */
                        new SymcolButton
                        {
                            ButtonName = "Lazer",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.Black,
                            ButtonColorBottom = Color4.Yellow,
                            ButtonSize = 90,
                            Action = delegate { Push(new ShawdooowLazerLiveWallpaper()); },
                            ButtonPosition = new Vector2(170 , 190),
                        },
                        /*
                        new SymcolButton
                        {
                            ButtonName = "Offset",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.Black,
                            ButtonColorBottom = Color4.White,
                            ButtonSize = 90,
                            Action = delegate { Push(new SymcolOffsetTicker()); },
                            ButtonPosition = new Vector2(-10 , -190),
                        },
                        new SymcolButton
                        {
                            ButtonName = "Pokeosu",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.DarkOrange,
                            ButtonColorBottom = Color4.Orange,
                            ButtonSize = 75,
                            Action = delegate { Push(new PokeosuMenu()); },
                            ButtonPosition = new Vector2(200 , 100),
                        },
                        */
                        new SymcolButton
                        {
                            ButtonName = "Map Mixer",
                            ButtonFontSizeMultiplier = 0.8f,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.Purple,
                            ButtonColorBottom = Color4.HotPink,
                            ButtonSize = 120,
                            Action = delegate { Push(new SymcolMapMixer()); },
                            ButtonPosition = new Vector2(-200 , -150),
                        },
                        /*
                        new SymcolButton
                        {
                            ButtonName = "Play",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.DarkGreen,
                            ButtonColorBottom = Color4.Green,
                            ButtonSize = 130,
                            Action = delegate { Push(new PlaySongSelect()); },
                            ButtonPosition = new Vector2(300 , -20),
                        },
                        */
                        new SymcolButton
                        {
                            ButtonName = "Multi",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.Blue,
                            ButtonColorBottom = Color4.Red,
                            ButtonSize = 120,
                            Action = delegate { Push(RulesetMultiplayerScreen); },
                            ButtonPosition = new Vector2(180 , -100),
                        },
                        /*
                        new SymcolButton
                        {
                            ButtonName = "Edit",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.DarkGoldenrod,
                            ButtonColorBottom = Color4.Gold,
                            ButtonSize = 90,
                            Action = delegate { Push(new Editor()); },
                            ButtonPosition = new Vector2(250 , -150),
                        },*/
                        new SymcolButton
                        {
                            ButtonName = "Tests",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.DarkCyan,
                            ButtonColorBottom = Color4.Cyan,
                            ButtonSize = 100,
                            Action = delegate { Push(new SymcolTestScreen()); },
                            ButtonPosition = new Vector2(-150 , 200),
                        },
                        new SymcolButton
                        {
                            ButtonName = "Back",
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre,
                            ButtonColorTop = Color4.DarkRed,
                            ButtonColorBottom = Color4.Red,
                            ButtonSize = 80,
                            Action = Exit,
                            ButtonPosition = new Vector2(-350 , 300),
                        },
                    },
                },
                logo = new OsuLogo
                {
                    Scale = new Vector2(1.25f),
                    Action = () => open(logo),
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game)
        {
            workingBeatmap.BindTo(game.Beatmap);
            workingBeatmap.ValueChanged += changeBackground;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            workingBeatmap.TriggerChange();
        }

        private bool open(Container container)
        {
            logo.Action = () => close(container);
            container.ScaleTo(new Vector2(0.5f), animation_duration, Easing.InOutBack);

            foreach(var button in buttonsContainer)
                button.MoveTo(button.ButtonPosition, animation_duration, Easing.InOutBack);
            return true;
        }

        private bool close(Container container)
        {
            logo.Action = () => open(container);
            container.ScaleTo(new Vector2(1.25f), animation_duration, Easing.InOutBack);

            foreach (var button in buttonsContainer)
                button.MoveTo(Vector2.Zero, animation_duration, Easing.InOutBack);
            return true;
        }

        private void changeBackground(WorkingBeatmap beatmap)
        {
            var backgroundModeBeatmap = Background as BackgroundScreenBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(background_blur, 1500);
                backgroundModeBeatmap.FadeTo(1, 250);
            }
        }

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            Content.FadeInFromZero(250);
        }
        
        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);
            Content.FadeIn(250);
            Content.ScaleTo(1, 250, Easing.OutSine);
        }

        protected override void OnSuspending(Screen next)
        {
            base.OnSuspending(next);
            Content.ScaleTo(1.1f, 250, Easing.InSine);
            Content.FadeOut(250);
        }

        protected override bool OnExiting(Screen next)
        {
            Content.FadeOut(100);
            return base.OnExiting(next);
        }
    }
}
