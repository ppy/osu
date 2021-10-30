using System.Collections.Specialized;
using System.Linq;
using M.Resources.Localisation.LLin.Plugins;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Collections;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Plugins;
using osu.Game.Screens.LLin.Plugins.Types;
using osuTK;
using osuTK.Input;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class CollectionPluginPage : PluginSidebarPage
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        [Resolved]
        private IImplementLLin mvisScreen { get; set; }

        private readonly CollectionHelper collectionHelper;

        private readonly Bindable<BeatmapCollection> selectedCollection = new Bindable<BeatmapCollection>();
        private readonly Bindable<CollectionPanel> selectedPanel = new Bindable<CollectionPanel>();

        private FillFlowContainer<CollectionPanel> collectionsFillFlow;
        private CollectionPanel selectedpanel;
        private CollectionPanel prevPanel;
        private OsuScrollContainer collectionScroll;
        private CollectionInfo info;

        public CollectionPluginPage(LLinPlugin plugin)
            : base(plugin)
        {
            Icon = FontAwesome.Solid.Check;
            RelativeSizeAxes = Axes.Both;
            collectionHelper = (CollectionHelper)plugin;
        }

        private DependencyContainer dependencies;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

        public override IPluginFunctionProvider GetFunctionEntry() => new CollectionFunctionProvider(this);
        public override Key ShortcutKey => Key.Period;

        [BackgroundDependencyLoader]
        private void load()
        {
            dependencies.Cache(collectionHelper);

            Children = new Drawable[]
            {
                new Container
                {
                    Name = "收藏夹选择界面",
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.3f,
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    Children = new Drawable[]
                    {
                        collectionScroll = new OsuScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = collectionsFillFlow = new FillFlowContainer<CollectionPanel>
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Spacing = new Vector2(10),
                                Padding = new MarginPadding(25)
                            }
                        }
                    }
                },
                info = new CollectionInfo
                {
                    Name = "收藏夹信息界面",
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.7f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                }
            };

            collectionManager.Collections.CollectionChanged += triggerRefresh;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            collectionHelper.CurrentCollection.BindValueChanged(OnCurrentCollectionChanged);
            selectedCollection.BindValueChanged(updateSelection);
            selectedPanel.BindValueChanged(updateSelectedPanel);

            RefreshCollectionList();
            mvisScreen.Resuming += RefreshCollectionList;
        }

        private void OnCurrentCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            if (v.NewValue == null) return;

            info.UpdateCollection(v.NewValue, true);

            searchForCurrentSelection();
        }

        /// <summary>
        /// 当<see cref="CollectionPanel"/>被选中时执行
        /// </summary>
        private void updateSelection(ValueChangedEvent<BeatmapCollection> v)
        {
            if (v.NewValue == null) return;

            //如果选择的收藏夹为正在播放的收藏夹，则更新isCurrent为true
            info.UpdateCollection(v.NewValue, v.NewValue == collectionHelper.CurrentCollection.Value);
        }

        private void updateSelectedPanel(ValueChangedEvent<CollectionPanel> v)
        {
            if (v.NewValue == null) return;

            selectedpanel?.Reset();
            selectedpanel = v.NewValue;
        }

        private void searchForCurrentSelection()
        {
            prevPanel?.Reset(true);

            foreach (var p in collectionsFillFlow)
            {
                if (p.Collection == collectionHelper.CurrentCollection.Value)
                    selectedpanel = prevPanel = p;
            }

            if (selectedpanel != null
                && collectionHelper.CurrentCollection.Value.Beatmaps.Count != 0)
                selectedpanel.State.Value = ActiveState.Active;
        }

        private void triggerRefresh(object sender, NotifyCollectionChangedEventArgs e)
            => RefreshCollectionList();

        public void RefreshCollectionList()
        {
            if (collectionHelper == null) return;

            var oldCollection = collectionHelper.CurrentCollection.Value;

            //清空界面
            collectionsFillFlow.Clear();
            info.UpdateCollection(null, false);
            selectedpanel = null;

            selectedCollection.Value = null;

            //如果收藏夹被删除，则留null
            if (!collectionManager.Collections.Contains(oldCollection))
                oldCollection = null;

            //如果收藏夹为0，则淡出sollectionScroll
            //否则，添加CollectionPanel
            if (collectionManager.Collections.Count == 0)
            {
                collectionScroll.FadeOut(300);
            }
            else
            {
                collectionsFillFlow.AddRange(collectionManager.Collections.Select(c => new CollectionPanel(c, makeCurrentSelected)
                {
                    SelectedCollection = { BindTarget = selectedCollection },
                    SelectedPanel = { BindTarget = selectedPanel }
                }));
                collectionScroll.FadeIn(300);
            }

            //重新赋值
            collectionHelper.CurrentCollection.Value = selectedCollection.Value = oldCollection;

            //根据选中的收藏夹寻找对应的BeatmapPanel
            searchForCurrentSelection();
        }

        private bool requestedOnce;

        private void makeCurrentSelected()
        {
            collectionHelper.CurrentCollection.Value = selectedCollection.Value;

            if (!requestedOnce)
            {
                requestedOnce = true;
                mvisScreen?.RequestAudioControl((CollectionHelper)Plugin, CollectionStrings.AudioControlRequest, null, null);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            //单线程下会有概率在这里报System.NullReferenceException
            if (collectionManager != null)
                collectionManager.Collections.CollectionChanged -= triggerRefresh;

            base.Dispose(isDisposing);
        }
    }
}
