using System.Collections.Generic;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Mvis;
using osu.Game.Screens.Mvis.Misc;
using osu.Game.Screens.Mvis.Plugins;
using osuTK;
using osuTK.Graphics;

namespace Mvis.Plugin.CloudMusicSupport.Sidebar
{
    public class LyricSidebarPage : PluginSidebarPage
    {
        private BeatmapCover cover;
        private FillFlowContainer<LyricInfoContainer> lyricFlow;
        private LoadingSpinner loading;
        private IconButton saveButton;
        private OsuSpriteText idText;

        public LyricSidebarPage(MvisPlugin plugin, float resizeWidth)
            : base(plugin, resizeWidth)
        {
        }

        public override PluginBottomBarButton CreateBottomBarButton()
            => new LyricBottombarButton(this);

        [Resolved]
        private MvisScreen mvisScreen { get; set; }

        [Resolved]
        private Storage storage { get; set; }

        private LyricPlugin plugin => (LyricPlugin)Plugin;

        //旧版(2021.424.0 -> 版本2)兼容
        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        [BackgroundDependencyLoader]
        private void load(CustomColourProvider provider)
        {
            if (dependencies.Get<MvisPluginManager>().PluginVersion < 3)
            {
                dependencies.Cache(this);
                dependencies.Cache(Plugin);
                dependencies.Cache(Dependencies.Get<MvisPluginManager>().GetConfigManager(Plugin));
            }

            Children = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                Height = 100,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    cover = new BeatmapCover(mvisScreen.Beatmap.Value),
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
                                    new FillFlowContainer
                                    {
                                        Margin = new MarginPadding(5),
                                        AutoSizeAxes = Axes.Both,
                                        Spacing = new Vector2(5),
                                        Direction = FillDirection.Horizontal,
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        AutoSizeDuration = 200,
                                        AutoSizeEasing = Easing.OutQuint,
                                        Children = new Drawable[]
                                        {
                                            saveButton = new IconButton
                                            {
                                                Icon = FontAwesome.Solid.Save,
                                                Size = new Vector2(45),
                                                TooltipText = "保存为lrc",
                                                Action = plugin.WriteLyricToDisk
                                            },
                                            new IconButton
                                            {
                                                Icon = FontAwesome.Solid.Circle,
                                                Size = new Vector2(45),
                                                TooltipText = "重新获取歌词",
                                                Action = plugin.RefreshLyric
                                            }
                                        }
                                    },
                                    idText = new OsuSpriteText
                                    {
                                        Margin = new MarginPadding(15),
                                        Font = OsuFont.GetFont(size: 20)
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new OsuScrollContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = lyricFlow = new FillFlowContainer<LyricInfoContainer>
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Spacing = new Vector2(5),
                                    Padding = new MarginPadding(5)
                                }
                            }
                        }
                    }
                },
                new TrackTimeIndicator(),
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
                        refreshLrcInfo(plugin.Lyrics);
                        loading.Hide();
                        saveButton.FadeIn(300, Easing.OutQuint);
                        break;

                    case LyricPlugin.Status.Failed:
                        loading.Hide();
                        saveButton.FadeOut(300, Easing.OutQuint);
                        break;

                    default:
                        lyricFlow.Clear();
                        loading.Show();
                        break;
                }
            }, true);
        }

        protected override void LoadComplete()
        {
            mvisScreen.OnBeatmapChanged += refreshBeatmap;
            refreshBeatmap(mvisScreen.Beatmap.Value);

            base.LoadComplete();
        }

        private void refreshLrcInfo(List<Lyric> lyrics)
        {
            lyricFlow.Clear();

            foreach (var t in lyrics)
            {
                lyricFlow.Add(new LyricInfoContainer(t));
            }
        }

        private void refreshBeatmap(WorkingBeatmap working)
        {
            idText.Text = working.BeatmapSetInfo.ID.ToString();
            cover.UpdateBackground(working);
        }
    }
}
