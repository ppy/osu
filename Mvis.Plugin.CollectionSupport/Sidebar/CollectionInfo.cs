using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using M.Resources.Localisation.LLin.Plugins;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osuTK;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class CollectionInfo : CompositeDrawable
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        private OsuSpriteText collectionName;
        private OsuSpriteText collectionBeatmapCount;
        private readonly Bindable<BeatmapCollection> collection = new Bindable<BeatmapCollection>();
        private readonly List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private BeatmapList beatmapList;
        private readonly BindableBool isCurrentCollection = new BindableBool();

        public CollectionInfo()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },
                    Content = new[]
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
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Vertical,
                                        Spacing = new Vector2(12),
                                        Padding = new MarginPadding { Horizontal = 35, Vertical = 25 },
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Children = new Drawable[]
                                        {
                                            collectionName = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 50),
                                                //RelativeSizeAxes = Axes.X,
                                                Text = CollectionStrings.NoCollectionSelected,
                                                //Truncate = true
                                            },
                                            collectionBeatmapCount = new OsuSpriteText
                                            {
                                                Font = OsuFont.GetFont(size: 38),
                                                //RelativeSizeAxes = Axes.X,
                                                Text = CollectionStrings.SelectOneFirst,
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    listContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    loadingSpinner = new LoadingSpinner(true)
                                    {
                                        Size = new Vector2(50)
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
        }

        private void OnCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            var c = v.NewValue;

            if (c == null)
            {
                clearInfo();
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
            collectionBeatmapCount.Text = CollectionStrings.SongCount(beatmapSets.Count);

            refreshBeatmapSetList();
        }

        private CancellationTokenSource refreshTaskCancellationToken;
        private Container listContainer;
        private LoadingSpinner loadingSpinner;

        private void refreshBeatmapSetList()
        {
            Task refreshTask;
            refreshTaskCancellationToken?.Cancel();
            refreshTaskCancellationToken = new CancellationTokenSource();

            beatmapList?.FadeOut(250).Then().Expire();
            loadingSpinner.Show();

            Task.Run(async () =>
            {
                refreshTask = Task.Run(() =>
                {
                    LoadComponentAsync(new BeatmapList(beatmapSets), newList =>
                    {
                        newList.IsCurrent.BindTo(isCurrentCollection);
                        beatmapList = newList;

                        listContainer.Add(newList);
                        newList.Show();
                        loadingSpinner.Hide();
                    }, refreshTaskCancellationToken.Token);
                }, refreshTaskCancellationToken.Token);

                await refreshTask.ConfigureAwait(true);
            });
        }

        public void UpdateCollection(BeatmapCollection collection, bool isCurrent)
        {
            if (collection != this.collection.Value && beatmapList != null)
            {
                beatmapList.IsCurrent.UnbindAll();
                beatmapList.IsCurrent.Value = false;
            }

            //设置当前选择是否为正在播放的收藏夹
            isCurrentCollection.Value = isCurrent;

            //将当前收藏夹设为collection
            this.collection.Value = collection;
        }

        private void clearInfo()
        {
            beatmapSets.Clear();
            beatmapList.ClearList();
            collectionName.Text = CollectionStrings.NoCollectionSelected;
            collectionBeatmapCount.Text = CollectionStrings.SelectOneFirst;
        }
    }
}
