using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.Share
{
    public class ShareBeatmapDetailArea : BeatmapDetailArea
    {
        public readonly Action AddNewPiece;

        protected override BeatmapDetailAreaTabItem[] CreateTabItems() => new BeatmapDetailAreaTabItem[]
        {
            new VoidTabItem()
        };

        private readonly BindableList<BeatmapSetInfo> list;
        private readonly DrawableBeatmapList drawableList;

        public ShareBeatmapDetailArea(BindableList<BeatmapSetInfo> list, Action action = null)
        {
            this.list = list;
            AddNewPiece = action;
            Child = new GridContainer
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
                        drawableList = new DrawableBeatmapList
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    new Drawable[]
                    {
                        new TriangleButton
                        {
                            Size = new Vector2(350, 40),
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Action = () => AddNewPiece?.Invoke(),
                            Text = "添加该谱面到列表中",
                            Margin = new MarginPadding { Vertical = 22 }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            drawableList.Items.BindTo(list);
        }

        private class VoidTabItem : BeatmapDetailAreaTabItem
        {
            public override string Name => "";
        }

        public class DrawableBeatmapList : OsuRearrangeableListContainer<BeatmapSetInfo>
        {
            [Resolved]
            private BeatmapManager manager { get; set; }

            protected override OsuRearrangeableListItem<BeatmapSetInfo> CreateOsuDrawable(BeatmapSetInfo item)
            {
                return new ShareBeatmapPiece(item, manager.GetWorkingBeatmap(item.Beatmaps.First()))
                {
                    RemoveItem = removeItem
                };
            }

            protected override FillFlowContainer<RearrangeableListItem<BeatmapSetInfo>> CreateListFillFlowContainer() => new FillFlowContainer<RearrangeableListItem<BeatmapSetInfo>>
            {
                Spacing = new Vector2(5),
                Padding = new MarginPadding { Right = 22, Vertical = 22 },
                LayoutDuration = 300,
                LayoutEasing = Easing.OutQuint
            };

            private void removeItem(BeatmapSetInfo b)
            {
                Items.Remove(b);
            }
        }
    }
}
