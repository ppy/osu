using System;
using System.Collections.Generic;
using System.Linq;
using M.Resources.Localisation.LLin.Plugins;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.LLin;
using osu.Game.Screens.LLin.Misc;
using osuTK;

namespace Mvis.Plugin.CollectionSupport.Sidebar
{
    public class CollectionPanel : CompositeDrawable
    {
        ///<summary>
        ///判断该panel所显示的BeatmapCollection
        ///</summary>
        public readonly BeatmapCollection Collection;

        ///<summary>
        ///用于触发<see cref="CollectionPluginPage"/>的SelectedCollection变更
        ///</summary>
        public Bindable<BeatmapCollection> SelectedCollection = new Bindable<BeatmapCollection>();

        public Bindable<CollectionPanel> SelectedPanel = new Bindable<CollectionPanel>();

        private readonly List<BeatmapSetInfo> beatmapSets = new List<BeatmapSetInfo>();

        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private CustomColourProvider colourProvider { get; set; }

        private OsuSpriteText collectionName;
        private OsuSpriteText collectionBeatmapCount;
        private OsuScrollContainer thumbnailScroll;
        private readonly Action doubleClick;

        public Bindable<ActiveState> State = new Bindable<ActiveState>();
        private Box stateBox;

        /// <summary>
        /// ...
        /// </summary>
        /// <param name="c">要设定的<see cref="BeatmapCollection"/></param>
        /// <param name="doubleClickAction">双击(再次选中)时执行的动作</param>
        public CollectionPanel(BeatmapCollection c, Action doubleClickAction)
        {
            BorderColour = Colour4.White;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 10f;
            Alpha = 0;

            Collection = c;
            doubleClick = doubleClickAction;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            sortBeatmapCollection();

            WorkingBeatmap targetBeatmap = beatmaps.GetWorkingBeatmap(beatmapSets.FirstOrDefault()?.Beatmaps.First());

            AddRangeInternal(new Drawable[]
            {
                new BeatmapCover(targetBeatmap)
                {
                    BackgroundBox = false,
                    TimeBeforeWrapperLoad = 0
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientVertical(
                        Color4Extensions.FromHex("#111").Opacity(0.6f),
                        Color4Extensions.FromHex("#111")
                    ),
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(15),
                    Spacing = new Vector2(15),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Name = "标题容器",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Children = new Drawable[]
                            {
                                new CircularContainer
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    Width = 5,
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Masking = true,
                                    Child = stateBox = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    }
                                },
                                new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Direction = FillDirection.Vertical,
                                    Spacing = new Vector2(4),
                                    Padding = new MarginPadding { Left = 15 },
                                    Children = new Drawable[]
                                    {
                                        collectionName = new OsuSpriteText
                                        {
                                            Font = OsuFont.GetFont(size: 30),
                                            //RelativeSizeAxes = Axes.X,
                                            Text = "???",
                                            //Truncate = true
                                        },
                                        collectionBeatmapCount = new OsuSpriteText
                                        {
                                            //RelativeSizeAxes = Axes.X,
                                            Text = "???"
                                        }
                                    }
                                }
                            }
                        },
                        thumbnailScroll = new ThumbnailScrollContainer(Direction.Horizontal)
                        {
                            Name = "谱面图标容器",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new BeatmapThumbnailFlow(beatmapSets)
                        }
                    }
                },
                new HoverClickSounds()
            });

            thumbnailScroll.ScrollContent.RelativeSizeAxes = Axes.None;
            thumbnailScroll.ScrollContent.AutoSizeAxes = Axes.Both;

            State.Value = beatmapSets.Count > 0
                ? ActiveState.Idle
                : ActiveState.Disabled;

            collectionName.Text = Collection.Name.Value;
            collectionBeatmapCount.Text = CollectionStrings.SongCount(beatmapSets.Count);

            State.BindValueChanged(onStateChanged, true);

            if (State.Value != ActiveState.Disabled)
                colourProvider.HueColour.BindValueChanged(_ => State.TriggerChange());
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            this.FadeIn(300);
        }

        private void onStateChanged(ValueChangedEvent<ActiveState> v)
        {
            if (v.NewValue >= ActiveState.Selected)
            {
                AutoSizeDuration = 400;
                AutoSizeEasing = Easing.OutQuint;
                thumbnailScroll.Show();
            }
            else
                thumbnailScroll.Hide();

            switch (v.NewValue)
            {
                case ActiveState.Active:
                    BorderThickness = 3f;
                    BorderColour = colourProvider.Highlight1;
                    stateBox.FadeColour(colourProvider.Highlight1, 300, Easing.OutQuint);
                    break;

                case ActiveState.Disabled:
                    BorderThickness = 0f;
                    Colour = Color4Extensions.FromHex("#555");
                    stateBox.FadeColour(Colour4.Gray, 300, Easing.OutQuint);
                    thumbnailScroll.Hide();
                    break;

                case ActiveState.Selected:
                    BorderThickness = 3f;
                    BorderColour = colourProvider.Light2;
                    stateBox.FadeColour(colourProvider.Light2, 300, Easing.OutQuint);
                    break;

                default:
                case ActiveState.Idle:
                    BorderThickness = 0f;
                    BorderColour = Colour4.White;
                    stateBox.FadeColour(Colour4.White, 300, Easing.OutQuint);
                    break;
            }
        }

        private void sortBeatmapCollection()
        {
            //From CollectionHelper.cs
            foreach (var item in Collection.Beatmaps)
            {
                //获取当前BeatmapSet
                var currentSet = item.BeatmapSet;

                //进行比对，如果beatmapList中不存在，则添加。
                if (!beatmapSets.Contains(currentSet))
                    beatmapSets.Add(currentSet);
            }
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (State.Value == ActiveState.Idle)
                BorderThickness = 1.5f;

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (State.Value == ActiveState.Idle)
                BorderThickness = 0f;

            base.OnHoverLost(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (State.Value == ActiveState.Disabled)
                return base.OnClick(e);

            //如果已经被选中了，则触发双击
            if (State.Value == ActiveState.Selected)
            {
                doubleClick?.Invoke();
                State.Value = ActiveState.Active;

                return base.OnClick(e);
            }

            //使SelectedCollection的值变为collection, 从而出触发CollectionSelectPanel的UpdateSelection
            SelectedCollection.Value = Collection;
            SelectedPanel.Value = this;

            if (State.Value != ActiveState.Active)
                State.Value = ActiveState.Selected;

            return base.OnClick(e);
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset(bool force = false)
        {
            if ((State.Value != ActiveState.Disabled && State.Value != ActiveState.Active)
                || (State.Value != ActiveState.Disabled && force))
            {
                State.Value = ActiveState.Idle;
            }
        }

        private class BeatmapThumbnailFlow : FillFlowContainer
        {
            [Resolved]
            private BeatmapManager beatmaps { get; set; }

            private readonly List<BeatmapSetInfo> beatmapSetList;

            public BeatmapThumbnailFlow(List<BeatmapSetInfo> list)
            {
                beatmapSetList = list;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5);
                AutoSizeAxes = Axes.Both;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                addBeatmapThumbnails();
            }

            private void addBeatmapThumbnails()
            {
                short collections = 0;
                const short limit = 10;

                foreach (var c in beatmapSetList)
                {
                    collections++;

                    var b = beatmaps.GetWorkingBeatmap(c.Beatmaps.First());
                    string tooltip = $"{b.Metadata.ArtistUnicode ?? b.Metadata.Artist}"
                                     + " - "
                                     + $"{b.Metadata.TitleUnicode ?? b.Metadata.Title}";

                    if (collections <= limit)
                    {
                        Add(new TooltipContainer
                        {
                            Size = new Vector2(40),
                            Masking = true,
                            CornerRadius = 7.25f,
                            TooltipText = tooltip,
                            Child = new BeatmapCover(b)
                        });
                        continue;
                    }

                    TooltipContainer t = Children.Last() as TooltipContainer;
                    int remaining = beatmapSetList.Count - limit;

                    t.AddRange(new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.Black.Opacity(0.7f),
                        },
                        new OsuSpriteText
                        {
                            Text = $"+{remaining}",
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    });
                    break;
                }
            }
        }

        private class ThumbnailScrollContainer : OsuScrollContainer
        {
            protected override bool OnMouseDown(MouseDownEvent e)
            {
                return false;
            }

            public ThumbnailScrollContainer(Direction scrollDirection)
                : base(scrollDirection)
            {
                ScrollbarVisible = false;
                Scrollbar.Width = 0;
            }
        }
    }

    public enum ActiveState
    {
        Disabled,
        Idle,
        Selected,
        Active,
    }
}
