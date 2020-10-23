using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class CollectionInfo : Container
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private Bindable<WorkingBeatmap> working { get; set; }

        private Box flashBox;
        private OsuSpriteText collectionName;
        private OsuSpriteText collectionBeatmapCount;
        private Bindable<BeatmapCollection> collection = new Bindable<BeatmapCollection>();
        private List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();
        private BeatmapCover cover;
        private BeatmapPiece currentPiece;

        [Cached]
        public readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private FillFlowContainer beatmapFillFlow;
        private OsuScrollContainer beatmapScroll;
        private bool isCurrentCollection;

        public CollectionInfo()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colourProvider.Background3,
                    RelativeSizeAxes = Axes.Both
                },
                cover = new BeatmapCover(null)
                {
                    BackgroundBox = false
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background3.Opacity(0.5f)
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new Dimension[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    },
                    Content = new []
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                Name = "标题容器",
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                AutoSizeDuration = 300,
                                AutoSizeEasing = Easing.OutQuint,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background3.Opacity(0.5f)
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(12),
                                        Padding = new MarginPadding{ Horizontal = 35, Vertical = 25 },
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Children = new Drawable[]
                                        {
                                            collectionName = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 50),
                                                RelativeSizeAxes = Axes.X,
                                                Text = "未选择收藏夹"
                                            },
                                            collectionBeatmapCount = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 38),
                                                RelativeSizeAxes = Axes.X,
                                                Text = "请先选择一个收藏夹!"
                                            }
                                        }
                                    },
                                    flashBox = new Box
                                    {
                                        Height = 3,
                                        RelativeSizeAxes = Axes.X,
                                        Colour = Colour4.Gold,
                                        Anchor = Anchor.BottomLeft,
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding{Left = 35, Right = 20, Vertical = 20},
                                Child = beatmapScroll = new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    RightMouseScrollbar = true,
                                    Child = beatmapFillFlow = new FillFlowContainer
                                    {
                                        Padding = new MarginPadding{Right = 15},
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Spacing = new Vector2(5)
                                    }
                                }
                            },
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            collection.BindValueChanged(OnCollectionChanged);
            working.BindValueChanged(OnBeatmapChanged);
        }

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            var c = v.NewValue;

            currentPiece = null;
            if (c == null)
            {
                ClearInfo();
                return;
            }

            beatmapSets.Clear();
            //From CollectionHelper.cs
            foreach (var item in c.Beatmaps)
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;

                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapSets.Contains(currentSet))
                    beatmapSets.Add(currentSet);
            }

            collectionName.Text = c.Name.Value;
            collectionBeatmapCount.Text = $"{beatmapSets.Count}首歌曲, {c.Beatmaps.Count}个谱面";

            cover.updateBackground(beatmaps.GetWorkingBeatmap(beatmapSets.ElementAt(0).Beatmaps.First()));
            flashBox.FlashColour(Colour4.White, 1000, Easing.OutQuint);

            beatmapFillFlow.Clear();
            beatmapFillFlow.AddRange(beatmapSets.Select(s => new BeatmapPiece(beatmaps.GetWorkingBeatmap(s.Beatmaps.First()))));

            working.TriggerChange();
        }

        private void OnBeatmapChanged(ValueChangedEvent<WorkingBeatmap> v)
        {
            currentPiece?.InActive();

            foreach (var d in beatmapFillFlow)
            {
                if (d is BeatmapPiece piece)
                    if (piece.beatmap.BeatmapSetInfo.Hash == v.NewValue.BeatmapSetInfo.Hash)
                    {
                        currentPiece = piece;
                        piece.MakeActive();
                        break;
                    }
            }

            this.Delay(5).Schedule(ScrollToCurrentBeatmap);
        }

        private void ScrollToCurrentBeatmap()
        {
            if ( !isCurrentCollection )
            {
                beatmapScroll.ScrollToStart();
                return;
            }

            if ( currentPiece == null ) return;

            float distance = 0;
            var index = beatmapFillFlow.IndexOf(currentPiece);

            //如果是第一个，那么滚动到头
            if (index == 0)
            {
                beatmapScroll.ScrollToStart();
                return;
            }
            else
            {
                distance = (index - 1) * 85;

                //如果滚动范围超出了beatmapFillFlow的高度，那么滚动到尾
                //n个piece, n-1个间隔
                if (distance + beatmapScroll.DrawHeight > (beatmapFillFlow.Count * 85 - 5))
                {
                    beatmapScroll.ScrollToEnd();
                    return;
                }
            }

            beatmapScroll.ScrollTo(distance);
        }

        public void UpdateCollection(BeatmapCollection collection, bool isCurrent)
        {
            if (!isCurrent) flashBox.FadeColour(Colour4.Gold, 300, Easing.OutQuint);
            else flashBox.FadeColour(Color4Extensions.FromHex("#88b300"), 300, Easing.OutQuint);

            //设置当前选择是否为正在播放的收藏夹
            isCurrentCollection = isCurrent;

            //将当前收藏夹设为collection
            this.collection.Value = collection;

            //更新BeatmapPiece
            foreach (var d in beatmapFillFlow)
                if (d is BeatmapPiece p)
                    p.isCurrent = isCurrent;

            currentPiece?.Active.TriggerChange();
        }

        private void ClearInfo()
        {
            cover.updateBackground(null);

            beatmapSets.Clear();
            beatmapFillFlow.Clear();
            collectionName.Text = "未选择收藏夹";
            collectionBeatmapCount.Text = "请先选择一个收藏夹!";

            flashBox.FadeColour(Colour4.Gold);
        }
    }
}