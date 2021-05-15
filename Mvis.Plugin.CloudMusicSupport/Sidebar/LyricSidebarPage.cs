using System.Collections.Generic;
using System.Linq;
using Mvis.Plugin.CloudMusicSupport.Config;
using Mvis.Plugin.CloudMusicSupport.Misc;
using osu.Framework;
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
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Settings;
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
        private FillFlowContainer<LyricInfoPiece> lyricFlow;
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

        [Resolved]
        private DialogOverlay dialog { get; set; }

        [Resolved(canBeNull: true)]
        private GameHost host { get; set; }

        private LyricPlugin plugin => (LyricPlugin)Plugin;

        //旧版(2021.424.0 -> 版本2)兼容
        private DependencyContainer dependencies;
        private OsuScrollContainer scroll;
        private int beatmapSetId;

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

            FillFlowContainer buttonsFillFlow;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = 125 },
                    Child = scroll = new OsuScrollContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = lyricFlow = new FillFlowContainer<LyricInfoPiece>
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Spacing = new Vector2(5),
                            Padding = new MarginPadding(5)
                        }
                    }
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
                                    Icon = FontAwesome.Solid.Undo,
                                    Size = new Vector2(45),
                                    TooltipText = "刷新",
                                    Action = () => plugin.RefreshLyric()
                                },
                                new IconButton
                                {
                                    Icon = FontAwesome.Solid.CloudDownloadAlt,
                                    Size = new Vector2(45),
                                    TooltipText = "重新获取歌词",
                                    Action = () => dialog.Push
                                    (
                                        new ConfirmDialog("重新获取歌词",
                                            () => plugin.RefreshLyric(true))
                                    )
                                },
                                new IconButton
                                {
                                    Icon = FontAwesome.Solid.AngleDown,
                                    Size = new Vector2(45),
                                    TooltipText = "滚动到当前歌词",
                                    Action = scrollToCurrent
                                }
                            }
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

            if (RuntimeInfo.IsDesktop)
            {
                buttonsFillFlow.Add(new IconButton
                {
                    Icon = FontAwesome.Solid.Code,
                    Size = new Vector2(45),
                    TooltipText = "编辑原始json",
                    Action = () => host?.OpenFileExternally(storage.GetFullPath($"custom/lyrics/beatmap-{beatmapSetId}.json"))
                });
            }

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

        private void scrollToCurrent()
        {
            var pos = lyricFlow.Children.FirstOrDefault(p =>
                p.Value == plugin.Lyrics.FindLast(l => mvisScreen.CurrentTrack.CurrentTime >= l.Time))?.Y ?? 0;

            if (pos + scroll.DrawHeight > lyricFlow.Height)
                scroll.ScrollToEnd();
            else
                scroll.ScrollTo(pos);
        }

        protected override void LoadComplete()
        {
            mvisScreen.OnBeatmapChanged(refreshBeatmap, this);
            refreshBeatmap(mvisScreen.Beatmap.Value);

            base.LoadComplete();
        }

        private void refreshLrcInfo(List<Lyric> lyrics)
        {
            lyricFlow.Clear();
            scroll.ScrollToStart();

            foreach (var t in lyrics)
            {
                lyricFlow.Add(new LyricInfoPiece(t)
                {
                    Action = l => mvisScreen.SeekTo(l.Time + 1)
                });
            }
        }

        private void refreshBeatmap(WorkingBeatmap working)
        {
            beatmapSetId = working.BeatmapSetInfo.ID;
            idText.Text = $"ID: {beatmapSetId}";
            cover.UpdateBackground(working);
        }
    }
}
