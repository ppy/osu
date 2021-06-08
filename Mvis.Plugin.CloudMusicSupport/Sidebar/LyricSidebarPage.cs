using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Sidebar.Screens;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.Misc;
using osu.Game.Screens.Mvis.Plugins;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class LyricSidebarSectionContainer : PluginSidebarPage
    {
        private BeatmapCover cover;
        private LoadingSpinner loading;
        private OsuSpriteText idText;

        public LyricSidebarSectionContainer(MvisPlugin plugin)
            : base(plugin)
        {
            Icon = FontAwesome.Solid.Music;
        }

        public override PluginBottomBarButton CreateBottomBarButton()
            => new LyricBottombarButton(this);

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        [Resolved]
        private Storage storage { get; set; }

        [Resolved]
        private DialogOverlay dialog { get; set; }

        [Resolved(canBeNull: true)]
        private GameHost host { get; set; }

        private LyricPlugin plugin => (LyricPlugin)Plugin;

        //旧版(2021.424.0 -> 版本2)兼容
        private DependencyContainer dependencies;
        public int BeatmapSetId;

        private ScreenStack screenStack;

        private FillFlowContainer buttonsFillFlow;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider provider)
        {
            LyricConfigManager config;

            if (dependencies.Get<MvisPluginManager>().PluginVersion < 3)
            {
                dependencies.Cache(this);
                dependencies.Cache(Plugin);
                dependencies.Cache(config = (LyricConfigManager)Dependencies.Get<MvisPluginManager>().GetConfigManager(Plugin));
            }
            else
                config = (LyricConfigManager)Config;

            dependencies.Cache(this);

            Children = new Drawable[]
            {
                screenStack = new SidebarScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 125,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        cover = new BeatmapCover(mvisScreen.Beatmap.Value)
                        {
                            TimeBeforeWrapperLoad = 0
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.7f,
                                    Colour = Color4.Black.Opacity(0.6f)
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.3f,
                                    Colour = ColourInfo.GradientHorizontal(
                                        Color4.Black.Opacity(0.6f),
                                        Color4.Black.Opacity(0))
                                }
                            }
                        },
                        new TrackTimeIndicator(),
                        buttonsFillFlow = new FillFlowContainer
                        {
                            Margin = new MarginPadding(5),
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Width = 0.5f,
                            Spacing = new Vector2(5),
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            AutoSizeDuration = 200,
                            AutoSizeEasing = Easing.OutQuint,
                        },
                        idText = new OsuSpriteText
                        {
                            Margin = new MarginPadding(15),
                            Font = OsuFont.GetFont(size: 20)
                        },
                        new SettingsSlider<double>
                        {
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                            Current = config.GetBindable<double>(LyricSettings.LyricOffset),
                            LabelText = "全局歌词偏移",
                            RelativeSizeAxes = Axes.None,
                            Width = 200 + 25,
                            Padding = new MarginPadding { Right = 10 }
                        }
                    }
                },
                loading = new LoadingLayer(true)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                }
            };

            plugin.CurrentStatus.BindValueChanged(v =>
            {
                switch (v.NewValue)
                {
                    case LyricPlugin.Status.Finish:
                    case LyricPlugin.Status.Failed:
                        loading.Hide();
                        break;

                    default:
                        loading.Show();
                        break;
                }
            }, true);

            screenStack.ScreenPushed += onScreenChanged;
            screenStack.ScreenExited += onScreenChanged;
        }

        private readonly IconButton backButton = new IconButton
        {
            Icon = FontAwesome.Solid.ArrowLeft,
            Size = new Vector2(45)
        };

        private void onScreenChanged(IScreen lastscreen, IScreen newscreen)
        {
            if (newscreen is SidebarScreen screen)
            {
                buttonsFillFlow.Clear(false);
                buttonsFillFlow.Children = screen.Entries;

                if (!(screen is LyricViewScreen))
                {
                    buttonsFillFlow.Add(backButton);
                    backButton.Action = screenStack.Exit;
                }
            }
        }

        protected override void LoadComplete()
        {
            mvisScreen.OnBeatmapChanged(refreshBeatmap, this);
            refreshBeatmap(mvisScreen.Beatmap.Value);

            screenStack.Push(new LyricViewScreen());
            base.LoadComplete();
        }

        private void refreshBeatmap(WorkingBeatmap working)
        {
            BeatmapSetId = working.BeatmapSetInfo.ID;
            idText.Text = $"ID: {BeatmapSetId}";
            cover.UpdateBackground(working);
        }
    }
}
