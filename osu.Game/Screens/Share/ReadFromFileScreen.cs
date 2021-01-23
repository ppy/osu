using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Screens.Share
{
    public class ReadFromFileScreen : OsuScreen
    {
        private FileSelector selector;
        private FillFlowContainer fillFlow;
        private LoadingIndicator loading;
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

        private readonly BindableList<GetBeatmapSetRequest> requests = new BindableList<GetBeatmapSetRequest>();
        private int apiFailures;
        private int beatmapsIgnored;
        private OsuSpriteText tipText;

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
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                RelativeSizeAxes = Axes.Both
                                            },
                                            scroll = new OsuScrollContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                ScrollbarVisible = false,
                                                Alpha = 0,
                                                ScrollContent = { Anchor = Anchor.Centre, Origin = Anchor.Centre },
                                                Child = fillFlow = new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    Spacing = new Vector2(10),
                                                    Margin = new MarginPadding { Vertical = 15 },
                                                }
                                            },
                                            loading = new LoadingIndicator()
                                        }
                                    }
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Horizontal = 22 },
                                        Margin = new MarginPadding { Vertical = 22 },
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(5),
                                        LayoutDuration = 300,
                                        LayoutEasing = Easing.OutQuint,
                                        Children = new Drawable[]
                                        {
                                            new TriangleButton
                                            {
                                                Text = "重新选取",
                                                Anchor = Anchor.BottomCentre,
                                                Origin = Anchor.BottomCentre,
                                                RelativeSizeAxes = Axes.X,
                                                Height = 40,
                                                Width = 0.9f,
                                                Action = () =>
                                                {
                                                    cancelAllRequests(true);
                                                    tipText.Text = string.Empty;
                                                    selector.CurrentFile.Value = null;
                                                    showSelector();
                                                }
                                            },
                                            tipText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.BottomCentre,
                                                Origin = Anchor.BottomCentre,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };

            selector.CurrentFile.BindValueChanged(v =>
            {
                readFrom(v.NewValue?.FullName);
            });

            requests.BindCollectionChanged(onRequestsChanged);
        }

        private void onRequestsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (requests.Count > 0)
                loading.Show();
            else
                loading.Hide();
        }

        private void toggleSelectorAndScroll(bool showSelector, bool clearScroll)
        {
            if (showSelector)
            {
                selector.FadeIn(200);

                if (clearScroll)
                {
                    scroll.FadeTo(0.01f, 200).OnComplete(_ =>
                    {
                        fillFlow.Clear();
                        scroll.FadeOut();
                    });
                }
                else
                    scroll.FadeOut(200);
            }
            else
            {
                selector.FadeOut(300);
                scroll.FadeIn(300);
            }
        }

        private void showSelector() => toggleSelectorAndScroll(true, true);
        private void hideSelector() => toggleSelectorAndScroll(false, false);

        private void readFrom(string location)
        {
            if (location == null) return;

            hideSelector();
            cancelAllRequests(true);
            apiFailures = 0;
            beatmapsIgnored = 0;

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
                        {
                            beatmapsIgnored++;
                            continue; //有，继续
                        }

                        //没有，添加进newBeatmaps
                        newBeatmaps.Add(currentID);
                    }
                }

                stream.Dispose();

                //完成后
                foreach (var id in newBeatmaps)
                {
                    //创建请求
                    var req = new GetBeatmapSetRequest(id);

                    //向列表添加该请求
                    requests.Add(req);

                    //当请求成功后：
                    req.Success += onRequestSuccess;

                    req.Success += _ => requests.Remove(req);

                    //请求失败时同样从列表中移除，并增加apiFailures计数
                    //bug: 在调用req.Cancel()后仍然能触发req.Success
                    req.Failure += _ => requests.Remove(req);

                    req.Failure += onRequestFail;

                    api.Queue(req);
                }

                tipText.Text = $"API请求失败了{apiFailures}次，忽略了{beatmapsIgnored}个已有谱面";
            }
            catch (Exception e)
            {
                Logger.Error(e, "尝试读取并列出谱面时发生了错误");
                cancelAllRequests();
                showSelector();
                selector.CurrentFile.Value = null;
            }
        }

        private void onRequestSuccess(APIBeatmapSet res)
        {
            Schedule(() =>
            {
                //创建(BeatmapSetInfo)onlineBeatmap
                //调用APIBeatmapSet为其赋值
                var onlineBeatmap = res.ToBeatmapSet(rulesets);

                //向fillFlow添加面板
                fillFlow.Add(new GridBeatmapPanel(onlineBeatmap)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre
                });
            });
        }

        private void onRequestFail(Exception e)
        {
            apiFailures++;

            //设置提示文字
            tipText.Text = $"API请求失败了{apiFailures}次，忽略了{beatmapsIgnored}个已有谱面";
        }

        private void cancelAllRequests(bool clearList = false)
        {
            foreach (var req in requests)
            {
                //fix: 请求失败后仍然可以触发Success和Failure内容
                req.Failure -= onRequestFail;
                req.Success -= onRequestSuccess;

                req.Cancel();
            }

            if (clearList) requests.Clear();
        }

        public override bool OnExiting(IScreen next)
        {
            cancelAllRequests();
            return base.OnExiting(next);
        }

        private class LoadingIndicator : VisibilityContainer
        {
            private LoadingSpinner spinner;

            [Resolved]
            private OsuColour colours { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                RelativeSizeAxes = Axes.Both;
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colours.GreySeafoamDark
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Children = new Drawable[]
                        {
                            spinner = new LoadingSpinner
                            {
                                Size = new Vector2(50),
                                Margin = new MarginPadding { Bottom = 10 }
                            },
                            new OsuSpriteText
                            {
                                Text = "处理中",
                                Font = OsuFont.GetFont(size: 25),
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            },
                            new OsuSpriteText
                            {
                                Text = "这可能需要一些时间...",
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                            }
                        }
                    }
                };
                spinner.Show();
            }

            protected override void PopIn()
            {
                this.FadeIn(300);
            }

            protected override void PopOut()
            {
                this.FadeOut(300);
            }
        }
    }
}
