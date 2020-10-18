using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
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

        private Box flashBox;
        private OsuSpriteText collectionName;
        private OsuSpriteText collectionBeatmapCount;
        private Bindable<BeatmapCollection> collection = new Bindable<BeatmapCollection>();
        private List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();
        private BeatmapCover cover;

        [Cached]
        public readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private FillFlowContainer beatmapFillFlow;

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
                cover = new BeatmapCover(null),
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
                                        Padding = new MarginPadding(35),
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
                            },
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding{Horizontal = 35, Vertical = 20},
                                Child = new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = beatmapFillFlow = new FillFlowContainer
                                    {
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

            collection.BindValueChanged(OnCollectionChanged);
        }

        private void AddBeatmapToFillFlow()
        {
            beatmapFillFlow.Clear();

            foreach (var b in beatmapSets)
            {
                beatmapFillFlow.Add(new BeatmapPiece( beatmaps.GetWorkingBeatmap(b.Beatmaps.First()) ));
            }
        }

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            var c = v.NewValue;

            if ( c == null )
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

            AddBeatmapToFillFlow();
        }

        public void UpdateCollection(BeatmapCollection collection, bool isCurrent)
        {
            if ( ! isCurrent )  flashBox.FadeColour( Colour4.Gold, 300, Easing.OutQuint );
            else    flashBox.FadeColour( Color4Extensions.FromHex("#88b300"), 300, Easing.OutQuint );

            this.collection.Value = collection;
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

        private class BeatmapPiece : Container
        {
            [Resolved]
            private OverlayColourProvider colourProvider { get; set; }

            private WorkingBeatmap beatmap;

            public BeatmapPiece(WorkingBeatmap b)
            {
                Masking = true;
                CornerRadius = 12.5f;
                BorderThickness = 3f;
                RelativeSizeAxes = Axes.X;
                Height = 80;
                
                beatmap = b;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                BorderColour = colourProvider.Highlight1;

                InternalChildren = new Drawable[]
                {
                    new BeatmapCover(beatmap),
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3.Opacity(0.5f)
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.5f,
                        Colour = ColourInfo.GradientHorizontal(
                            colourProvider.Background3,
                            colourProvider.Background3.Opacity(0)
                        )
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding{Left = 15},
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold)
                            },
                            new OsuSpriteText
                            {
                                Text = beatmap.Metadata.ArtistUnicode ?? beatmap.Metadata.Artist,
                                Font = OsuFont.GetFont(weight: FontWeight.Bold)
                            }
                        }
                    }
                };
            }
        }
    }
}