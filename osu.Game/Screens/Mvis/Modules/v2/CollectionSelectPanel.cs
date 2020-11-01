using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Collections;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Mvis.BottomBar.Buttons;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules.v2
{
    public class CollectionSelectPanel : VisibilityContainer
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        public readonly Bindable<BeatmapCollection> CurrentCollection = new Bindable<BeatmapCollection>();

        private Bindable<BeatmapCollection> SelectedCollection = new Bindable<BeatmapCollection>();
        private Bindable<CollectionPanel> SelectedPanel = new Bindable<CollectionPanel>();

        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private FillFlowContainer collectionsFillFlow;
        private CollectionPanel selectedpanel;
        private OsuScrollContainer collectionScroll;
        private CollectionInfo info;

        public CollectionSelectPanel()
        {
            RelativeSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 12.5f;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(0.9f, 0.8f);
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background5,
                },
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
                            Child = collectionsFillFlow = new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Spacing = new Vector2(10),
                                Padding = new MarginPadding(25),
                                Margin = new MarginPadding{Bottom = 40}
                            }
                        },
                        new BottomBarButton()
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Size = new Vector2(60, 40),
                            NoIcon = true,
                            ExtraDrawable = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Text = "刷新列表"
                            },
                            Action = () => RefreshCollections(),
                            Margin = new MarginPadding(5),
                        }
                    }
                },
                new Container
                {
                    Name = "收藏夹信息界面",
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.7f,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Children = new Drawable[]
                    {
                        info = new CollectionInfo()
                    }
                },
                new IconButton()
                {
                    Icon = FontAwesome.Solid.Times,
                    Margin = new MarginPadding(25),
                    Size = new Vector2(30),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Action = Hide
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            CurrentCollection.BindValueChanged(OnCurrentCollectionChanged);
            SelectedCollection.BindValueChanged(UpdateSelection);
            SelectedPanel.BindValueChanged(UpdateSelectedPanel);

            RefreshCollections();
        }

        private void OnCurrentCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            if (v.NewValue == null) return;

            info.UpdateCollection(v.NewValue, true);

            foreach (var d in collectionsFillFlow)
                if (d is CollectionPanel panel)
                    panel.Reset(true);

            if (selectedpanel != null)
                selectedpanel.state.Value = ActiveState.Active;
        }

        /// <summary>
        /// 当<see cref="CollectionPanel"/>被选中时执行
        /// </summary>
        private void UpdateSelection(ValueChangedEvent<BeatmapCollection> v)
        {
            if (v.NewValue == null) return;

            //如果选择的收藏夹为正在播放的收藏夹，则更新isCurrent为true
            if (v.NewValue == CurrentCollection.Value)
                info.UpdateCollection(v.NewValue, true);
            else
                info.UpdateCollection(v.NewValue, false);
        }

        private void UpdateSelectedPanel(ValueChangedEvent<CollectionPanel> v)
        {
            if (v.NewValue == null) return;
            selectedpanel?.Reset();
            selectedpanel = v.NewValue;
        }

        private void SearchForCurrentSelection()
        {
            foreach (var d in collectionsFillFlow)
            {
                if (d is CollectionPanel panel)
                    if (panel.collection == CurrentCollection.Value)
                        selectedpanel = panel;
            }
        }

        public void RefreshCollections()
        {
            var oldCollection = CurrentCollection.Value;

            //清空界面
            collectionsFillFlow.Clear();
            info.UpdateCollection(null, false);
            selectedpanel = null;

            SelectedCollection.Value = null;

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
                collectionsFillFlow.AddRange(collectionManager.Collections.Select(c => new CollectionPanel(c, MakeCurrentSelected)
                {
                    SelectedCollection = { BindTarget = this.SelectedCollection },
                    SelectedPanel = { BindTarget = this.SelectedPanel }
                }));
                collectionScroll.FadeIn(300);
            }

            //重新赋值
            CurrentCollection.Value = SelectedCollection.Value = oldCollection;

            //根据选中的收藏夹寻找对应的BeatmapPanel
            SearchForCurrentSelection();

            //CurrentCollection需要手动触发因为它和MvisScreen中的CurrentCollection绑在一起
            CurrentCollection.TriggerChange();
        }

        private void MakeCurrentSelected()
        {
            if (CurrentCollection.Value == SelectedCollection.Value)
                CurrentCollection.TriggerChange();
            else
                CurrentCollection.Value = SelectedCollection.Value;
        }

        protected override void PopIn()
        {
            this.FadeOut().Then().ScaleTo(0.9f)
                            .Then()
                            .ScaleTo(1, 300, Easing.OutExpo)
                            .FadeIn(500);
        }

        protected override void PopOut()
        {
            this.ScaleTo(0.9f, 300, Easing.OutExpo);
            this.FadeOut(300, Easing.OutExpo);
        }
    }
}