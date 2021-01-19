using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.Share
{
    public class ReadFromFileScreen : OsuScreen
    {
        private FileSelector selector;
        private TriangleButton readButton;
        private FillFlowContainer fillFlow;
        private LoadingLayer loading;
        private OsuScrollContainer scroll;

        [Resolved]
        private Storage storage { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var ex = new[] { ".bl" };

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.8f,
                    Height = 0.9f,
                    Masking = true,
                    CornerRadius = 12.5f,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.GreySeafoamDark
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.AutoSize)
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            selector = new FileSelector(validFileExtensions: ex)
                                            {
                                                RelativeSizeAxes = Axes.Both
                                            },
                                            scroll = new OsuScrollContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                ScrollbarVisible = false,
                                                Alpha = 0,
                                                Child = fillFlow = new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(10),
                                                    Margin = new MarginPadding { Vertical = 15 },
                                                }
                                            },
                                            loading = new LoadingLayer(true)
                                        }
                                    }
                                },
                                new Drawable[]
                                {
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Horizontal = 22 },
                                        Margin = new MarginPadding { Vertical = 22 },
                                        Child = readButton = new TriangleButton
                                        {
                                            Text = "开始读取",
                                            Anchor = Anchor.BottomCentre,
                                            Origin = Anchor.BottomCentre,
                                            RelativeSizeAxes = Axes.X,
                                            Height = 40,
                                            Width = 0.9f,
                                            Action = () => readFrom(selector.CurrentFile.Value?.FullName)
                                        },
                                    }
                                }
                            }
                        },
                    }
                }
            };
        }

        private void readFrom(string location)
        {
            if (location == null) return;

            readButton.Enabled.Value = false;

            selector.Hide();
            readButton.Hide();
            loading.Show();
            scroll.Show();

            try
            {
                //打开文件流
                var stream = storage.GetStream(location);

                //获取所有可用谱面
                var localBeatmaps = beatmapManager.GetAllUsableBeatmapSets(IncludedDetails.Minimal);
                var newBeatmaps = new List<int>();

                //创建StreamReader
                using (var reader = new StreamReader(stream))
                {
                    string currentLine; //当前行
                    int currentID; //当前ID

                    //如果当前行不为空
                    while ((currentLine = reader.ReadLine()) != null)
                    {
                        //将当前行解析为int
                        currentID = int.Parse(currentLine);

                        //在可用谱面中对比，是否已有该图
                        if (localBeatmaps.Any(b => b.OnlineBeatmapSetID == currentID)
                            || newBeatmaps.Any(id => id == currentID))
                            continue; //有，继续

                        //没有，添加进newBeatmaps
                        newBeatmaps.Add(currentID);
                    }
                }

                stream.Dispose();

                //完成后
                foreach (var id in newBeatmaps)
                {
                    Logger.Log($"发送有关{id}的请求");
                    var req = new GetBeatmapSetRequest(id);
                    req.Success += res => Schedule(() =>
                    {
                        var onlineBeatmap = res.ToBeatmapSet(rulesets);
                        fillFlow.Add(new GridBeatmapPanel(onlineBeatmap)
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        });
                    });

                    req.Failure += res => this.Exit();
                    api.Queue(req);
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, "尝试读取并列出谱面时发生了错误");
                throw;
            }

            readButton.Enabled.Value = true;
            loading.Hide();
        }
    }
}
