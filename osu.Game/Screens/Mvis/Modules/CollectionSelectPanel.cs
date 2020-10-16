using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Collections;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Mvis.Modules
{
    //TODO: 当做Overlay从Mvis调用
    public class CollectionSelectPanel : VisibilityContainer
    {
        [Resolved]
        private CollectionManager collectionManager { get; set; }

        public readonly Bindable<BeatmapCollection> CurrentCollection = new Bindable<BeatmapCollection>();

        private Bindable<BeatmapCollection> SelectedCollection = new Bindable<BeatmapCollection>();

        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue1);
        private FillFlowContainer collectionsFillFlow;
        private OsuSpriteText titleText;
        private Container contentContainer;
        private CollectionPill selectedPill;

        public CollectionSelectPanel()
        {
            RelativeSizeAxes = Axes.X;
            Height = 500;
            Child = contentContainer = new Container
            {
                Masking = true,
                CornerRadius = 25f,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background5,
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        RowDimensions = new Dimension[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Distributed),
                            new Dimension(GridSizeMode.AutoSize),
                        },
                        Content = new[]
                        {
                            new[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Name = "Header",
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background4,
                                        },
                                        titleText = new OsuSpriteText
                                        {
                                            Text = "选择收藏夹",
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                            Margin = new MarginPadding(10),
                                            Font = OsuFont.GetFont(size: 25)
                                        }
                                    }
                                },
                            },
                            new Drawable[]
                            {
                                new OsuScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = collectionsFillFlow = new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Y,
                                        RelativeSizeAxes = Axes.X,
                                        Spacing = new Vector2(10),
                                        Padding = new MarginPadding(10)
                                    }
                                }
                            },
                            new[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = colourProvider.Background4,
                                        },
                                        new TriangleButton()
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Width = 0.5f,
                                            Margin = new MarginPadding(15),
                                            Text = "选中该收藏夹",
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Action = () =>
                                            {
                                                CurrentCollection.Value = SelectedCollection.Value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            AddCollections();

            CurrentCollection.BindValueChanged(OnCurrentCollectionChanged);
            SelectedCollection.BindValueChanged(UpdateSelection);
        }

        private void OnCurrentCollectionChanged(ValueChangedEvent<BeatmapCollection> v)
        {
            titleText.Text = string.IsNullOrEmpty(v.NewValue?.Name.Value) ? "选择收藏夹" : v.NewValue.Name.Value;

            foreach(var d in collectionsFillFlow)
            {
                if ( d is CollectionPill pill )
                {
                    pill.InActive();
                    pill.IsCurrent = false;
                }
            }

            selectedPill.Active();
        }

        private void UpdateSelection(ValueChangedEvent<BeatmapCollection> v)
        {
            foreach(var d in collectionsFillFlow)
            {
                if ( d is CollectionPill pill )
                {
                    if (v.NewValue == pill.collection )
                    {
                        //如果pill是被选中的，则调用Selected并将selectedPill设置为这个
                        pill.OnSelect();
                        selectedPill = pill;
                    }
                    else
                    {
                        if ( !pill.IsCurrent )
                            pill.InActive();
                    }
                }
            }
        }

        private void AddCollections()
        {
            collectionsFillFlow.Clear();

            foreach (var collection in collectionManager.Collections)
            {
                collectionsFillFlow.Add(new CollectionPill(collection)
                {
                    SelectedCollection = {BindTarget = this.SelectedCollection}
                });
            }
        }

        protected override void PopIn()
        {
            this.FadeIn();
        }

        protected override void PopOut()
        {
            this.FadeOut();
        }

        protected class CollectionPill : CircularContainer
        {
            private Box flashBox;
            public readonly BeatmapCollection collection;
            public bool selected = false;
            public bool IsCurrent = false;

            public Bindable<BeatmapCollection> SelectedCollection = new Bindable<BeatmapCollection>();

            public CollectionPill(BeatmapCollection collection)
            {
                RelativeSizeAxes = Axes.X;
                Height = 35;
                Masking = true;
                BorderColour = Colour4.White;

                this.collection = collection;

                Children = new Drawable[]
                {
                    new Box
                    {
                        Colour = Colour4.Black.Opacity(0.6f),
                        RelativeSizeAxes = Axes.Both
                    },
                    new OsuSpriteText
                    {
                        Text = collection?.Name.Value ?? "???",
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        Margin = new MarginPadding{Left = 20}
                    },
                    flashBox = new Box
                    {
                        Colour = Colour4.White.Opacity(0.1f),
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0
                    }
                };
            }

            protected override bool OnClick(ClickEvent e)
            {
                SelectedCollection.Value = collection;
                return base.OnClick(e);
            }

            protected override bool OnHover(HoverEvent e)
            {
                if ( !selected )
                    this.BorderThickness = 1.5f;

                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                if ( !selected )
                    this.BorderThickness = 0;

                base.OnHoverLost(e);
            }

            public void Active()
            {
                OnSelect();
                IsCurrent = true;
                BorderColour = Color4Extensions.FromHex(@"88b300");
            }

            public void OnSelect()
            {
                selected = true;
                this.BorderThickness = 3;
            }

            public void UnSelect()
            {
                selected = false;
                this.BorderThickness = 0;
            }

            public void InActive()
            {
                selected = false;
                IsCurrent = false;
                BorderColour = Colour4.White;
                this.BorderThickness = 0;
            }
        }
    }
}